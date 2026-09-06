using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using TrackpadCapture;

namespace TrackpadBridge
{
    /// <summary>Accepts one Unix-socket client and writes 48-byte GestureFrames.</summary>
    internal sealed class GestureSocketServer : IDisposable
    {
        private readonly string _path;
        private readonly object _gate = new object();
        private Socket _listen;
        private Socket _client;
        private Thread _acceptThread;
        private volatile bool _running;

        public GestureSocketServer(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path => _path;

        public bool TryStart(out string error)
        {
            error = null;
            try
            {
                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }

                _listen = new Socket(
                    AddressFamily.Unix,
                    SocketType.Stream,
                    ProtocolType.Unspecified
                );
                _listen.Bind(new UnixDomainSocketEndPoint(_path));
                _listen.Listen(1);
                _running = true;

                _acceptThread = new Thread(AcceptLoop)
                {
                    IsBackground = true,
                    Name = "TrackpadBridge-accept",
                };
                _acceptThread.Start();

                Console.Error.WriteLine("TrackpadBridge: listening on " + _path);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Dispose();
                return false;
            }
        }

        public void Send(GestureFrame frame)
        {
            byte[] bytes = FrameToBytes(frame);
            lock (_gate)
            {
                if (_client == null)
                {
                    return;
                }

                try
                {
                    int n = _client.Send(bytes);
                    if (n != bytes.Length)
                    {
                        DropClientUnlocked();
                        Console.Error.WriteLine("TrackpadBridge: client disconnected");
                    }
                }
                catch
                {
                    DropClientUnlocked();
                    Console.Error.WriteLine("TrackpadBridge: client disconnected");
                }
            }
        }

        public void Dispose()
        {
            _running = false;

            try
            {
                _listen?.Close();
            }
            catch
            {
                // ignore
            }

            _listen = null;

            if (_acceptThread != null)
            {
                if (!_acceptThread.Join(2000))
                {
                    // Accept may block; closing listen should unblock.
                }

                _acceptThread = null;
            }

            lock (_gate)
            {
                DropClientUnlocked();
            }

            try
            {
                if (File.Exists(_path))
                {
                    File.Delete(_path);
                }
            }
            catch
            {
                // ignore
            }
        }

        private void AcceptLoop()
        {
            while (_running)
            {
                try
                {
                    Socket incoming = _listen.Accept();
                    lock (_gate)
                    {
                        DropClientUnlocked();
                        _client = incoming;
                    }

                    Console.Error.WriteLine("TrackpadBridge: client connected");
                }
                catch (SocketException)
                {
                    if (!_running)
                    {
                        break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_running)
                    {
                        Console.Error.WriteLine("TrackpadBridge: accept error: " + ex.Message);
                    }

                    break;
                }
            }
        }

        private void DropClientUnlocked()
        {
            if (_client == null)
            {
                return;
            }

            try
            {
                _client.Close();
            }
            catch
            {
                // ignore
            }

            _client = null;
        }

        private static byte[] FrameToBytes(GestureFrame frame)
        {
            var bytes = new byte[GestureFrame.Size];
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(frame, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            return bytes;
        }
    }
}
