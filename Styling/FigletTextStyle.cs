namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// A <see cref="TextStyle"/> that instructs the renderer to draw the styled text
    /// (currently only headings) as large ASCII-art using Spectre.Console's
    /// <c>FigletText</c> widget. The base <see cref="TextStyle.Foreground"/> color is
    /// used as the FIGlet foreground color when supplied; decoration and background
    /// are ignored by <c>FigletText</c>.
    /// </summary>
    /// <remarks>
    /// To opt in for top-level (<c>#</c>) headings, set
    /// <c>DisplayOptions.Headers[0]</c> (or any specific level) to an instance of
    /// <see cref="FigletTextStyle"/>. Deeper levels that still use a plain
    /// <see cref="TextStyle"/> continue to render with the existing styled markup
    /// approach.
    /// </remarks>
    public class FigletTextStyle : TextStyle
    {
        public FigletTextStyle(
            TextJustification? justification = null,
            TextColor? foreground = null,
            TextDecoration decoration = TextDecoration.None,
            TextColor? background = null)
            : base(decoration: decoration, foreground: foreground, background: background)
        {
            Justification = justification;
        }

        /// <summary>
        /// The horizontal justification for the rendered FIGlet text. When <see langword="null"/>,
        /// Spectre.Console's default justification is used.
        /// </summary>
        public TextJustification? Justification { get; }

        public override bool Equals(object? obj)
        {
            if (!base.Equals(obj))
            {
                return false;
            }
            // Safe: TextStyle.Equals requires obj.GetType() == GetType(), so when base.Equals returns true
            // obj is guaranteed to be a FigletTextStyle.
            var other = (FigletTextStyle)obj!;
            return Justification == other.Justification;
        }

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Justification);
    }
}
