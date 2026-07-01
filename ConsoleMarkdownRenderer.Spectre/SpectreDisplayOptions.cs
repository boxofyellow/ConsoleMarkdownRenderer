using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre;

[SpectreSourceFile]
public sealed class SpectreDisplayOptions
{
    public Style AbbreviationTitle { get; set; } = new(decoration: Decoration.Dim);

    public Style AlertCaution { get; set; } = new(foreground: Color.Red, decoration: Decoration.Bold);
    public Style AlertImportant { get; set; } = new(foreground: Color.Purple, decoration: Decoration.Bold);
    public Style AlertNote { get; set; } = new(foreground: Color.Blue, decoration: Decoration.Bold);
    public Style AlertTip { get; set; } = new(foreground: Color.Green, decoration: Decoration.Bold);
    public Style AlertWarning { get; set; } = new(foreground: Color.Yellow, decoration: Decoration.Bold);
    public BoxBorder AlertPanelBorder { get; set; } = BoxBorder.Rounded;

    public Style Bold { get; set; } = new(decoration: Decoration.Bold);
    public Style Citation { get; set; } = new(decoration: Decoration.Italic);
    public Style CodeBlock { get; set; } = new(foreground: Color.Yellow, background: Color.Blue);
    public Style CodeInLine { get; set; } = new(foreground: Color.Yellow, background: Color.Blue);
    public Style CustomContainer { get; set; } = new(decoration: Decoration.None);
    public Style CustomContainerInfo { get; set; } = new(decoration: Decoration.Bold);
    public Style CustomContainerInline { get; set; } = new(decoration: Decoration.Bold);
    public Style DefinitionItem { get; set; } = new(decoration: Decoration.None);
    public Style DefinitionList { get; set; } = new(decoration: Decoration.None);
    public Style DefinitionTerm { get; set; } = new(decoration: Decoration.Bold);
    public bool ShowFencedCodeBlockInfo { get; set; } = false;
    public Style FencedCodeBlockInfo { get; set; } = new(foreground: Color.Green, background: Color.Blue);
    public BoxBorder FencedCodeBlockInfoPanelBorder { get; set; } = BoxBorder.Rounded;
    public Style FigureCaption { get; set; } = new(decoration: Decoration.Italic);

    // List of Styles to use for headers the first will be used for #, the second for ## and so on
    // If the document referenced more than the length of the list, the Style in header will be used.
    // By default the first entry is a FigletTextStyle, so top-level (#) headings render as
    // FIGlet ASCII art. Replace or remove that entry (or assign a plain TextStyle) to opt
    // H1 into the styled-markup path used by deeper levels.
    public List<ISpectreHeaderStyle> Headers { get; set; } = new()
    {
        SpectreFigletTextStyle.Create(justification: Justify.Left),
    };
    public ISpectreHeaderStyle Header { get; set; } = new SpectreTextStyle(decoration: Decoration.Bold | Decoration.Underline | Decoration.Invert);

    public Style HtmlBlock { get; set; } = new(foreground: Color.Black, background: Color.Green);
    public Style HtmlInline { get; set; } = new(foreground: Color.Black, background: Color.Green);
    public Style Footer { get; set; } = new(decoration: Decoration.Dim | Decoration.Italic);
    public Style Footnote { get; set; } = new(decoration: Decoration.Bold);
    public Style FootnoteGroup { get; set; } = new(decoration: Decoration.Italic);
    public Style FootnoteLink { get; set; } = new(foreground: Color.Blue, decoration: Decoration.Underline);
    public bool Emojis { get; set; } = true;
    public Style Inserted { get; set; } = new(decoration: Decoration.Underline);
    public Style Italic { get; set; } = new(decoration: Decoration.Italic);
    public Style Marked { get; set; } = new(foreground: Color.Black, background: Color.Yellow);
    public Style MathBlock { get; set; } = new(foreground: Color.Green, background: Color.Purple);
    public Style MathBlockLabel { get; set; } = new(foreground: Color.Yellow, background: Color.Purple);
    public string MathBlockLabelText { get; set; } = string.Empty;
    public BoxBorder MathBlockPanelBorder { get; set; } = BoxBorder.Rounded;
    public Style MathInline { get; set; } = new(foreground: Color.Green, background: Color.Purple);
    public Style QuotedBlock { get; set; } = new(decoration: Decoration.Italic);
    public bool SmartyPants { get; set; } = true;
    public Style Strikethrough { get; set; } = new(decoration: Decoration.Strikethrough);
    public Style ThematicBreak { get; set; } = new();
    public TableBorder TableBorder { get; set; } = TableBorder.Square;
    public Style TableBorderStyle { get; set; } = new();
    public bool TableExpand { get; set; } = false;
    public Style Subscript { get; set; } = new(decoration: Decoration.SlowBlink);
    public Style Superscript { get; set; } = new(decoration: Decoration.RapidBlink);

    // Yes, these are more than a style, but it should help identify where things need updating
    public Style UnknownDelimiterChar { get; set; } = new(decoration: Decoration.Dim);
    public Style UnknownDelimiterContent { get; set; } = new(decoration: Decoration.Invert);

    public Style YamlFrontMatter { get; set; } = new(decoration: Decoration.Italic | Decoration.Dim);

    // When set to true wrap Headers with '#'s 
    public bool WrapHeader { get; set; } = true;

    public bool UseTerminalHyperlinks { get; set; } = true;
    public bool IncludeDebug { get; set; } = false;

    public SpectreDisplayOptions Clone() => new()
    {
        AbbreviationTitle = this.AbbreviationTitle,
        AlertCaution = this.AlertCaution,
        AlertImportant = this.AlertImportant,
        AlertNote = this.AlertNote,
        AlertPanelBorder = this.AlertPanelBorder,
        AlertTip = this.AlertTip,
        AlertWarning = this.AlertWarning,
        Bold = this.Bold,
        Citation = this.Citation,
        CodeBlock = this.CodeBlock,
        CodeInLine = this.CodeInLine,
        CustomContainer = this.CustomContainer,
        CustomContainerInfo = this.CustomContainerInfo,
        CustomContainerInline = this.CustomContainerInline,
        DefinitionItem = this.DefinitionItem,
        DefinitionList = this.DefinitionList,
        DefinitionTerm = this.DefinitionTerm,
        Emojis = this.Emojis,
        FencedCodeBlockInfo = this.FencedCodeBlockInfo,
        FencedCodeBlockInfoPanelBorder = this.FencedCodeBlockInfoPanelBorder,
        FigureCaption = this.FigureCaption,
        Footer = this.Footer,
        Footnote = this.Footnote,
        FootnoteGroup = this.FootnoteGroup,
        FootnoteLink = this.FootnoteLink,
        Header = this.Header,
        Headers = new(this.Headers),
        HtmlBlock = this.HtmlBlock,
        HtmlInline = this.HtmlInline,
        IncludeDebug = this.IncludeDebug,
        Inserted = this.Inserted,
        Italic = this.Italic,
        Marked = this.Marked,
        MathBlock = this.MathBlock,
        MathBlockLabel = this.MathBlockLabel,
        MathBlockLabelText = this.MathBlockLabelText,
        MathBlockPanelBorder = this.MathBlockPanelBorder,
        MathInline = this.MathInline,
        QuotedBlock = this.QuotedBlock,
        ShowFencedCodeBlockInfo = this.ShowFencedCodeBlockInfo,
        SmartyPants = this.SmartyPants,
        Strikethrough = this.Strikethrough,
        Subscript = this.Subscript,
        Superscript = this.Superscript,
        TableBorder = this.TableBorder,
        TableBorderStyle = this.TableBorderStyle,
        TableExpand = this.TableExpand,
        ThematicBreak = this.ThematicBreak,
        UnknownDelimiterChar = this.UnknownDelimiterChar,
        UnknownDelimiterContent = this.UnknownDelimiterContent,
        UseTerminalHyperlinks = this.UseTerminalHyperlinks,
        WrapHeader = this.WrapHeader,
        YamlFrontMatter = this.YamlFrontMatter,
    };

    public override bool Equals(object? obj)
    {
        if (obj is not SpectreDisplayOptions other)
        {
            return false;
        }
        
        return AbbreviationTitle.Equals(other.AbbreviationTitle)
            && AlertCaution.Equals(other.AlertCaution)
            && AlertImportant.Equals(other.AlertImportant)
            && AlertNote.Equals(other.AlertNote)
            && AlertPanelBorder == other.AlertPanelBorder
            && AlertTip.Equals(other.AlertTip)
            && AlertWarning.Equals(other.AlertWarning)
            && Bold.Equals(other.Bold)
            && Citation.Equals(other.Citation)
            && CodeBlock.Equals(other.CodeBlock)
            && CodeInLine.Equals(other.CodeInLine)
            && CustomContainer.Equals(other.CustomContainer)
            && CustomContainerInfo.Equals(other.CustomContainerInfo)
            && CustomContainerInline.Equals(other.CustomContainerInline)
            && DefinitionItem.Equals(other.DefinitionItem)
            && DefinitionList.Equals(other.DefinitionList)
            && DefinitionTerm.Equals(other.DefinitionTerm)
            && Emojis == other.Emojis
            && FencedCodeBlockInfo.Equals(other.FencedCodeBlockInfo)
            && FencedCodeBlockInfoPanelBorder == other.FencedCodeBlockInfoPanelBorder
            && FigureCaption.Equals(other.FigureCaption)
            && Footer.Equals(other.Footer)
            && Footnote.Equals(other.Footnote)
            && FootnoteGroup.Equals(other.FootnoteGroup)
            && FootnoteLink.Equals(other.FootnoteLink)
            && Header.Equals(other.Header)
            && Headers.SequenceEqual(other.Headers) 
            && HtmlBlock.Equals(other.HtmlBlock)
            && HtmlInline.Equals(other.HtmlInline)
            && IncludeDebug == other.IncludeDebug
            && Inserted.Equals(other.Inserted)
            && Italic.Equals(other.Italic)
            && Marked.Equals(other.Marked)
            && MathBlock.Equals(other.MathBlock)
            && MathBlockLabel.Equals(other.MathBlockLabel)
            && MathBlockLabelText == other.MathBlockLabelText
            && MathBlockPanelBorder == other.MathBlockPanelBorder
            && MathInline.Equals(other.MathInline)
            && QuotedBlock.Equals(other.QuotedBlock)
            && ShowFencedCodeBlockInfo == other.ShowFencedCodeBlockInfo
            && SmartyPants == other.SmartyPants
            && Strikethrough.Equals(other.Strikethrough)
            && Subscript.Equals(other.Subscript)
            && Superscript.Equals(other.Superscript)
            && TableBorder == other.TableBorder
            && TableBorderStyle.Equals(other.TableBorderStyle)
            && TableExpand == other.TableExpand
            && ThematicBreak.Equals(other.ThematicBreak)
            && UnknownDelimiterChar.Equals(other.UnknownDelimiterChar)
            && UnknownDelimiterContent.Equals(other.UnknownDelimiterContent)
            && UseTerminalHyperlinks == other.UseTerminalHyperlinks
            && WrapHeader == other.WrapHeader
            && YamlFrontMatter.Equals(other.YamlFrontMatter);   
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(AbbreviationTitle);
        hash.Add(AlertCaution);
        hash.Add(AlertImportant);
        hash.Add(AlertNote);
        hash.Add(AlertPanelBorder);
        hash.Add(AlertTip);
        hash.Add(AlertWarning);
        hash.Add(Bold);
        hash.Add(Citation);
        hash.Add(CodeBlock);
        hash.Add(CodeInLine);
        hash.Add(CustomContainer);
        hash.Add(CustomContainerInfo);
        hash.Add(CustomContainerInline);
        hash.Add(DefinitionItem);
        hash.Add(DefinitionList);
        hash.Add(DefinitionTerm);
        hash.Add(Emojis);
        hash.Add(FencedCodeBlockInfo);
        hash.Add(FencedCodeBlockInfoPanelBorder);
        hash.Add(FigureCaption);
        hash.Add(Footer);
        hash.Add(Footnote);
        hash.Add(FootnoteGroup);
        hash.Add(FootnoteLink);
        hash.Add(Header);
        foreach (var h in Headers)
        {
            hash.Add(h);
        }
        hash.Add(HtmlBlock);
        hash.Add(HtmlInline);
        hash.Add(IncludeDebug);
        hash.Add(Inserted);
        hash.Add(Italic);
        hash.Add(Marked);
        hash.Add(MathBlock);
        hash.Add(MathBlockLabel);
        hash.Add(MathBlockLabelText);
        hash.Add(MathBlockPanelBorder);
        hash.Add(MathInline);
        hash.Add(QuotedBlock);
        hash.Add(ShowFencedCodeBlockInfo);
        hash.Add(SmartyPants);
        hash.Add(Strikethrough);
        hash.Add(Subscript);
        hash.Add(Superscript);
        hash.Add(TableBorder);
        hash.Add(TableBorderStyle);
        hash.Add(TableExpand);
        hash.Add(ThematicBreak);
        hash.Add(UnknownDelimiterChar);
        hash.Add(UnknownDelimiterContent);
        hash.Add(UseTerminalHyperlinks);
        hash.Add(WrapHeader);
        hash.Add(YamlFrontMatter);
        return hash.ToHashCode();
    }
    

    /// <summary>
    /// Computes which style to use for given Object Level
    /// </summary>
    /// <param name="level">The level of the Object for `#` it will 1, for `##` it will be 2, and so on</param>
    /// <returns>The style to use</returns>
    internal ISpectreHeaderStyle EffectiveHeader(int level) => 
        level <= Headers.Count 
               ? Headers[level - 1]
               : Header;

    /// <summary>
    /// Computes which style to use for the kind label of a GitHub-style alert block.
    /// </summary>
    /// <param name="kind">The alert kind as parsed by Markdig (e.g. "NOTE", "WARNING"). Matched case-insensitively.</param>
    /// <returns>The style to use for the kind label. Unknown kinds fall back to <see cref="QuotedBlock"/>.</returns>
    internal Style EffectiveAlert(string kind) => kind.ToUpperInvariant() switch
    {
        "CAUTION" => AlertCaution,
        "IMPORTANT" => AlertImportant,
        "NOTE" => AlertNote,
        "TIP" => AlertTip,
        "WARNING" => AlertWarning,
        _ => QuotedBlock,
    };

    public string Serialize(JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(this, BuildEffectiveOptions(options, createObject: null));

    public static async Task<SpectreDisplayOptions> DeserializeAsync(
        string json,
        JsonSerializerOptions? options = null,
        Func<SpectreDisplayOptions>? createObject = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(json);
        var result = JsonSerializer.Deserialize<SpectreDisplayOptions>(json, BuildEffectiveOptions(options, createObject))
            ?? throw new JsonException($"{nameof(SpectreDisplayOptions)} JSON deserialized to null.");
        await result.EnsureHeaderFontsLoadedAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public static async Task<SpectreDisplayOptions> DeserializeAsync(
        Stream utf8Json,
        JsonSerializerOptions? options = null,
        Func<SpectreDisplayOptions>? createObject = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(utf8Json);
        var result = JsonSerializer.Deserialize<SpectreDisplayOptions>(utf8Json, BuildEffectiveOptions(options, createObject))
            ?? throw new JsonException($"{nameof(SpectreDisplayOptions)} JSON deserialized to null.");
        await result.EnsureHeaderFontsLoadedAsync(cancellationToken).ConfigureAwait(false);
        return result;
    }

    public static SpectreDisplayOptions Empty()
    {
        return new()
        {
            AbbreviationTitle = Style.Plain,
            AlertCaution = Style.Plain,
            AlertImportant = Style.Plain,
            AlertNote = Style.Plain,
            AlertPanelBorder = BoxBorder.None,
            AlertTip = Style.Plain,
            AlertWarning = Style.Plain,
            Bold = Style.Plain,
            Citation = Style.Plain,
            CodeBlock = Style.Plain,
            CodeInLine = Style.Plain,
            CustomContainer = Style.Plain,
            CustomContainerInfo = Style.Plain,
            CustomContainerInline = Style.Plain,
            DefinitionItem = Style.Plain,
            DefinitionList = Style.Plain,
            DefinitionTerm = Style.Plain,
            Emojis = false,
            FencedCodeBlockInfo = Style.Plain,
            FencedCodeBlockInfoPanelBorder = BoxBorder.None,
            FigureCaption = Style.Plain,
            Footer = Style.Plain,
            Footnote = Style.Plain,
            FootnoteGroup = Style.Plain,
            FootnoteLink = Style.Plain,
            Header = new SpectreTextStyle(),
            Headers = [],
            HtmlBlock = Style.Plain,
            HtmlInline = Style.Plain,
            IncludeDebug = false,
            Inserted = Style.Plain,
            Italic = Style.Plain,
            Marked = Style.Plain,
            MathBlock = Style.Plain,
            MathBlockLabel = Style.Plain,
            MathBlockLabelText = string.Empty,
            MathBlockPanelBorder = BoxBorder.None,
            MathInline = Style.Plain,
            QuotedBlock = Style.Plain,
            ShowFencedCodeBlockInfo = false,
            SmartyPants = false,
            Strikethrough = Style.Plain,
            Subscript = Style.Plain,
            Superscript = Style.Plain,
            TableBorder = DefaultTableBorder.Default,
            TableBorderStyle = Style.Plain,
            TableExpand = false,
            ThematicBreak = Style.Plain,
            UnknownDelimiterChar = Style.Plain,
            UnknownDelimiterContent = Style.Plain,
            UseTerminalHyperlinks = false,
            WrapHeader = false,
            YamlFrontMatter = Style.Plain,
        };
    }

    private async Task EnsureHeaderFontsLoadedAsync(CancellationToken cancellationToken)
    {
        foreach (var headerStyle in Headers)
        {
            if (headerStyle is SpectreFigletTextStyle figlet)
            {
                await figlet.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        if (Header is SpectreFigletTextStyle headerFiglet)
        {
            await headerFiglet.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    internal static JsonSerializerOptions BuildEffectiveOptions(JsonSerializerOptions? caller, Func<SpectreDisplayOptions>? createObject = null)
    {
        bool hasHeader = false;
        bool hasColor = false;
        bool hasBoxBorder = false;
        bool hasTableBorder = false;
        bool hasStyle = false;
        bool hasOptions = false;

        JsonSerializerOptions result;
        if (caller is not null)
        {
            foreach (var converter in caller.Converters)
            {
                hasHeader |= converter is JsonConverter<ISpectreHeaderStyle>;
                hasColor |= converter is JsonConverter<Color>;
                hasBoxBorder |= converter is JsonConverter<BoxBorder>;
                hasTableBorder |= converter is JsonConverter<TableBorder>;
                hasStyle |= converter is JsonConverter<Style>;
                hasOptions |= converter is JsonConverter<SpectreDisplayOptions>;

                if (converter is SpectreDisplayOptionsJsonConverter spectreDisplayOptionsJsonConverter)
                {
                    if (spectreDisplayOptionsJsonConverter.CreateObjectFunction != createObject)
                    {
                        throw new InvalidOperationException($"Caller provided a {nameof(SpectreDisplayOptionsJsonConverter)} with a different {nameof(SpectreDisplayOptionsJsonConverter.CreateObjectFunction)} than the {nameof(createObject)}.");
                    }
                }
                else if (converter is JsonConverter<SpectreDisplayOptions> && createObject is not null)
                {
                    throw new InvalidOperationException($"Caller provided a {nameof(JsonConverter<SpectreDisplayOptions>)} that is not a {nameof(SpectreDisplayOptionsJsonConverter)} with a {nameof(createObject)}.");
                }
            }

            if ( hasHeader && hasColor && hasBoxBorder && hasTableBorder && hasStyle && hasOptions)
            {
                return caller;
            }
            result = new JsonSerializerOptions(caller);
        }
        else
        {
            result = new JsonSerializerOptions();
        }

        if (!hasHeader)
        {
            result.Converters.Add(new SpectreHeaderStyleJsonConverter());
        }
        if (!hasColor)
        {
            result.Converters.Add(new ColorJsonConverter());
        }
        if (!hasBoxBorder)
        {
            result.Converters.Add(new BoxBorderJsonConverter());
        }   
        if (!hasTableBorder)
        {
            result.Converters.Add(new TableBorderJsonConverter());
        }
        if (!hasStyle)
        {
            result.Converters.Add(new StyleJsonConverter());
        }
        if (!hasOptions)
        {
            result.Converters.Add(new SpectreDisplayOptionsJsonConverter(createObject));
        }
        return result;
    }

    internal static IReadOnlyDictionary<string, Action<SpectreDisplayOptions, JsonSerializerOptions, JsonElement>> Deserializers 
        = new Dictionary<string, Action<SpectreDisplayOptions, JsonSerializerOptions, JsonElement>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(AbbreviationTitle)] = (options, jsonOptions, element) => options.AbbreviationTitle = element.Deserialize<Style>(jsonOptions),
            [nameof(AlertCaution)] = (options, jsonOptions, element) => options.AlertCaution = element.Deserialize<Style>(jsonOptions),
            [nameof(AlertImportant)] = (options, jsonOptions, element) => options.AlertImportant = element.Deserialize<Style>(jsonOptions),
            [nameof(AlertNote)] = (options, jsonOptions, element) => options.AlertNote = element.Deserialize<Style>(jsonOptions),
            [nameof(AlertPanelBorder)] = (options, jsonOptions, element) => options.AlertPanelBorder = element.Deserialize<BoxBorder>(jsonOptions) ?? BoxBorder.Rounded,
            [nameof(AlertTip)] = (options, jsonOptions, element) => options.AlertTip = element.Deserialize<Style>(jsonOptions),
            [nameof(AlertWarning)] = (options, jsonOptions, element) => options.AlertWarning = element.Deserialize<Style>(jsonOptions),
            [nameof(Bold)] = (options, jsonOptions, element) => options.Bold = element.Deserialize<Style>(jsonOptions),
            [nameof(Citation)] = (options, jsonOptions, element) => options.Citation = element.Deserialize<Style>(jsonOptions),
            [nameof(CodeBlock)] = (options, jsonOptions, element) => options.CodeBlock = element.Deserialize<Style>(jsonOptions),
            [nameof(CodeInLine)] = (options, jsonOptions, element) => options.CodeInLine = element.Deserialize<Style>(jsonOptions),
            [nameof(CustomContainer)] = (options, jsonOptions, element) => options.CustomContainer = element.Deserialize<Style>(jsonOptions),
            [nameof(CustomContainerInfo)] = (options, jsonOptions, element) => options.CustomContainerInfo = element.Deserialize<Style>(jsonOptions),
            [nameof(CustomContainerInline)] = (options, jsonOptions, element) => options.CustomContainerInline = element.Deserialize<Style>(jsonOptions),
            [nameof(DefinitionItem)] = (options, jsonOptions, element) => options.DefinitionItem = element.Deserialize<Style>(jsonOptions),
            [nameof(DefinitionList)] = (options, jsonOptions, element) => options.DefinitionList = element.Deserialize<Style>(jsonOptions),
            [nameof(DefinitionTerm)] = (options, jsonOptions, element) => options.DefinitionTerm = element.Deserialize<Style>(jsonOptions),
            [nameof(Emojis)] = (options, jsonOptions, element) => options.Emojis = element.GetBoolean(),
            [nameof(FencedCodeBlockInfo)] = (options, jsonOptions, element) => options.FencedCodeBlockInfo = element.Deserialize<Style>(jsonOptions),
            [nameof(FencedCodeBlockInfoPanelBorder)] = (options, jsonOptions, element) => options.FencedCodeBlockInfoPanelBorder = element.Deserialize<BoxBorder>(jsonOptions) ?? BoxBorder.Rounded,
            [nameof(FigureCaption)] = (options, jsonOptions, element) => options.FigureCaption = element.Deserialize<Style>(jsonOptions),
            [nameof(Footer)] = (options, jsonOptions, element) => options.Footer = element.Deserialize<Style>(jsonOptions),
            [nameof(Footnote)] = (options, jsonOptions, element) => options.Footnote = element.Deserialize<Style>(jsonOptions),
            [nameof(FootnoteGroup)] = (options, jsonOptions, element) => options.FootnoteGroup = element.Deserialize<Style>(jsonOptions),
            [nameof(FootnoteLink)] = (options, jsonOptions, element) => options.FootnoteLink = element.Deserialize<Style>(jsonOptions),
            [nameof(Header)] = (options, jsonOptions, element) => options.Header = element.Deserialize<ISpectreHeaderStyle>(jsonOptions) ?? new SpectreTextStyle(),
            [nameof(Headers)] = (options, jsonOptions, element) => options.Headers = element.Deserialize<List<ISpectreHeaderStyle>>(jsonOptions) ?? [],
            [nameof(HtmlBlock)] = (options, jsonOptions, element) => options.HtmlBlock = element.Deserialize<Style>(jsonOptions),
            [nameof(HtmlInline)] = (options, jsonOptions, element) => options.HtmlInline = element.Deserialize<Style>(jsonOptions),
            [nameof(IncludeDebug)] = (options, jsonOptions, element) => options.IncludeDebug = element.GetBoolean(),
            [nameof(Inserted)] = (options, jsonOptions, element) => options.Inserted = element.Deserialize<Style>(jsonOptions),
            [nameof(Italic)] = (options, jsonOptions, element) => options.Italic = element.Deserialize<Style>(jsonOptions),
            [nameof(Marked)] = (options, jsonOptions, element) => options.Marked = element.Deserialize<Style>(jsonOptions),
            [nameof(MathBlock)] = (options, jsonOptions, element) => options.MathBlock = element.Deserialize<Style>(jsonOptions),
            [nameof(MathBlockLabel)] = (options, jsonOptions, element) => options.MathBlockLabel = element.Deserialize<Style>(jsonOptions),
            [nameof(MathBlockLabelText)] = (options, jsonOptions, element) => options.MathBlockLabelText = element.GetString() ?? string.Empty,
            [nameof(MathBlockPanelBorder)] = (options, jsonOptions, element) => options.MathBlockPanelBorder = element.Deserialize<BoxBorder>(jsonOptions) ?? BoxBorder.Rounded,
            [nameof(MathInline)] = (options, jsonOptions, element) => options.MathInline = element.Deserialize<Style>(jsonOptions),
            [nameof(QuotedBlock)] = (options, jsonOptions, element) => options.QuotedBlock = element.Deserialize<Style>(jsonOptions),
            [nameof(ShowFencedCodeBlockInfo)] = (options, jsonOptions, element) => options.ShowFencedCodeBlockInfo = element.GetBoolean(),
            [nameof(SmartyPants)] = (options, jsonOptions, element) => options.SmartyPants = element.GetBoolean(),
            [nameof(Strikethrough)] = (options, jsonOptions, element) => options.Strikethrough = element.Deserialize<Style>(jsonOptions),
            [nameof(Subscript)] = (options, jsonOptions, element) => options.Subscript = element.Deserialize<Style>(jsonOptions),
            [nameof(Superscript)] = (options, jsonOptions, element) => options.Superscript = element.Deserialize<Style>(jsonOptions),
            [nameof(TableBorder)] = (options, jsonOptions, element) => options.TableBorder = element.Deserialize<TableBorder>(jsonOptions) ?? DefaultTableBorder.Default,
            [nameof(TableBorderStyle)] = (options, jsonOptions, element) => options.TableBorderStyle = element.Deserialize<Style>(jsonOptions),
            [nameof(TableExpand)] = (options, jsonOptions, element) => options.TableExpand = element.GetBoolean(),
            [nameof(ThematicBreak)] = (options, jsonOptions, element) => options.ThematicBreak = element.Deserialize<Style>(jsonOptions),
            [nameof(UnknownDelimiterChar)] = (options, jsonOptions, element) => options.UnknownDelimiterChar = element.Deserialize<Style>(jsonOptions),
            [nameof(UnknownDelimiterContent)] = (options, jsonOptions, element) => options.UnknownDelimiterContent = element.Deserialize<Style>(jsonOptions),
            [nameof(UseTerminalHyperlinks)] = (options, jsonOptions, element) => options.UseTerminalHyperlinks = element.GetBoolean(),
            [nameof(WrapHeader)] = (options, jsonOptions, element) => options.WrapHeader = element.GetBoolean(),
            [nameof(YamlFrontMatter)] = (options, jsonOptions, element) => options.YamlFrontMatter = element.Deserialize<Style>(jsonOptions), 
        };

    internal static IReadOnlyList<Action<SpectreDisplayOptions, Utf8JsonWriter, JsonSerializerOptions>> Serializers
        = [
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(AbbreviationTitle), options.AbbreviationTitle),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(AlertCaution), options.AlertCaution),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(AlertImportant), options.AlertImportant),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(AlertNote), options.AlertNote),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(AlertPanelBorder), options.AlertPanelBorder),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(AlertTip), options.AlertTip),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(AlertWarning), options.AlertWarning),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Bold), options.Bold),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Citation), options.Citation),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(CodeBlock), options.CodeBlock),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(CodeInLine), options.CodeInLine),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(CustomContainer), options.CustomContainer),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(CustomContainerInfo), options.CustomContainerInfo),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(CustomContainerInline), options.CustomContainerInline),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(DefinitionItem), options.DefinitionItem),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(DefinitionList), options.DefinitionList),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(DefinitionTerm), options.DefinitionTerm),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Emojis), options.Emojis),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(FencedCodeBlockInfo), options.FencedCodeBlockInfo),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(FencedCodeBlockInfoPanelBorder), options.FencedCodeBlockInfoPanelBorder),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(FigureCaption), options.FigureCaption),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Footer), options.Footer),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Footnote), options.Footnote),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(FootnoteGroup), options.FootnoteGroup),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(FootnoteLink), options.FootnoteLink),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Header), options.Header),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Headers), options.Headers),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(HtmlBlock), options.HtmlBlock),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(HtmlInline), options.HtmlInline),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(IncludeDebug), options.IncludeDebug),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Inserted), options.Inserted),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Italic), options.Italic),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Marked), options.Marked),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(MathBlock), options.MathBlock),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(MathBlockLabel), options.MathBlockLabel),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(MathBlockLabelText), options.MathBlockLabelText),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(MathBlockPanelBorder), options.MathBlockPanelBorder),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(MathInline), options.MathInline),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(QuotedBlock), options.QuotedBlock),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(ShowFencedCodeBlockInfo), options.ShowFencedCodeBlockInfo),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(SmartyPants), options.SmartyPants),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Strikethrough), options.Strikethrough),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Subscript), options.Subscript),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(Superscript), options.Superscript),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(TableBorder), options.TableBorder),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(TableBorderStyle), options.TableBorderStyle),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(TableExpand), options.TableExpand),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(ThematicBreak), options.ThematicBreak),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(UnknownDelimiterChar), options.UnknownDelimiterChar),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(UnknownDelimiterContent), options.UnknownDelimiterContent),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(UseTerminalHyperlinks), options.UseTerminalHyperlinks),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(WrapHeader), options.WrapHeader),
            (options, writer, jsonOptions) => JsonWriteHelpers.WriteProperty(writer, jsonOptions, nameof(YamlFrontMatter), options.YamlFrontMatter),
        ];
}