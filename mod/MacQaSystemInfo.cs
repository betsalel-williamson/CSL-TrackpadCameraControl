using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
#if HAS_CITIES
using UnityEngine;
#endif

namespace TrackpadCameraControl
{
    /// <summary>
    /// macOS-first QA context (OS, hardware, connected input devices). Fail soft off Mac or on API errors.
    /// </summary>
    internal static class MacQaSystemInfo
    {
        private const string CoreFoundationPath =
            "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
        private const string IOKitPath = "/System/Library/Frameworks/IOKit.framework/IOKit";
        private const uint CfStringEncodingUtf8 = 0x08000100;
        private const int CfNumberIntType = 9;

        internal static void AppendSection(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine("--- System ---");
            AppendOperatingSystem(sb);
            AppendHardware(sb);
            sb.AppendLine();
            sb.AppendLine("--- Input devices ---");
            AppendInputDevices(sb);
        }

        private static void AppendOperatingSystem(StringBuilder sb)
        {
#if HAS_CITIES
            try
            {
                string os = SystemInfo.operatingSystem;
                if (!string.IsNullOrEmpty(os))
                {
                    sb.AppendLine("OS: " + os);
                    return;
                }
            }
            catch
            {
                // fall through
            }
#endif
            sb.AppendLine("OS: " + Environment.OSVersion);
        }

        private static void AppendHardware(StringBuilder sb)
        {
#if HAS_CITIES
            try
            {
                string model = SystemInfo.deviceModel;
                if (!string.IsNullOrEmpty(model))
                {
                    sb.AppendLine("Model: " + model);
                }

                string cpu = SystemInfo.processorType;
                if (!string.IsNullOrEmpty(cpu))
                {
                    sb.AppendLine("CPU: " + cpu);
                }

                int memoryMb = SystemInfo.systemMemorySize;
                if (memoryMb > 0)
                {
                    sb.AppendLine("Memory: " + memoryMb + " MB");
                }

                return;
            }
            catch
            {
                // fall through
            }
#endif
            string hwModel = TryReadSysctl("hw.model");
            if (!string.IsNullOrEmpty(hwModel))
            {
                sb.AppendLine("Model: " + hwModel);
            }

            string machine = TryReadSysctl("hw.machine");
            if (!string.IsNullOrEmpty(machine))
            {
                sb.AppendLine("Machine: " + machine);
            }
        }

        private static void AppendInputDevices(StringBuilder sb)
        {
            if (!IsMacOS())
            {
                sb.AppendLine("(macOS input enumeration unavailable on this host)");
                return;
            }

            List<string> keyboards = new List<string>();
            List<string> mice = new List<string>();
            List<string> trackpads = new List<string>();
            if (!TryCollectHidDevices(keyboards, mice, trackpads))
            {
                sb.AppendLine("(unable to enumerate input devices)");
                return;
            }

            AppendDeviceGroup(sb, "Keyboard", keyboards);
            AppendDeviceGroup(sb, "Mouse", mice);
            AppendDeviceGroup(sb, "Trackpad", trackpads);
            if (keyboards.Count == 0 && mice.Count == 0 && trackpads.Count == 0)
            {
                sb.AppendLine("(none detected)");
            }
        }

        private static void AppendDeviceGroup(StringBuilder sb, string label, List<string> devices)
        {
            for (int i = 0; i < devices.Count; i++)
            {
                sb.AppendLine(label + ": " + devices[i]);
            }
        }

        private static bool TryCollectHidDevices(
            List<string> keyboards,
            List<string> mice,
            List<string> trackpads
        )
        {
            IntPtr manager = IntPtr.Zero;
            IntPtr deviceSet = IntPtr.Zero;
            IntPtr[] deviceBuffer = null;
            try
            {
                manager = IOHIDManagerCreate(IntPtr.Zero, 0);
                if (manager == IntPtr.Zero)
                {
                    return false;
                }

                IOHIDManagerSetDeviceMatching(manager, IntPtr.Zero);
                if (IOHIDManagerOpen(manager, 0) != 0)
                {
                    return false;
                }

                deviceSet = IOHIDManagerCopyDevices(manager);
                if (deviceSet == IntPtr.Zero)
                {
                    return true;
                }

                long count = CFSetGetCount(deviceSet);
                if (count <= 0)
                {
                    return true;
                }

                deviceBuffer = new IntPtr[count];
                CFSetGetValues(deviceSet, deviceBuffer);
                HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < count; i++)
                {
                    ClassifyDevice(deviceBuffer[i], keyboards, mice, trackpads, seen);
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                deviceBuffer = null;
                if (deviceSet != IntPtr.Zero)
                {
                    CFRelease(deviceSet);
                }

                if (manager != IntPtr.Zero)
                {
                    IOHIDManagerClose(manager, 0);
                    CFRelease(manager);
                }
            }
        }

        private static void ClassifyDevice(
            IntPtr device,
            List<string> keyboards,
            List<string> mice,
            List<string> trackpads,
            HashSet<string> seen
        )
        {
            if (device == IntPtr.Zero)
            {
                return;
            }

            int usagePage = ReadIntProperty(device, "PrimaryUsagePage");
            int usage = ReadIntProperty(device, "PrimaryUsage");
            string name = FormatDeviceName(
                ReadStringProperty(device, "Manufacturer"),
                ReadStringProperty(device, "Product")
            );
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            if (!seen.Add(name))
            {
                return;
            }

            if (usagePage == 0x01 && usage == 0x06)
            {
                keyboards.Add(name);
                return;
            }

            if (usagePage == 0x01 && (usage == 0x02 || usage == 0x01))
            {
                mice.Add(name);
                return;
            }

            if (
                usagePage == 0x0D
                || name.IndexOf("trackpad", StringComparison.OrdinalIgnoreCase) >= 0
            )
            {
                trackpads.Add(name);
            }
        }

        private static string FormatDeviceName(string manufacturer, string product)
        {
            if (string.IsNullOrEmpty(product))
            {
                return null;
            }

            if (string.IsNullOrEmpty(manufacturer))
            {
                return product;
            }

            if (product.IndexOf(manufacturer, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return product;
            }

            return manufacturer + " " + product;
        }

        private static int ReadIntProperty(IntPtr device, string key)
        {
            IntPtr keyRef = IntPtr.Zero;
            IntPtr valueRef = IntPtr.Zero;
            try
            {
                keyRef = CFStringCreateWithCString(IntPtr.Zero, key, CfStringEncodingUtf8);
                if (keyRef == IntPtr.Zero)
                {
                    return 0;
                }

                valueRef = IOHIDDeviceGetProperty(device, keyRef);
                if (valueRef == IntPtr.Zero)
                {
                    return 0;
                }

                int value;
                if (!CFNumberGetValue(valueRef, CfNumberIntType, out value))
                {
                    return 0;
                }

                return value;
            }
            finally
            {
                if (keyRef != IntPtr.Zero)
                {
                    CFRelease(keyRef);
                }
            }
        }

        private static string ReadStringProperty(IntPtr device, string key)
        {
            IntPtr keyRef = IntPtr.Zero;
            try
            {
                keyRef = CFStringCreateWithCString(IntPtr.Zero, key, CfStringEncodingUtf8);
                if (keyRef == IntPtr.Zero)
                {
                    return null;
                }

                IntPtr valueRef = IOHIDDeviceGetProperty(device, keyRef);
                if (valueRef == IntPtr.Zero)
                {
                    return null;
                }

                return ReadCfString(valueRef);
            }
            finally
            {
                if (keyRef != IntPtr.Zero)
                {
                    CFRelease(keyRef);
                }
            }
        }

        private static string ReadCfString(IntPtr cfString)
        {
            if (cfString == IntPtr.Zero)
            {
                return null;
            }

            IntPtr direct = CFStringGetCStringPtr(cfString, CfStringEncodingUtf8);
            if (direct != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(direct);
            }

            int length = CFStringGetLength(cfString);
            if (length <= 0)
            {
                return null;
            }

            int byteLength = CFStringGetMaximumSizeForEncoding(length, CfStringEncodingUtf8);
            if (byteLength <= 0)
            {
                return null;
            }

            IntPtr buffer = Marshal.AllocHGlobal(byteLength);
            try
            {
                if (!CFStringGetCString(cfString, buffer, byteLength, CfStringEncodingUtf8))
                {
                    return null;
                }

                return Marshal.PtrToStringAnsi(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static string TryReadSysctl(string name)
        {
            if (!IsMacOS())
            {
                return null;
            }

            IntPtr lengthPtr = Marshal.AllocHGlobal(IntPtr.Size);
            try
            {
                Marshal.WriteIntPtr(lengthPtr, IntPtr.Zero);
                if (sysctlbyname(name, IntPtr.Zero, lengthPtr, IntPtr.Zero, IntPtr.Zero) != 0)
                {
                    return null;
                }

                int length = Marshal.ReadInt32(lengthPtr);
                if (length <= 0)
                {
                    return null;
                }

                IntPtr buffer = Marshal.AllocHGlobal(length);
                try
                {
                    if (sysctlbyname(name, buffer, lengthPtr, IntPtr.Zero, IntPtr.Zero) != 0)
                    {
                        return null;
                    }

                    return Marshal.PtrToStringAnsi(buffer);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(lengthPtr);
            }
        }

        private static bool IsMacOS()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            if (platform != PlatformID.MacOSX && platform != PlatformID.Unix)
            {
                return false;
            }

            return File.Exists("/System/Library/Frameworks/IOKit.framework/IOKit");
        }

        [DllImport(IOKitPath)]
        private static extern IntPtr IOHIDManagerCreate(IntPtr allocator, int options);

        [DllImport(IOKitPath)]
        private static extern void IOHIDManagerSetDeviceMatching(IntPtr manager, IntPtr matching);

        [DllImport(IOKitPath)]
        private static extern int IOHIDManagerOpen(IntPtr manager, int options);

        [DllImport(IOKitPath)]
        private static extern int IOHIDManagerClose(IntPtr manager, int options);

        [DllImport(IOKitPath)]
        private static extern IntPtr IOHIDManagerCopyDevices(IntPtr manager);

        [DllImport(IOKitPath)]
        private static extern IntPtr IOHIDDeviceGetProperty(IntPtr device, IntPtr key);

        [DllImport(CoreFoundationPath)]
        private static extern long CFSetGetCount(IntPtr theSet);

        [DllImport(CoreFoundationPath)]
        private static extern void CFSetGetValues(IntPtr theSet, IntPtr[] values);

        [DllImport(CoreFoundationPath)]
        private static extern void CFRelease(IntPtr cf);

        [DllImport(CoreFoundationPath)]
        private static extern IntPtr CFStringCreateWithCString(
            IntPtr alloc,
            string cStr,
            uint encoding
        );

        [DllImport(CoreFoundationPath)]
        private static extern IntPtr CFStringGetCStringPtr(IntPtr theString, uint encoding);

        [DllImport(CoreFoundationPath)]
        private static extern int CFStringGetLength(IntPtr theString);

        [DllImport(CoreFoundationPath)]
        private static extern int CFStringGetMaximumSizeForEncoding(int length, uint encoding);

        [DllImport(CoreFoundationPath)]
        private static extern bool CFStringGetCString(
            IntPtr theString,
            IntPtr buffer,
            int bufferSize,
            uint encoding
        );

        [DllImport(CoreFoundationPath)]
        private static extern bool CFNumberGetValue(IntPtr number, int theType, out int value);

        [DllImport("/usr/lib/libSystem.dylib")]
        private static extern int sysctlbyname(
            string name,
            IntPtr oldp,
            IntPtr oldlenp,
            IntPtr newp,
            IntPtr newlen
        );
    }
}
