using System;
using TrackpadCameraControl.Gestures;

namespace TrackpadCameraControl.Rewrite
{
    public enum StylePrimitive
    {
        CentroidMotion = 0,
        Pinch = 1,
        Rotate = 2,
    }

    public struct StyleBindingRow
    {
        public CameraOp Op;
        public StylePrimitive Primitive;
        public int MinFingerCount;
        public int MaxFingerCount;
        public GestureModifiers RequiredModifiers;
        public GestureModifiers ForbiddenModifiers;

        public StyleBindingRow(
            CameraOp op,
            StylePrimitive primitive,
            int minFingerCount,
            int maxFingerCount,
            GestureModifiers requiredModifiers,
            GestureModifiers forbiddenModifiers
        )
        {
            Op = op;
            Primitive = primitive;
            MinFingerCount = minFingerCount;
            MaxFingerCount = maxFingerCount;
            RequiredModifiers = requiredModifiers;
            ForbiddenModifiers = forbiddenModifiers;
        }

        public bool MatchesFingers(int fingerCount)
        {
            return fingerCount >= MinFingerCount && fingerCount <= MaxFingerCount;
        }

        public bool MatchesModifiers(uint modifiers)
        {
            uint required = (uint)RequiredModifiers;
            uint forbidden = (uint)ForbiddenModifiers;
            if ((modifiers & required) != required)
            {
                return false;
            }

            if (forbidden != 0 && (modifiers & forbidden) != 0)
            {
                return false;
            }

            return true;
        }
    }

    public sealed class StyleBindingTable
    {
        private readonly StyleBindingRow[] _rows;

        public StyleBindingTable(StyleBindingRow[] rows)
        {
            if (rows == null || rows.Length == 0)
            {
                throw new ArgumentException("style table requires at least one row", "rows");
            }

            _rows = new StyleBindingRow[rows.Length];
            Array.Copy(rows, _rows, rows.Length);
        }

        public int Count => _rows.Length;

        public StyleBindingRow this[int index] => _rows[index];

        public StyleBindingRow[] ToArray()
        {
            var copy = new StyleBindingRow[_rows.Length];
            Array.Copy(_rows, copy, _rows.Length);
            return copy;
        }
    }
}
