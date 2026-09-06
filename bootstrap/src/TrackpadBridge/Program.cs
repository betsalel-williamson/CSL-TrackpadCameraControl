using System;
using System.Runtime.InteropServices;
using TrackpadCapture;

namespace TrackpadBridge
{
    internal static class Program
    {
        private static volatile bool s_running = true;

        private static int Main(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Console.Error.WriteLine(
                    "TrackpadBridge: macOS only (MultitouchSupport is unavailable here)."
                );
                return 1;
            }

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                s_running = false;
            };

            bool debug = !string.IsNullOrEmpty(
                Environment.GetEnvironmentVariable("TRACKPAD_BRIDGE_DEBUG")
            );
            string path = ResolveSocketPath();

            using (var server = new GestureSocketServer(path))
            {
                if (!server.TryStart(out string sockErr))
                {
                    Console.Error.WriteLine("TrackpadBridge: socket failed: " + sockErr);
                    return 1;
                }

                using (var capture = new MacTrackpadCapture(debug, Console.Error.WriteLine))
                {
                    if (!capture.TryStart(server.Send, out string capErr))
                    {
                        Console.Error.WriteLine("TrackpadBridge: capture failed: " + capErr);
                        return 1;
                    }

                    Console.Error.WriteLine(
                        "TrackpadBridge: ready (centroid / pinch / rotate / modifiers)"
                    );

                    while (s_running)
                    {
                        // Pump CFRunLoop so Multitouch contact callbacks fire.
                        MacRunLoop.RunInDefaultMode(0.25, true);
                    }
                }
            }

            Console.Error.WriteLine("TrackpadBridge: stopped");
            return 0;
        }

        /// <summary>Matches IpcGestureSource.DefaultSocketPath.</summary>
        internal static string ResolveSocketPath()
        {
            string env = Environment.GetEnvironmentVariable("TRACKPAD_BRIDGE_SOCKET");
            if (!string.IsNullOrEmpty(env))
            {
                return env;
            }

            string tmp = Environment.GetEnvironmentVariable("TMPDIR");
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
    }
}
