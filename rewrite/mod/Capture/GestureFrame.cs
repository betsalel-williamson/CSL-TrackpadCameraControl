// Binary layout matches shared/protocol/gesture_frame.h (little-endian, 48 bytes).

using System;
using System.Runtime.InteropServices;

namespace TrackpadCameraControl.Rewrite
{
    public enum GesturePhase : int
    {
        Began = 0,
        Changed = 1,
        Ended = 2,
        Cancelled = 3,
    }

    [Flags]
    public enum GestureModifiers : uint
    {
        None = 0,
        Option = 1u << 0,
        Shift = 1u << 1,
        Command = 1u << 2,
        Control = 1u << 3,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GestureFrame
    {
        public const uint Magic = 0x54435046; // TCPF
        public const ushort Version = 1;
        public const int Size = 48;

        public uint magic;
        public ushort version;
        public ushort flags;
        public long timestampNs;
        public int fingerCount;
        public int phase;
        public float centroidDeltaX;
        public float centroidDeltaY;
        public float pinchScaleDelta;
        public float rotateDelta;
        public uint modifiers;
        public uint reserved;

        public bool IsValid => magic == Magic && version == Version;
    }
}
