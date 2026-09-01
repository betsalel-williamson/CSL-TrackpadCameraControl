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
        private const string LibSystem = "/usr/lib/libSystem.dylib";
        private const string CoreFoundationPath =
            "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
        private const string IOKitPath = "/System/Library/Frameworks/IOKit.framework/IOKit";
        private const uint CfStringEncodingUtf8 = 0x08000100;
        private const int CfNumberSInt32Type = 3;
        private const int CfNumberIntType = 9;

        private static bool _nativeResolved;
        private static bool _nativeReady;
        private static IntPtr _ioKit;
        private static IntPtr _coreFoundation;

        private static IOServiceMatchingFn _ioServiceMatching;
        private static IOServiceGetMatchingServicesFn _ioServiceGetMatchingServices;
        private static IOIteratorNextFn _ioIteratorNext;
        private static IOObjectReleaseFn _ioObjectRelease;
        private static IORegistryEntryCreateCFPropertyFn _ioRegistryEntryCreateCFProperty;
        private static CFReleaseFn _cfRelease;
        private static CFStringCreateWithCStringFn _cfStringCreateWithCString;
        private static CFStringGetCStringPtrFn _cfStringGetCStringPtr;
        private static CFStringGetLengthFn _cfStringGetLength;
        private static CFStringGetMaximumSizeForEncodingFn _cfStringGetMaximumSizeForEncoding;
        private static CFStringGetCStringFn _cfStringGetCString;
        private static CFNumberGetValueFn _cfNumberGetValue;
        private static CFGetTypeIDFn _cfGetTypeID;
        private static CFStringGetTypeIDFn _cfStringGetTypeID;
        private static CFNumberGetTypeIDFn _cfNumberGetTypeID;
        private static CFBooleanGetTypeIDFn _cfBooleanGetTypeID;
        private static CFBooleanGetValueFn _cfBooleanGetValue;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr IOServiceMatchingFn(string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IOServiceGetMatchingServicesFn(
            uint mainPort,
            IntPtr matching,
            out uint iterator
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint IOIteratorNextFn(uint iterator);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int IOObjectReleaseFn(uint obj);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr IORegistryEntryCreateCFPropertyFn(
            uint entry,
            IntPtr key,
            IntPtr allocator,
            uint options
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void CFReleaseFn(IntPtr cf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CFStringCreateWithCStringFn(
            IntPtr alloc,
            string cStr,
            uint encoding
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CFStringGetCStringPtrFn(IntPtr theString, uint encoding);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CFStringGetLengthFn(IntPtr theString);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int CFStringGetMaximumSizeForEncodingFn(int length, uint encoding);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CFStringGetCStringFn(
            IntPtr theString,
            IntPtr buffer,
            int bufferSize,
            uint encoding
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CFNumberGetValueFn(IntPtr number, int theType, out int value);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CFGetTypeIDFn(IntPtr cf);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CFStringGetTypeIDFn();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CFNumberGetTypeIDFn();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CFBooleanGetTypeIDFn();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool CFBooleanGetValueFn(IntPtr boolean);

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
                    return;
                }
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
            string error;
            if (!TryCollectHidDevices(keyboards, mice, trackpads, out error))
            {
                sb.AppendLine(
                    string.IsNullOrEmpty(error)
                        ? "(unable to enumerate input devices)"
                        : "(unable to enumerate input devices: " + error + ")"
                );
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
            List<string> trackpads,
            out string error
        )
        {
            error = null;
            if (!EnsureNative())
            {
                error = "IOKit unavailable";
                return false;
            }

            IntPtr matching = IntPtr.Zero;
            uint iterator = 0;
            try
            {
                // Consumed by IOServiceGetMatchingServices on success — do not CFRelease.
                matching = _ioServiceMatching("IOHIDDevice");
                if (matching == IntPtr.Zero)
                {
                    error = "IOServiceMatching failed";
                    return false;
                }

                int kr = _ioServiceGetMatchingServices(0, matching, out iterator);
                matching = IntPtr.Zero;
                if (kr != 0)
                {
                    error = "IOServiceGetMatchingServices " + kr;
                    return false;
                }

                Dictionary<string, DeviceRecord> devices = new Dictionary<string, DeviceRecord>(
                    StringComparer.OrdinalIgnoreCase
                );
                for (; ; )
                {
                    uint entry = _ioIteratorNext(iterator);
                    if (entry == 0)
                    {
                        break;
                    }

                    try
                    {
                        ClassifyRegistryEntry(entry, devices);
                    }
                    finally
                    {
                        _ioObjectRelease(entry);
                    }
                }

                foreach (KeyValuePair<string, DeviceRecord> pair in devices)
                {
                    DeviceRecord record = pair.Value;
                    string line = FormatDeviceLine(record.Display, record.Quantity);
                    if (record.IsKeyboard)
                    {
                        keyboards.Add(line);
                    }

                    if (record.IsMouse && !record.IsTrackpad)
                    {
                        mice.Add(line);
                    }

                    if (record.IsTrackpad)
                    {
                        trackpads.Add(line);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.GetType().Name;
                return false;
            }
            finally
            {
                if (iterator != 0)
                {
                    _ioObjectRelease(iterator);
                }

                if (matching != IntPtr.Zero)
                {
                    _cfRelease(matching);
                }
            }
        }

        private sealed class DeviceRecord
        {
            public string Display;
            public int Score;
            public bool IsKeyboard;
            public bool IsMouse;
            public bool IsTrackpad;
            public readonly List<string> InstanceKeys = new List<string>();

            public int Quantity
            {
                get { return InstanceKeys.Count > 0 ? InstanceKeys.Count : 1; }
            }

            public void NoteInstance(string instanceKey)
            {
                if (string.IsNullOrEmpty(instanceKey))
                {
                    // Multiple HID interfaces of one device often omit identity — keep quantity 1.
                    if (InstanceKeys.Count == 0)
                    {
                        InstanceKeys.Add("anon");
                    }

                    return;
                }

                if (!ContainsIgnoreCase(InstanceKeys, instanceKey))
                {
                    InstanceKeys.Add(instanceKey);
                }
            }
        }

        private static void ClassifyRegistryEntry(
            uint entry,
            Dictionary<string, DeviceRecord> devices
        )
        {
            int usagePage = ReadIntProperty(entry, "PrimaryUsagePage");
            int usage = ReadIntProperty(entry, "PrimaryUsage");
            string name = FormatDeviceName(
                ReadStringProperty(entry, "Manufacturer"),
                ReadStringProperty(entry, "Product")
            );
            if (string.IsNullOrEmpty(name))
            {
                name = ReadStringProperty(entry, "USB Product Name");
            }

            if (string.IsNullOrEmpty(name) || IsNoiseProduct(name))
            {
                return;
            }

            bool nameSaysKeyboard =
                name.IndexOf("keyboard", StringComparison.OrdinalIgnoreCase) >= 0;
            bool nameSaysMouse = name.IndexOf("mouse", StringComparison.OrdinalIgnoreCase) >= 0;
            bool nameSaysTrackpad =
                name.IndexOf("trackpad", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("touchpad", StringComparison.OrdinalIgnoreCase) >= 0;

            // Product name wins when HID primary usage is a secondary interface (gaming mice, etc.).
            bool isKeyboard =
                nameSaysKeyboard
                || (usagePage == 0x01 && usage == 0x06 && !nameSaysMouse && !nameSaysTrackpad);
            bool isMouse =
                nameSaysMouse
                || (usagePage == 0x01 && (usage == 0x02 || usage == 0x01) && !nameSaysKeyboard);
            bool isTrackpad = nameSaysTrackpad || usagePage == 0x0D;

            // Built-in Mac boards often report as one HID device (keyboard + trackpad).
            if (!isKeyboard && !isMouse && !isTrackpad)
            {
                return;
            }

            int vendorId = ReadIntProperty(entry, "VendorID");
            int productId = ReadIntProperty(entry, "ProductID");
            int version = ReadIntProperty(entry, "VersionNumber");
            string transport = ReadStringProperty(entry, "Transport");
            bool builtIn = ReadBoolProperty(entry, "Built-In");
            string display = FormatDeviceDisplay(
                name,
                vendorId,
                productId,
                version,
                transport,
                builtIn
            );
            int score = ModelInfoScore(vendorId, productId, version, transport, builtIn);
            // Instance identity for counting only — never written to the clipboard.
            string instanceKey = ReadInstanceKey(entry);

            DeviceRecord record;
            if (!devices.TryGetValue(name, out record))
            {
                record = new DeviceRecord();
                devices[name] = record;
            }

            record.IsKeyboard = record.IsKeyboard || isKeyboard;
            record.IsMouse = record.IsMouse || isMouse;
            record.IsTrackpad = record.IsTrackpad || isTrackpad;
            record.NoteInstance(instanceKey);
            if (score > record.Score || string.IsNullOrEmpty(record.Display))
            {
                record.Display = display;
                record.Score = score;
            }
        }

        /// <summary>
        /// Opaque per-device key for quantity only. Prefer LocationID over serial so we never
        /// need serial in normal USB cases; serial is used only as a last-resort count key.
        /// </summary>
        private static string ReadInstanceKey(uint entry)
        {
            int locationId = ReadIntProperty(entry, "LocationID");
            if (locationId != 0)
            {
                return "loc:" + locationId.ToString("X");
            }

            string serial = ReadStringProperty(entry, "SerialNumber");
            if (!string.IsNullOrEmpty(serial))
            {
                return "sn:" + serial;
            }

            return null;
        }

        /// <summary>
        /// USB-style model identity for QA (hex VID:PID). Never includes serial numbers.
        /// </summary>
        internal static string FormatDeviceDisplay(
            string name,
            int vendorId,
            int productId,
            int versionNumber,
            string transport,
            bool builtIn
        )
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            List<string> parts = new List<string>();
            string id = FormatModelId(vendorId, productId);
            if (!string.IsNullOrEmpty(id))
            {
                parts.Add(id);
            }

            if (versionNumber > 0)
            {
                parts.Add("rev " + versionNumber.ToString("X4"));
            }

            if (!string.IsNullOrEmpty(transport))
            {
                parts.Add(transport);
            }

            if (builtIn)
            {
                parts.Add("built-in");
            }

            if (parts.Count == 0)
            {
                return name;
            }

            StringBuilder sb = new StringBuilder(name);
            sb.Append(" (");
            for (int i = 0; i < parts.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" · ");
                }

                sb.Append(parts[i]);
            }

            sb.Append(')');
            return sb.ToString();
        }

        /// <summary>Append ×N when more than one identical model is connected.</summary>
        internal static string FormatDeviceLine(string display, int quantity)
        {
            if (string.IsNullOrEmpty(display) || quantity <= 1)
            {
                return display;
            }

            return display + " ×" + quantity;
        }

        internal static string FormatModelId(int vendorId, int productId)
        {
            if (vendorId == 0 && productId == 0)
            {
                return null;
            }

            if (vendorId == 0)
            {
                return "pid " + productId.ToString("X4");
            }

            return vendorId.ToString("X4") + ":" + productId.ToString("X4");
        }

        private static int ModelInfoScore(
            int vendorId,
            int productId,
            int versionNumber,
            string transport,
            bool builtIn
        )
        {
            int score = 0;
            if (vendorId != 0)
            {
                score += 4;
            }

            if (productId != 0)
            {
                score += 4;
            }

            if (versionNumber > 0)
            {
                score += 1;
            }

            if (!string.IsNullOrEmpty(transport))
            {
                score += 1;
            }

            if (builtIn)
            {
                score += 1;
            }

            return score;
        }

        private static bool IsNoiseProduct(string name)
        {
            return name.IndexOf("backlight", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("headset", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("ambient light", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("fingerprint", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("faceid", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("lidar", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ContainsIgnoreCase(List<string> items, string value)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (string.Equals(items[i], value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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

        private static int ReadIntProperty(uint entry, string key)
        {
            IntPtr valueRef = CopyProperty(entry, key);
            if (valueRef == IntPtr.Zero)
            {
                return 0;
            }

            try
            {
                if (_cfGetTypeID(valueRef) != _cfNumberGetTypeID())
                {
                    return 0;
                }

                int value;
                if (_cfNumberGetValue(valueRef, CfNumberSInt32Type, out value))
                {
                    return value;
                }

                if (_cfNumberGetValue(valueRef, CfNumberIntType, out value))
                {
                    return value;
                }

                return 0;
            }
            finally
            {
                _cfRelease(valueRef);
            }
        }

        private static string ReadStringProperty(uint entry, string key)
        {
            IntPtr valueRef = CopyProperty(entry, key);
            if (valueRef == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                if (_cfGetTypeID(valueRef) != _cfStringGetTypeID())
                {
                    return null;
                }

                return ReadCfString(valueRef);
            }
            finally
            {
                _cfRelease(valueRef);
            }
        }

        private static bool ReadBoolProperty(uint entry, string key)
        {
            IntPtr valueRef = CopyProperty(entry, key);
            if (valueRef == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                if (_cfBooleanGetTypeID == null || _cfBooleanGetValue == null)
                {
                    return false;
                }

                if (_cfGetTypeID(valueRef) != _cfBooleanGetTypeID())
                {
                    return false;
                }

                return _cfBooleanGetValue(valueRef);
            }
            finally
            {
                _cfRelease(valueRef);
            }
        }

        private static IntPtr CopyProperty(uint entry, string key)
        {
            IntPtr keyRef = _cfStringCreateWithCString(IntPtr.Zero, key, CfStringEncodingUtf8);
            if (keyRef == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            try
            {
                return _ioRegistryEntryCreateCFProperty(entry, keyRef, IntPtr.Zero, 0);
            }
            finally
            {
                _cfRelease(keyRef);
            }
        }

        private static string ReadCfString(IntPtr cfString)
        {
            if (cfString == IntPtr.Zero)
            {
                return null;
            }

            IntPtr direct = _cfStringGetCStringPtr(cfString, CfStringEncodingUtf8);
            if (direct != IntPtr.Zero)
            {
                return Marshal.PtrToStringAnsi(direct);
            }

            int length = _cfStringGetLength(cfString);
            if (length <= 0)
            {
                return null;
            }

            int byteLength = _cfStringGetMaximumSizeForEncoding(length, CfStringEncodingUtf8);
            if (byteLength <= 0)
            {
                return null;
            }

            IntPtr buffer = Marshal.AllocHGlobal(byteLength);
            try
            {
                if (!_cfStringGetCString(cfString, buffer, byteLength, CfStringEncodingUtf8))
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

        private static bool EnsureNative()
        {
            if (_nativeResolved)
            {
                return _nativeReady;
            }

            _nativeResolved = true;
            try
            {
                _ioKit = dlopen(IOKitPath, 1);
                _coreFoundation = dlopen(CoreFoundationPath, 1);
                if (_ioKit == IntPtr.Zero || _coreFoundation == IntPtr.Zero)
                {
                    return false;
                }

                _ioServiceMatching = LoadFn<IOServiceMatchingFn>(_ioKit, "IOServiceMatching");
                _ioServiceGetMatchingServices = LoadFn<IOServiceGetMatchingServicesFn>(
                    _ioKit,
                    "IOServiceGetMatchingServices"
                );
                _ioIteratorNext = LoadFn<IOIteratorNextFn>(_ioKit, "IOIteratorNext");
                _ioObjectRelease = LoadFn<IOObjectReleaseFn>(_ioKit, "IOObjectRelease");
                _ioRegistryEntryCreateCFProperty = LoadFn<IORegistryEntryCreateCFPropertyFn>(
                    _ioKit,
                    "IORegistryEntryCreateCFProperty"
                );
                _cfRelease = LoadFn<CFReleaseFn>(_coreFoundation, "CFRelease");
                _cfStringCreateWithCString = LoadFn<CFStringCreateWithCStringFn>(
                    _coreFoundation,
                    "CFStringCreateWithCString"
                );
                _cfStringGetCStringPtr = LoadFn<CFStringGetCStringPtrFn>(
                    _coreFoundation,
                    "CFStringGetCStringPtr"
                );
                _cfStringGetLength = LoadFn<CFStringGetLengthFn>(
                    _coreFoundation,
                    "CFStringGetLength"
                );
                _cfStringGetMaximumSizeForEncoding = LoadFn<CFStringGetMaximumSizeForEncodingFn>(
                    _coreFoundation,
                    "CFStringGetMaximumSizeForEncoding"
                );
                _cfStringGetCString = LoadFn<CFStringGetCStringFn>(
                    _coreFoundation,
                    "CFStringGetCString"
                );
                _cfNumberGetValue = LoadFn<CFNumberGetValueFn>(_coreFoundation, "CFNumberGetValue");
                _cfGetTypeID = LoadFn<CFGetTypeIDFn>(_coreFoundation, "CFGetTypeID");
                _cfStringGetTypeID = LoadFn<CFStringGetTypeIDFn>(
                    _coreFoundation,
                    "CFStringGetTypeID"
                );
                _cfNumberGetTypeID = LoadFn<CFNumberGetTypeIDFn>(
                    _coreFoundation,
                    "CFNumberGetTypeID"
                );
                _cfBooleanGetTypeID = LoadFn<CFBooleanGetTypeIDFn>(
                    _coreFoundation,
                    "CFBooleanGetTypeID"
                );
                _cfBooleanGetValue = LoadFn<CFBooleanGetValueFn>(
                    _coreFoundation,
                    "CFBooleanGetValue"
                );

                _nativeReady =
                    _ioServiceMatching != null
                    && _ioServiceGetMatchingServices != null
                    && _ioIteratorNext != null
                    && _ioObjectRelease != null
                    && _ioRegistryEntryCreateCFProperty != null
                    && _cfRelease != null
                    && _cfStringCreateWithCString != null
                    && _cfStringGetCStringPtr != null
                    && _cfStringGetLength != null
                    && _cfStringGetMaximumSizeForEncoding != null
                    && _cfStringGetCString != null
                    && _cfNumberGetValue != null
                    && _cfGetTypeID != null
                    && _cfStringGetTypeID != null
                    && _cfNumberGetTypeID != null
                    && _cfBooleanGetTypeID != null
                    && _cfBooleanGetValue != null;
                return _nativeReady;
            }
            catch
            {
                _nativeReady = false;
                return false;
            }
        }

        private static T LoadFn<T>(IntPtr lib, string name)
            where T : class
        {
            IntPtr sym = dlsym(lib, name);
            if (sym == IntPtr.Zero)
            {
                return null;
            }

            return (T)(object)Marshal.GetDelegateForFunctionPointer(sym, typeof(T));
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
                if (sysctlbyname(name, IntPtr.Zero, lengthPtr, IntPtr.Zero, 0) != 0)
                {
                    return null;
                }

                long lengthLong = Marshal.ReadIntPtr(lengthPtr).ToInt64();
                if (lengthLong <= 0 || lengthLong > int.MaxValue)
                {
                    return null;
                }

                int length = (int)lengthLong;
                IntPtr buffer = Marshal.AllocHGlobal(length);
                try
                {
                    if (sysctlbyname(name, buffer, lengthPtr, IntPtr.Zero, 0) != 0)
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

            return File.Exists(IOKitPath);
        }

        [DllImport(LibSystem)]
        private static extern IntPtr dlopen(string path, int mode);

        [DllImport(LibSystem)]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport(LibSystem)]
        private static extern int sysctlbyname(
            string name,
            IntPtr oldp,
            IntPtr oldlenp,
            IntPtr newp,
            ulong newlen
        );
    }
}
