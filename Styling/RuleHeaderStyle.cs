namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// A heading style that renders the heading text as the title of a Spectre.Console
    /// <c>Rule</c> widget — for example <c>──── Overview ────</c>. This produces a
    /// visually prominent section divider that demarcates document sections without
    /// taking up multiple lines like a <see cref="FigletTextStyle"/>.
    /// </summary>
    /// <remarks>
    /// Like <see cref="FigletTextStyle"/>, only a small subset of the
    /// <see cref="IHeaderStyle"/> contract is meaningful for this implementation:
    /// <c>Rule</c> does not support a background colour or text decoration on its title,
    /// so <see cref="IHeaderStyle.Background"/> is hard-coded to <see langword="null"/>
    /// and <see cref="IHeaderStyle.Decoration"/> to <see cref="TextDecoration.None"/>.
    /// It is therefore modeled as a peer of <see cref="TextStyle"/> and
    /// <see cref="FigletTextStyle"/> (all three implement <see cref="IHeaderStyle"/>)
    /// rather than as a subclass.
    /// <para>
    /// Assign an instance to a level in <see cref="DisplayOptions.Headers"/> (or to
    /// <see cref="DisplayOptions.Header"/>) to opt that level in to <c>Rule</c> rendering.
    /// Instances are created exclusively via <see cref="Create"/>.
    /// </para>
    /// </remarks>
    public sealed class RuleHeaderStyle : IHeaderStyle
    {
        private RuleHeaderStyle(
            TextJustification? justification,
            TextColor? foreground,
            RuleBorder? border)
        {
            Justification = justification;
            Foreground = foreground;
            Border = border;
        }

        /// <summary>
        /// Creates a new <see cref="RuleHeaderStyle"/>.
        /// </summary>
        /// <param name="justification">Horizontal placement of the title within the rule line.
        /// When <see langword="null"/>, Spectre.Console's default justification is used.</param>
        /// <param name="foreground">Foreground colour applied to the heading text in the rule
        /// title. When <see langword="null"/>, the title inherits whatever colour
        /// Spectre.Console would otherwise use.</param>
        /// <param name="border">Border style for the rule line characters. When
        /// <see langword="null"/>, Spectre.Console's default <c>Rule</c> border is used.</param>
        public static RuleHeaderStyle Create(
            TextJustification? justification = null,
            TextColor? foreground = null,
            RuleBorder? border = null)
            => new(justification, foreground, border);

        /// <summary>
        /// The horizontal justification of the title within the rule. When <see langword="null"/>,
        /// Spectre.Console's default justification is used.
        /// </summary>
        public TextJustification? Justification { get; }

        /// <summary>
        /// The foreground colour applied to the rule's title text. When <see langword="null"/>,
        /// the title inherits whatever colour Spectre.Console would otherwise use.
        /// </summary>
        public TextColor? Foreground { get; }

        /// <summary>
        /// The border style for the rule line. When <see langword="null"/>, Spectre.Console's
        /// default <c>Rule</c> border is used.
        /// </summary>
        public RuleBorder? Border { get; }

        /// <summary>
        /// Always <see langword="null"/>: <c>Rule</c> does not support a background colour.
        /// </summary>
        TextColor? IHeaderStyle.Background => null;

        /// <summary>
        /// Always <see cref="TextDecoration.None"/>: <c>Rule</c> does not support text
        /// decoration (bold, italic, etc.) on its title.
        /// </summary>
        TextDecoration IHeaderStyle.Decoration => TextDecoration.None;

        public override bool Equals(object? obj)
            => obj is RuleHeaderStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground)
                && Border == other.Border;

        public override int GetHashCode() => HashCode.Combine(Justification, Foreground, Border);
    }
}
