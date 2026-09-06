using System;

namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Which Capture primitive a style row consumes.</summary>
    public enum StylePrimitive
    {
        /// <summary>Centroid drag (pan / orbit).</summary>
        CentroidMotion = 0,

        /// <summary>Pinch scale delta (zoom).</summary>
        Pinch = 1,

        /// <summary>Two-finger twist (rotate).</summary>
        Rotate = 2,
    }

    /// <summary>
    /// One style-table row: finger band + modifiers + primitive → camera op.
    /// Resolve matches rows only — no parallel Maps+ heuristics beside the table (ADR 0004).
    /// </summary>
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

    /// <summary>Live style binding table consumed by Policy resolve.</summary>
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
