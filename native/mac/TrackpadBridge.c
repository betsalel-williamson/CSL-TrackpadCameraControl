/*
 * TrackpadBridge (macOS) — MultitouchSupport → GestureFrame over a Unix socket.
 * Emits raw primitives only; C# owns pan/orbit/zoom bindings.
 *
 * Default socket: $TMPDIR/trackpad-camera-control.sock
 * Env override: TRACKPAD_BRIDGE_SOCKET
 */
#include "gesture_frame.h"

#include <CoreFoundation/CoreFoundation.h>
#include <dlfcn.h>
#include <errno.h>
#include <math.h>
#include <pthread.h>
#include <signal.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <time.h>
#include <unistd.h>

typedef struct {
    float x;
    float y;
} MTPoint;

typedef struct {
    MTPoint pos;
    MTPoint vel;
} MTVector;

typedef struct {
    int32_t frame;
    double timestamp;
    int32_t pathIndex;
    int32_t state;
    int32_t fingerID;
    int32_t handID;
    MTVector normalized;
    float size;
    int32_t zero1;
    float angle;
    float majorAxis;
    float minorAxis;
    MTVector absolute;
    int32_t zero2;
    int32_t zero3;
    float zDensity;
} MTTouch;

typedef void *MTDeviceRef;
typedef void (*MTContactCallbackFunction)(MTDeviceRef device, MTTouch *touches, int numTouches,
                                          double timestamp, int frame);

typedef CFMutableArrayRef (*MTDeviceCreateListFn)(void);
typedef void (*MTRegisterContactFrameCallbackFn)(MTDeviceRef, MTContactCallbackFunction);
typedef void (*MTDeviceStartFn)(MTDeviceRef, int);
typedef void (*MTDeviceStopFn)(MTDeviceRef);

static MTDeviceCreateListFn g_MTDeviceCreateList;
static MTRegisterContactFrameCallbackFn g_MTRegisterContactFrameCallback;
static MTDeviceStartFn g_MTDeviceStart;
static MTDeviceStopFn g_MTDeviceStop;
static int g_debug;

static pthread_mutex_t g_client_mu = PTHREAD_MUTEX_INITIALIZER;
static int g_client_fd = -1;
static int g_listen_fd = -1;
static volatile sig_atomic_t g_running = 1;

static int g_pinch_active;
static float g_last_distance = -1.f;

static void on_signal(int sig) {
    (void)sig;
    g_running = 0;
}

static int64_t now_ns(void) {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (int64_t)ts.tv_sec * 1000000000LL + (int64_t)ts.tv_nsec;
}

static void send_frame(const GestureFrame *frame) {
    pthread_mutex_lock(&g_client_mu);
    int fd = g_client_fd;
    if (fd >= 0) {
        ssize_t n = write(fd, frame, sizeof(*frame));
        if (n != (ssize_t)sizeof(*frame)) {
            close(fd);
            g_client_fd = -1;
            fprintf(stderr, "TrackpadBridge: client disconnected\n");
        }
    }
    pthread_mutex_unlock(&g_client_mu);
}

static float touch_distance(const MTTouch *a, const MTTouch *b) {
    float dx = a->normalized.pos.x - b->normalized.pos.x;
    float dy = a->normalized.pos.y - b->normalized.pos.y;
    return sqrtf(dx * dx + dy * dy);
}

static void contact_callback(MTDeviceRef device, MTTouch *touches, int numTouches, double timestamp,
                             int frame) {
    (void)device;
    (void)timestamp;
    (void)frame;

    if (g_debug) {
        fprintf(stderr, "TrackpadBridge: contacts=%d\n", numTouches);
    }

    GestureFrame out;
    memset(&out, 0, sizeof(out));
    out.magic = GESTURE_FRAME_MAGIC;
    out.version = GESTURE_FRAME_VERSION;
    out.timestampNs = now_ns();
    out.fingerCount = numTouches;

    if (numTouches < 2) {
        if (g_pinch_active) {
            out.phase = GesturePhase_Ended;
            out.pinchScaleDelta = 0.f;
            send_frame(&out);
            g_pinch_active = 0;
            g_last_distance = -1.f;
        }
        return;
    }

    float dist = touch_distance(&touches[0], &touches[1]);
    if (!g_pinch_active || g_last_distance < 0.f) {
        out.phase = GesturePhase_Began;
        out.pinchScaleDelta = 0.f;
        g_pinch_active = 1;
        g_last_distance = dist;
        send_frame(&out);
        return;
    }

    float delta = 0.f;
    if (g_last_distance > 1e-6f) {
        delta = (dist - g_last_distance) / g_last_distance;
    }
    g_last_distance = dist;

    out.phase = GesturePhase_Changed;
    out.pinchScaleDelta = delta;
    send_frame(&out);
}

static int load_multitouch(void) {
    void *lib =
        dlopen("/System/Library/PrivateFrameworks/MultitouchSupport.framework/MultitouchSupport",
               RTLD_LAZY);
    if (!lib) {
        fprintf(stderr, "TrackpadBridge: dlopen MultitouchSupport failed: %s\n", dlerror());
        return -1;
    }

    g_MTDeviceCreateList = (MTDeviceCreateListFn)dlsym(lib, "MTDeviceCreateList");
    g_MTRegisterContactFrameCallback =
        (MTRegisterContactFrameCallbackFn)dlsym(lib, "MTRegisterContactFrameCallback");
    g_MTDeviceStart = (MTDeviceStartFn)dlsym(lib, "MTDeviceStart");
    g_MTDeviceStop = (MTDeviceStopFn)dlsym(lib, "MTDeviceStop");

    if (!g_MTDeviceCreateList || !g_MTRegisterContactFrameCallback || !g_MTDeviceStart ||
        !g_MTDeviceStop) {
        fprintf(stderr, "TrackpadBridge: missing MultitouchSupport symbols\n");
        return -1;
    }
    return 0;
}

static int start_devices(void) {
    CFMutableArrayRef devices = g_MTDeviceCreateList();
    if (!devices) {
        fprintf(stderr, "TrackpadBridge: MTDeviceCreateList returned null\n");
        return -1;
    }

    CFIndex count = CFArrayGetCount(devices);
    if (count < 1) {
        fprintf(stderr, "TrackpadBridge: no multitouch devices\n");
        CFRelease(devices);
        return -1;
    }

    for (CFIndex i = 0; i < count; i++) {
        MTDeviceRef dev = (MTDeviceRef)CFArrayGetValueAtIndex(devices, i);
        g_MTRegisterContactFrameCallback(dev, contact_callback);
        g_MTDeviceStart(dev, 0);
    }

    fprintf(stderr, "TrackpadBridge: started %ld multitouch device(s)\n", (long)count);
    /* Keep array alive for device lifetimes while process runs. */
    return 0;
}

static void resolve_socket_path(char *out, size_t out_len) {
    const char *env = getenv("TRACKPAD_BRIDGE_SOCKET");
    if (env && env[0]) {
        snprintf(out, out_len, "%s", env);
        return;
    }
    const char *tmpdir = getenv("TMPDIR");
    if (!tmpdir || !tmpdir[0]) {
        tmpdir = "/tmp";
    }
    snprintf(out, out_len, "%strackpad-camera-control.sock", tmpdir);
}

static int serve_socket(const char *path) {
    unlink(path);

    int fd = socket(AF_UNIX, SOCK_STREAM, 0);
    if (fd < 0) {
        perror("socket");
        return -1;
    }

    struct sockaddr_un addr;
    memset(&addr, 0, sizeof(addr));
    addr.sun_family = AF_UNIX;
    if (strlen(path) >= sizeof(addr.sun_path)) {
        fprintf(stderr, "TrackpadBridge: socket path too long\n");
        close(fd);
        return -1;
    }
    snprintf(addr.sun_path, sizeof(addr.sun_path), "%s", path);

    if (bind(fd, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
        perror("bind");
        close(fd);
        return -1;
    }
    if (listen(fd, 1) < 0) {
        perror("listen");
        close(fd);
        return -1;
    }

    g_listen_fd = fd;
    fprintf(stderr, "TrackpadBridge: listening on %s\n", path);
    return 0;
}

static void *accept_loop(void *arg) {
    (void)arg;
    while (g_running) {
        int client = accept(g_listen_fd, NULL, NULL);
        if (client < 0) {
            if (!g_running) {
                break;
            }
            if (errno == EINTR) {
                continue;
            }
            perror("accept");
            break;
        }

        pthread_mutex_lock(&g_client_mu);
        if (g_client_fd >= 0) {
            close(g_client_fd);
        }
        g_client_fd = client;
        pthread_mutex_unlock(&g_client_mu);
        fprintf(stderr, "TrackpadBridge: client connected\n");
    }
    return NULL;
}

int main(int argc, char **argv) {
    (void)argc;
    (void)argv;

    signal(SIGINT, on_signal);
    signal(SIGTERM, on_signal);
    g_debug = getenv("TRACKPAD_BRIDGE_DEBUG") != NULL;

    if (load_multitouch() != 0) {
        return 1;
    }

    char path[512];
    resolve_socket_path(path, sizeof(path));
    if (serve_socket(path) != 0) {
        return 1;
    }

    if (start_devices() != 0) {
        close(g_listen_fd);
        unlink(path);
        return 1;
    }

    pthread_t accept_thread;
    if (pthread_create(&accept_thread, NULL, accept_loop, NULL) != 0) {
        perror("pthread_create");
        return 1;
    }

    fprintf(stderr, "TrackpadBridge: ready (pinch frames only for MVP)\n");
    while (g_running) {
        CFRunLoopRunInMode(kCFRunLoopDefaultMode, 0.25, true);
    }

    g_running = 0;
    if (g_listen_fd >= 0) {
        close(g_listen_fd);
        g_listen_fd = -1;
    }
    pthread_join(accept_thread, NULL);

    pthread_mutex_lock(&g_client_mu);
    if (g_client_fd >= 0) {
        close(g_client_fd);
        g_client_fd = -1;
    }
    pthread_mutex_unlock(&g_client_mu);
    unlink(path);
    fprintf(stderr, "TrackpadBridge: stopped\n");
    return 0;
}
