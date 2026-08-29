using System;
using System.Runtime.InteropServices;

namespace TrackpadCameraControl
{
    /// <summary>Dev-path client: Unix domain socket to TrackpadBridge (libc P/Invoke for Unity Mono).</summary>
    public sealed class IpcGestureSource : IGestureSource, IDisposable
    {
        private const int AfUnix = 1;
        private const int SockStream = 1;
        private const int SohnPathLen = 104;
        private const int SockAddrLen = 106; // sun_len + sun_family + sun_path[104]

        private readonly string _socketPath;
        private readonly object _gate = new object();
        private int _fd = -1;
        private readonly byte[] _buffer = new byte[GestureFrame.Size];
        private int _buffered;

        public IpcGestureSource(string socketPath = null)
        {
            _socketPath = socketPath ?? DefaultSocketPath();
        }

        public bool IsConnected
        {
            get
            {
                lock (_gate)
                {
                    return _fd >= 0;
                }
            }
        }

        public static string DefaultSocketPath()
        {
            var env = Environment.GetEnvironmentVariable("TRACKPAD_BRIDGE_SOCKET");
            if (!string.IsNullOrEmpty(env))
            {
                return env;
            }

            var tmp = Environment.GetEnvironmentVariable("TMPDIR");
            if (string.IsNullOrEmpty(tmp))
            {
                tmp = "/tmp/";
            }

            if (!tmp.EndsWith("/", StringComparison.Ordinal))
            {
                tmp += "/";
            }

            return tmp + "trackpad-camera-control.sock";
        }

        public void Connect()
        {
            lock (_gate)
            {
                DisconnectUnlocked();
                try
                {
                    int fd = socket(AfUnix, SockStream, 0);
                    if (fd < 0)
                    {
                        return;
                    }

                    byte[] addr = BuildSockAddrBytes(_socketPath);
                    var handle = GCHandle.Alloc(addr, GCHandleType.Pinned);
                    try
                    {
                        int rc = connect(fd, handle.AddrOfPinnedObject(), (uint)addr.Length);
                        if (rc != 0)
                        {
                            close(fd);
                            return;
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }

                    SetNonBlocking(fd);
                    _fd = fd;
                    _buffered = 0;
                }
                catch
                {
                    DisconnectUnlocked();
                }
            }
        }

        public void Disconnect()
        {
            lock (_gate)
            {
                DisconnectUnlocked();
            }
        }

        public bool TryDequeue(out GestureFrame frame)
        {
            frame = default;
            lock (_gate)
            {
                if (_fd < 0)
                {
                    return false;
                }

                try
                {
                    while (_buffered < GestureFrame.Size)
                    {
                        int n = ReadInto(_fd, _buffer, _buffered, GestureFrame.Size - _buffered);
                        if (n == 0)
                        {
                            DisconnectUnlocked();
                            return false;
                        }

                        if (n < 0)
                        {
                            int err = Marshal.GetLastWin32Error();
                            if (err == 35 || err == 11) // EAGAIN / EWOULDBLOCK
                            {
                                return false;
                            }

                            DisconnectUnlocked();
                            return false;
                        }

                        _buffered += n;
                    }

                    frame = BytesToFrame(_buffer);
                    _buffered = 0;
                    return frame.IsValid;
                }
                catch
                {
                    DisconnectUnlocked();
                    return false;
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        private void DisconnectUnlocked()
        {
            _buffered = 0;
            if (_fd >= 0)
            {
                close(_fd);
                _fd = -1;
            }
        }

        private static void SetNonBlocking(int fd)
        {
            int flags = fcntl(
                fd,
                3 /* F_GETFL */
                ,
                0
            );
            if (flags >= 0)
            {
                fcntl(
                    fd,
                    4 /* F_SETFL */
                    ,
                    flags | 4 /* O_NONBLOCK */
                );
            }
        }

        private static byte[] BuildSockAddrBytes(string path)
        {
            byte[] pathBytes = System.Text.Encoding.UTF8.GetBytes(path);
            if (pathBytes.Length >= SohnPathLen)
            {
                throw new ArgumentException("socket path too long", nameof(path));
            }

            var addr = new byte[SockAddrLen];
            addr[0] = (byte)(2 + pathBytes.Length); // sun_len
            addr[1] = AfUnix; // sun_family
            Buffer.BlockCopy(pathBytes, 0, addr, 2, pathBytes.Length);
            return addr;
        }

        private static GestureFrame BytesToFrame(byte[] bytes)
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<GestureFrame>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        private static int ReadInto(int fd, byte[] buffer, int offset, int count)
        {
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = IntPtr.Add(handle.AddrOfPinnedObject(), offset);
                return read(fd, ptr, count);
            }
            finally
            {
                handle.Free();
            }
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int socket(int domain, int type, int protocol);

        [DllImport("libc", SetLastError = true)]
        private static extern int connect(int sockfd, IntPtr addr, uint addrlen);

        [DllImport("libc", SetLastError = true)]
        private static extern int close(int fd);

        [DllImport("libc", SetLastError = true)]
        private static extern int read(int fd, IntPtr buf, int count);

        [DllImport("libc", SetLastError = true)]
        private static extern int fcntl(int fd, int cmd, int arg);
    }
}
