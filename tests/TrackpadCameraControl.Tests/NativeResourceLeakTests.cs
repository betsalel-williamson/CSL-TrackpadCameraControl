using System;
using System.Collections.Generic;
using Xunit;

namespace TrackpadCameraControl.Tests
{
    public class NativeResourceLeakTests
    {
        [Fact]
        public void UnpairedGCHandleAlloc_IsReported()
        {
            string source =
                @"
using System.Runtime.InteropServices;
class Leak {
    void Go(byte[] bytes) {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
    }
}
";
            List<string> findings = NativeResourceLeakAnalyzer.AnalyzeSource("Leak.cs", source);
            Assert.Contains(findings, f => f.Contains("GCHandle.Alloc") && f.Contains("Free"));
        }

        [Fact]
        public void TryFinallyGCHandle_IsClean()
        {
            string source =
                @"
using System.Runtime.InteropServices;
class Ok {
    void Go(byte[] bytes) {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        try { Use(handle); }
        finally { handle.Free(); }
    }
    void Use(GCHandle h) {}
}
";
            Assert.Empty(NativeResourceLeakAnalyzer.AnalyzeSource("Ok.cs", source));
        }

        [Fact]
        public void UnpairedCFStringCreate_IsReported()
        {
            string source =
                @"
class Leak {
    System.IntPtr Title() {
        return CFStringCreateWithCString(System.IntPtr.Zero, ""hi"", 0);
    }
}
";
            List<string> findings = NativeResourceLeakAnalyzer.AnalyzeSource("Cf.cs", source);
            Assert.Contains(
                findings,
                f => f.Contains("CFStringCreateWithCString") && f.Contains("CFRelease")
            );
        }

        [Fact]
        public void NativeLeakOk_SuppressesAcquire()
        {
            string source =
                @"
class Cache {
    System.IntPtr Mode() {
        return CFStringCreateWithCString(System.IntPtr.Zero, ""mode"", 0); // native-leak-ok: process-lifetime
    }
}
";
            Assert.Empty(NativeResourceLeakAnalyzer.AnalyzeSource("Cache.cs", source));
        }

        [Fact]
        public void DeviceStartWithoutStop_IsReported()
        {
            string source =
                @"
class Leak {
    void Go(System.IntPtr dev) {
        MultitouchNative.DeviceStart(dev, 0);
    }
}
";
            List<string> findings = NativeResourceLeakAnalyzer.AnalyzeSource("Dev.cs", source);
            Assert.Contains(findings, f => f.Contains("DeviceStart") && f.Contains("DeviceStop"));
        }

        [Fact]
        public void LocalMonitorWithoutRemove_IsReported()
        {
            string source =
                @"
class Leak {
    void Go() {
        objc_msgSend(cls, sel_registerName(""addLocalMonitorForEventsMatchingMask:handler:""));
    }
}
";
            List<string> findings = NativeResourceLeakAnalyzer.AnalyzeSource("Mon.cs", source);
            Assert.Contains(
                findings,
                f => f.Contains("addLocalMonitor") && f.Contains("removeMonitor")
            );
        }

        [Fact]
        public void GCHandleField_RequiresIDisposable()
        {
            string source =
                @"
using System.Runtime.InteropServices;
class Leak {
    private GCHandle _blockHandle;
    void Go() {
        _blockHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
        _blockHandle.Free();
    }
}
";
            List<string> findings = NativeResourceLeakAnalyzer.AnalyzeSource("Field.cs", source);
            Assert.Contains(findings, f => f.Contains("IDisposable"));
        }

        [Fact]
        public void CaptureAndInteropSources_HaveNoUnpairedNativeResources()
        {
            string root = NativeResourceLeakAnalyzer.FindRepoRoot();
            List<string> findings = NativeResourceLeakAnalyzer.AnalyzeTree(root);
            Assert.True(
                findings.Count == 0,
                "Unpaired native resources:\n" + string.Join("\n", findings)
            );
        }
    }
}
