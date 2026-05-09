namespace ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Represents a color for console text rendering.
    /// Can be a named color or an RGB value.
    /// </summary>
    public sealed class TextColor
    {
        private TextColor(NamedColor named)
        {
            Named = named;
            IsRgb = false;
        }

        private TextColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            IsRgb = true;
        }

        public bool IsRgb { get; }
        public NamedColor Named { get; }
        public byte R { get; }
        public byte G { get; }
        public byte B { get; }

        /// <summary>
        /// Creates a TextColor from RGB values.
        /// </summary>
        public static TextColor FromRgb(byte r, byte g, byte b) => new(r, g, b);

        /// <summary>
        /// Creates a TextColor from a NamedColor value.
        /// </summary>
        internal static TextColor FromNamed(NamedColor named) => new(named);

        public static TextColor Black { get; } = new(NamedColor.Black);
        public static TextColor Red { get; } = new(NamedColor.Red);
        public static TextColor Green { get; } = new(NamedColor.Green);
        public static TextColor Yellow { get; } = new(NamedColor.Yellow);
        public static TextColor Blue { get; } = new(NamedColor.Blue);
        public static TextColor Purple { get; } = new(NamedColor.Purple);
        public static TextColor Default { get; } = new(NamedColor.Default);

        public override bool Equals(object? obj)
        {
            if (obj is not TextColor other)
            {
                return false;
            }
            if (IsRgb != other.IsRgb)
            {
                return false;
            }
            if (IsRgb)
            {
                return R == other.R && G == other.G && B == other.B;
            }
            return Named == other.Named;
        }

        public override int GetHashCode()
        {
            if (IsRgb)
            {
                return HashCode.Combine(IsRgb, R, G, B);
            }
            return HashCode.Combine(IsRgb, Named);
        }

        public override string ToString()
            => IsRgb ? $"rgb({R},{G},{B})" : Named.ToString();
    }

    public enum NamedColor
    {
        Default,
        Black,
        Red,
        Green,
        Yellow,
        Blue,
        Purple,
    }
}
