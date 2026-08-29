/*
 * TrackpadBridge skeleton (macOS).
 * dlopen MultitouchSupport, register contact callback, emit primitives on Unix socket.
 * Do NOT map pan/orbit/zoom here — C# owns bindings via ModSettings.
 */
#include <stdio.h>

int main(int argc, char **argv) {
    (void)argc;
    (void)argv;
    /* TODO: MTDeviceCreateList, MTRegisterContactFrameCallback, socket serve */
    fprintf(stderr, "TrackpadBridge template — not implemented\n");
    return 1;
}
