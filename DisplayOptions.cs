using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer;

/// <summary>
/// Class for controlling the styling and other display options for the Markdown elements 
/// </summary>
[SourceFile]
public sealed class DisplayOptions
{
    public TextStyle AbbreviationTitle { get; set; } = c_defaultSpectreOptions.AbbreviationTitle.ToTextStyle(preferNullColors: true);
    public TextStyle AlertCaution { get; set; } = c_defaultSpectreOptions.AlertCaution.ToTextStyle(preferNullColors: true);
    public TextStyle AlertImportant { get; set; } = c_defaultSpectreOptions.AlertImportant.ToTextStyle(preferNullColors: true);
    public TextStyle AlertNote { get; set; } = c_defaultSpectreOptions.AlertNote.ToTextStyle(preferNullColors: true);
    public RuleBorder AlertPanelBorder { get; set; } = c_defaultSpectreOptions.AlertPanelBorder.ToRuleBorder();
    public TextStyle AlertTip { get; set; } = c_defaultSpectreOptions.AlertTip.ToTextStyle(preferNullColors: true);
    public TextStyle AlertWarning { get; set; } = c_defaultSpectreOptions.AlertWarning.ToTextStyle(preferNullColors: true);
    public TextStyle Bold { get; set; } = c_defaultSpectreOptions.Bold.ToTextStyle(preferNullColors: true);
    public TextStyle Citation { get; set; } = c_defaultSpectreOptions.Citation.ToTextStyle(preferNullColors: true);
    public TextStyle CodeBlock { get; set; } = c_defaultSpectreOptions.CodeBlock.ToTextStyle(preferNullColors: true);
    public TextStyle CodeInLine { get; set; } = c_defaultSpectreOptions.CodeInLine.ToTextStyle(preferNullColors: true);
    public TextStyle CustomContainer { get; set; } = c_defaultSpectreOptions.CustomContainer.ToTextStyle(preferNullColors: true);
    public TextStyle CustomContainerInfo { get; set; } = c_defaultSpectreOptions.CustomContainerInfo.ToTextStyle(preferNullColors: true);
    public TextStyle CustomContainerInline { get; set; } = c_defaultSpectreOptions.CustomContainerInline.ToTextStyle(preferNullColors: true);
    public TextStyle DefinitionItem { get; set; } = c_defaultSpectreOptions.DefinitionItem.ToTextStyle(preferNullColors: true);
    public TextStyle DefinitionList { get; set; } = c_defaultSpectreOptions.DefinitionList.ToTextStyle(preferNullColors: true);
    public TextStyle DefinitionTerm { get; set; } = c_defaultSpectreOptions.DefinitionTerm.ToTextStyle(preferNullColors: true);
    public bool ShowFencedCodeBlockInfo { get; set; } = c_defaultSpectreOptions.ShowFencedCodeBlockInfo;
    public TextStyle FencedCodeBlockInfo { get; set; } = c_defaultSpectreOptions.FencedCodeBlockInfo.ToTextStyle(preferNullColors: true);
    public TextStyle FigureCaption { get; set; } = c_defaultSpectreOptions.FigureCaption.ToTextStyle(preferNullColors: true);
    public List<IHeaderStyle> Headers { get; set; } = [.. c_defaultSpectreOptions.Headers.Select(h => h.ToHeaderStyle())];
    public IHeaderStyle Header { get; set; } = c_defaultSpectreOptions.Header.ToHeaderStyle();
    public TextStyle HtmlBlock { get; set; } = c_defaultSpectreOptions.HtmlBlock.ToTextStyle(preferNullColors: true);
    public TextStyle HtmlInline { get; set; } = c_defaultSpectreOptions.HtmlInline.ToTextStyle(preferNullColors: true);
    public TextStyle Footer { get; set; } = c_defaultSpectreOptions.Footer.ToTextStyle(preferNullColors: true);
    public TextStyle Footnote { get; set; } = c_defaultSpectreOptions.Footnote.ToTextStyle(preferNullColors: true);
    public TextStyle FootnoteGroup { get; set; } = c_defaultSpectreOptions.FootnoteGroup.ToTextStyle(preferNullColors: true);
    public TextStyle FootnoteLink { get; set; } = c_defaultSpectreOptions.FootnoteLink.ToTextStyle(preferNullColors: true);
    public bool Emojis { get; set; } = c_defaultSpectreOptions.Emojis;
    public TextStyle Inserted { get; set; } = c_defaultSpectreOptions.Inserted.ToTextStyle(preferNullColors: true);
    public TextStyle Italic { get; set; } = c_defaultSpectreOptions.Italic.ToTextStyle(preferNullColors: true);
    public TextStyle Marked { get; set; } = c_defaultSpectreOptions.Marked.ToTextStyle(preferNullColors: true);
    public TextStyle MathBlock { get; set; } = c_defaultSpectreOptions.MathBlock.ToTextStyle(preferNullColors: true);
    public TextStyle MathBlockLabel { get; set; } = c_defaultSpectreOptions.MathBlockLabel.ToTextStyle(preferNullColors: true);
    public string MathBlockLabelText { get; set; } = c_defaultSpectreOptions.MathBlockLabelText;
    public TextStyle MathInline { get; set; } = c_defaultSpectreOptions.MathInline.ToTextStyle(preferNullColors: true);
    public TextStyle QuotedBlock { get; set; } = c_defaultSpectreOptions.QuotedBlock.ToTextStyle(preferNullColors: true);
    public bool SmartyPants { get; set; } = c_defaultSpectreOptions.SmartyPants;
    public TextStyle Strikethrough { get; set; } = c_defaultSpectreOptions.Strikethrough.ToTextStyle(preferNullColors: true);
    public TextStyle ThematicBreak { get; set; } = c_defaultSpectreOptions.ThematicBreak.ToTextStyle(preferNullColors: true);
    public TextTableBorder TableBorder { get; set; } = c_defaultSpectreOptions.TableBorder.ToTextTableBorder();
    public TextStyle TableBorderStyle { get; set; } = c_defaultSpectreOptions.TableBorderStyle.ToTextStyle(preferNullColors: true);
    public bool TableExpand { get; set; } = c_defaultSpectreOptions.TableExpand;
    public TextStyle Subscript { get; set; } = c_defaultSpectreOptions.Subscript.ToTextStyle(preferNullColors: true);
    public TextStyle Superscript { get; set; } = c_defaultSpectreOptions.Superscript.ToTextStyle(preferNullColors: true);
    public TextStyle UnknownDelimiterChar { get; set; } = c_defaultSpectreOptions.UnknownDelimiterChar.ToTextStyle(preferNullColors: true);
    public TextStyle UnknownDelimiterContent { get; set; } = c_defaultSpectreOptions.UnknownDelimiterContent.ToTextStyle(preferNullColors: true);
    public TextStyle YamlFrontMatter { get; set; } = c_defaultSpectreOptions.YamlFrontMatter.ToTextStyle(preferNullColors: true);
    public bool WrapHeader { get; set; } = c_defaultSpectreOptions.WrapHeader;
    public bool UseTerminalHyperlinks { get; set; } = c_defaultSpectreOptions.UseTerminalHyperlinks;
    public bool IncludeDebug { get; set; } = c_defaultSpectreOptions.IncludeDebug;

    public DisplayOptions Clone() => new()
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

    /// <summary>
    /// Serializes this <see cref="DisplayOptions"/> to JSON using the converters honored
    /// by <see cref="DeserializeAsync(string, JsonSerializerOptions?, CancellationToken)"/>
    /// so that the result round-trips back to an equivalent <see cref="DisplayOptions"/>
    /// instance.
    /// </summary>
    /// <param name="options">
    /// Optional caller-supplied <see cref="JsonSerializerOptions"/>. Pass an instance to
    /// control output settings such as <see cref="JsonSerializerOptions.WriteIndented"/>;
    /// the library copies the provided options and adds the converters required to
    /// serialize <see cref="DisplayOptions"/>, so the caller's instance is not mutated.
    /// When <see langword="null"/> (the default) compact JSON is emitted with the
    /// library's default settings.
    /// </param>
    public string Serialize(JsonSerializerOptions? options = null)
        => JsonSerializer.Serialize(this, BuildEffectiveOptions(options));

    /// <summary>
    /// Deserializes a <see cref="DisplayOptions"/> from a JSON <paramref name="json"/>
    /// string and awaits <see cref="FigletTextStyle.EnsureFontLoadedAsync"/> on every
    /// <see cref="FigletTextStyle"/> in <see cref="Headers"/> and <see cref="Header"/>
    /// before returning, so the result is ready to hand to a renderer.
    /// </summary>
    /// <param name="json">The JSON text to deserialize.</param>
    /// <param name="options">
    /// Optional caller-supplied <see cref="JsonSerializerOptions"/>. The library copies
    /// the provided options and adds the converters required to deserialize a
    /// <see cref="DisplayOptions"/>, so the caller's instance is not mutated. When
    /// <see langword="null"/> (the default) the library's default settings are used.
    /// </param>
    /// <param name="cancellationToken">A token to observe while loading FIGlet fonts.</param>
    public static async Task<DisplayOptions> DeserializeAsync(
        string json,
        JsonSerializerOptions? options = null,
        Func<DisplayOptions>? createObject = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(json);
        var result = JsonSerializer.Deserialize<DisplayOptions>(json, BuildEffectiveOptions(options, createObject))
            ?? throw new JsonException($"{nameof(DisplayOptions)} JSON deserialized to null.");
        await EnsureHeaderFontsLoadedAsync(result, cancellationToken).ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Deserializes a <see cref="DisplayOptions"/> from a UTF-8 JSON
    /// <paramref name="utf8Json"/> stream and awaits
    /// <see cref="FigletTextStyle.EnsureFontLoadedAsync"/> on every
    /// <see cref="FigletTextStyle"/> in <see cref="Headers"/> and <see cref="Header"/>
    /// before returning, so the result is ready to hand to a renderer.
    /// </summary>
    /// <param name="utf8Json">The UTF-8 JSON stream to deserialize.</param>
    /// <param name="options">
    /// Optional caller-supplied <see cref="JsonSerializerOptions"/>. The library copies
    /// the provided options and adds the converters required to deserialize a
    /// <see cref="DisplayOptions"/>, so the caller's instance is not mutated. When
    /// <see langword="null"/> (the default) the library's default settings are used.
    /// </param>
    /// <param name="cancellationToken">A token to observe while reading the stream and loading FIGlet fonts.</param>
    public static async Task<DisplayOptions> DeserializeAsync(
        Stream utf8Json,
        JsonSerializerOptions? options = null,
        Func<DisplayOptions>? createObject = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(utf8Json);
        var result = await JsonSerializer.DeserializeAsync<DisplayOptions>(utf8Json, BuildEffectiveOptions(options, createObject), cancellationToken).ConfigureAwait(false)
            ?? throw new JsonException($"{nameof(DisplayOptions)} JSON deserialized to null.");
        await EnsureHeaderFontsLoadedAsync(result, cancellationToken).ConfigureAwait(false);
        return result;
    }

    private static async Task EnsureHeaderFontsLoadedAsync(DisplayOptions options, CancellationToken cancellationToken)
    {
        foreach (var headerStyle in options.Headers)
        {
            if (headerStyle is FigletTextStyle figlet)
            {
                await figlet.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        if (options.Header is FigletTextStyle headerFiglet)
        {
            await headerFiglet.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    internal static JsonSerializerOptions BuildEffectiveOptions(JsonSerializerOptions? caller, Func<DisplayOptions>? createObject = null)
    {
        bool hasHeader = false;
        bool hasColor = false;
        bool hasStyle = false;
        bool hasOptions = false;

        JsonSerializerOptions result;
        if (caller is not null)
        {
            foreach (var converter in caller.Converters)
            {
                hasHeader |= converter is JsonConverter<IHeaderStyle>;
                hasColor |= converter is JsonConverter<TextColor>;
                hasStyle |= converter is JsonConverter<TextStyle>;
                hasOptions |= converter is JsonConverter<DisplayOptions>;

                if (converter is DisplayOptionsJsonConverter displayOptionsJsonConverter)
                {
                    if (displayOptionsJsonConverter.CreateObjectFunction != createObject)
                    {
                        throw new InvalidOperationException($"Caller provided a {nameof(DisplayOptionsJsonConverter)} with a different {nameof(DisplayOptionsJsonConverter.CreateObjectFunction)} than the {nameof(createObject)}.");
                    }
                }
                else if (converter is JsonConverter<DisplayOptions> && createObject is not null)
                {
                    throw new InvalidOperationException($"Caller provided a {nameof(JsonConverter<DisplayOptions>)} that is not a {nameof(DisplayOptionsJsonConverter)} with a {nameof(createObject)}.");
                }
            }
            if (hasHeader && hasColor && hasStyle && hasOptions)
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
            result.Converters.Add(new HeaderStyleJsonConverter());
        }
        if (!hasColor)
        {
            result.Converters.Add(new TextColorJsonConverter());
        }
        if (!hasStyle)
        {
            result.Converters.Add(new TextStyleJsonConverter());
        }
        if (!hasOptions)
        {
            result.Converters.Add(new DisplayOptionsJsonConverter(createObject));
        }
        return result;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DisplayOptions other)
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

    public static DisplayOptions Empty() => FromSpectreOptions(SpectreDisplayOptions.Empty(), preferNullColors: true);

    internal static IReadOnlyDictionary<string, Action<DisplayOptions, JsonSerializerOptions, JsonElement>> Deserializers 
        = new Dictionary<string, Action<DisplayOptions, JsonSerializerOptions, JsonElement>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(AbbreviationTitle)] = (options, jsonOptions, element) => options.AbbreviationTitle = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(AbbreviationTitle)),
            [nameof(AlertCaution)] = (options, jsonOptions, element) => options.AlertCaution = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(AlertCaution)),
            [nameof(AlertImportant)] = (options, jsonOptions, element) => options.AlertImportant = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(AlertImportant)),
            [nameof(AlertNote)] = (options, jsonOptions, element) => options.AlertNote = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(AlertNote)),
            [nameof(AlertPanelBorder)] = (options, jsonOptions, element) => options.AlertPanelBorder = element.Deserialize<RuleBorder>(jsonOptions),
            [nameof(AlertTip)] = (options, jsonOptions, element) => options.AlertTip = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(AlertTip)),
            [nameof(AlertWarning)] = (options, jsonOptions, element) => options.AlertWarning = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(AlertWarning)),
            [nameof(Bold)] = (options, jsonOptions, element) => options.Bold = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Bold)),
            [nameof(Citation)] = (options, jsonOptions, element) => options.Citation = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Citation)),
            [nameof(CodeBlock)] = (options, jsonOptions, element) => options.CodeBlock = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(CodeBlock)),
            [nameof(CodeInLine)] = (options, jsonOptions, element) => options.CodeInLine = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(CodeInLine)),
            [nameof(CustomContainer)] = (options, jsonOptions, element) => options.CustomContainer = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(CustomContainer)),
            [nameof(CustomContainerInfo)] = (options, jsonOptions, element) => options.CustomContainerInfo = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(CustomContainerInfo)),
            [nameof(CustomContainerInline)] = (options, jsonOptions, element) => options.CustomContainerInline = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(CustomContainerInline)),
            [nameof(DefinitionItem)] = (options, jsonOptions, element) => options.DefinitionItem = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(DefinitionItem)),
            [nameof(DefinitionList)] = (options, jsonOptions, element) => options.DefinitionList = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(DefinitionList)),
            [nameof(DefinitionTerm)] = (options, jsonOptions, element) => options.DefinitionTerm = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(DefinitionTerm)),
            [nameof(Emojis)] = (options, jsonOptions, element) => options.Emojis = element.GetBoolean(),
            [nameof(FencedCodeBlockInfo)] = (options, jsonOptions, element) => options.FencedCodeBlockInfo = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(FencedCodeBlockInfo)),
            [nameof(FigureCaption)] = (options, jsonOptions, element) => options.FigureCaption = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(FigureCaption)),
            [nameof(Footer)] = (options, jsonOptions, element) => options.Footer = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Footer)),
            [nameof(Footnote)] = (options, jsonOptions, element) => options.Footnote = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Footnote)),
            [nameof(FootnoteGroup)] = (options, jsonOptions, element) => options.FootnoteGroup = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(FootnoteGroup)),
            [nameof(FootnoteLink)] = (options, jsonOptions, element) => options.FootnoteLink = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(FootnoteLink)),
            [nameof(Header)] = (options, jsonOptions, element) => options.Header = element.Deserialize<IHeaderStyle>(jsonOptions) ?? new TextStyle(),
            [nameof(Headers)] = (options, jsonOptions, element) => options.Headers = element.Deserialize<List<IHeaderStyle>>(jsonOptions) ?? [],
            [nameof(HtmlBlock)] = (options, jsonOptions, element) => options.HtmlBlock = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(HtmlBlock)),
            [nameof(HtmlInline)] = (options, jsonOptions, element) => options.HtmlInline = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(HtmlInline)),
            [nameof(IncludeDebug)] = (options, jsonOptions, element) => options.IncludeDebug = element.GetBoolean(),
            [nameof(Inserted)] = (options, jsonOptions, element) => options.Inserted = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Inserted)),
            [nameof(Italic)] = (options, jsonOptions, element) => options.Italic = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Italic)),
            [nameof(Marked)] = (options, jsonOptions, element) => options.Marked = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Marked)),
            [nameof(MathBlock)] = (options, jsonOptions, element) => options.MathBlock = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(MathBlock)),
            [nameof(MathBlockLabel)] = (options, jsonOptions, element) => options.MathBlockLabel = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(MathBlockLabel)),
            [nameof(MathBlockLabelText)] = (options, jsonOptions, element) => options.MathBlockLabelText = element.GetString() ?? string.Empty,
            [nameof(MathInline)] = (options, jsonOptions, element) => options.MathInline = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(MathInline)),
            [nameof(QuotedBlock)] = (options, jsonOptions, element) => options.QuotedBlock = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(QuotedBlock)),
            [nameof(ShowFencedCodeBlockInfo)] = (options, jsonOptions, element) => options.ShowFencedCodeBlockInfo = element.GetBoolean(),
            [nameof(SmartyPants)] = (options, jsonOptions, element) => options.SmartyPants = element.GetBoolean(),
            [nameof(Strikethrough)] = (options, jsonOptions, element) => options.Strikethrough = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Strikethrough)),
            [nameof(Subscript)] = (options, jsonOptions, element) => options.Subscript = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Subscript)),
            [nameof(Superscript)] = (options, jsonOptions, element) => options.Superscript = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(Superscript)),
            [nameof(TableBorder)] = (options, jsonOptions, element) => options.TableBorder = element.Deserialize<TextTableBorder>(jsonOptions),
            [nameof(TableBorderStyle)] = (options, jsonOptions, element) => options.TableBorderStyle = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(TableBorderStyle)),
            [nameof(TableExpand)] = (options, jsonOptions, element) => options.TableExpand = element.GetBoolean(),
            [nameof(ThematicBreak)] = (options, jsonOptions, element) => options.ThematicBreak = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(ThematicBreak)),
            [nameof(UnknownDelimiterChar)] = (options, jsonOptions, element) => options.UnknownDelimiterChar = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(UnknownDelimiterChar)),
            [nameof(UnknownDelimiterContent)] = (options, jsonOptions, element) => options.UnknownDelimiterContent = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(UnknownDelimiterContent)),
            [nameof(UseTerminalHyperlinks)] = (options, jsonOptions, element) => options.UseTerminalHyperlinks = element.GetBoolean(),
            [nameof(WrapHeader)] = (options, jsonOptions, element) => options.WrapHeader = element.GetBoolean(),
            [nameof(YamlFrontMatter)] = (options, jsonOptions, element) => options.YamlFrontMatter = element.Deserialize<TextStyle>(jsonOptions).AssertDeserializationIsNotNull(nameof(YamlFrontMatter)), 
        };

    internal static IReadOnlyList<Action<DisplayOptions, Utf8JsonWriter, JsonSerializerOptions>> Serializers
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

    internal SpectreDisplayOptions ToSpectreOptions() => new()
    {
        AbbreviationTitle = this.AbbreviationTitle.ToSpectreStyle(),
        AlertCaution = this.AlertCaution.ToSpectreStyle(),
        AlertImportant = this.AlertImportant.ToSpectreStyle(),
        AlertNote = this.AlertNote.ToSpectreStyle(),
        AlertPanelBorder = this.AlertPanelBorder.ToSpectreBoxBorder(),
        AlertTip = this.AlertTip.ToSpectreStyle(),
        AlertWarning = this.AlertWarning.ToSpectreStyle(),
        Bold = this.Bold.ToSpectreStyle(),
        Citation = this.Citation.ToSpectreStyle(),
        CodeBlock = this.CodeBlock.ToSpectreStyle(),
        CodeInLine = this.CodeInLine.ToSpectreStyle(),
        CustomContainer = this.CustomContainer.ToSpectreStyle(),
        CustomContainerInfo = this.CustomContainerInfo.ToSpectreStyle(),
        CustomContainerInline = this.CustomContainerInline.ToSpectreStyle(),
        DefinitionItem = this.DefinitionItem.ToSpectreStyle(),
        DefinitionList = this.DefinitionList.ToSpectreStyle(),
        DefinitionTerm = this.DefinitionTerm.ToSpectreStyle(),
        Emojis = this.Emojis,
        FencedCodeBlockInfo = this.FencedCodeBlockInfo.ToSpectreStyle(),
        FigureCaption = this.FigureCaption.ToSpectreStyle(),
        Footer = this.Footer.ToSpectreStyle(),
        Footnote = this.Footnote.ToSpectreStyle(),
        FootnoteGroup = this.FootnoteGroup.ToSpectreStyle(),
        FootnoteLink = this.FootnoteLink.ToSpectreStyle(),
        Header = this.Header.ToSpectreHeaderStyle(),
        Headers = [.. this.Headers.Select(h => h.ToSpectreHeaderStyle())],
        HtmlBlock = this.HtmlBlock.ToSpectreStyle(),
        HtmlInline = this.HtmlInline.ToSpectreStyle(),
        IncludeDebug = this.IncludeDebug,
        Inserted = this.Inserted.ToSpectreStyle(),
        Italic = this.Italic.ToSpectreStyle(),
        Marked = this.Marked.ToSpectreStyle(),
        MathBlock = this.MathBlock.ToSpectreStyle(),
        MathBlockLabel = this.MathBlockLabel.ToSpectreStyle(),
        MathBlockLabelText = this.MathBlockLabelText,
        MathInline = this.MathInline.ToSpectreStyle(),
        QuotedBlock = this.QuotedBlock.ToSpectreStyle(),
        ShowFencedCodeBlockInfo = this.ShowFencedCodeBlockInfo,
        SmartyPants = this.SmartyPants,
        Strikethrough = this.Strikethrough.ToSpectreStyle(),
        Subscript = this.Subscript.ToSpectreStyle(),
        Superscript = this.Superscript.ToSpectreStyle(),
        TableBorder = this.TableBorder.ToSpectreTableBorder(),
        TableBorderStyle = this.TableBorderStyle.ToSpectreStyle(),
        TableExpand = this.TableExpand,
        ThematicBreak = this.ThematicBreak.ToSpectreStyle(),
        UnknownDelimiterChar = this.UnknownDelimiterChar.ToSpectreStyle(),
        UnknownDelimiterContent = this.UnknownDelimiterContent.ToSpectreStyle(),
        UseTerminalHyperlinks = this.UseTerminalHyperlinks,
        WrapHeader = this.WrapHeader,
        YamlFrontMatter = this.YamlFrontMatter.ToSpectreStyle(),
    };

    internal static DisplayOptions FromSpectreOptions(SpectreDisplayOptions spectreOptions, bool preferNullColors = false) => new()
    {
        AbbreviationTitle = spectreOptions.AbbreviationTitle.ToTextStyle(preferNullColors),
        AlertCaution = spectreOptions.AlertCaution.ToTextStyle(preferNullColors),
        AlertImportant = spectreOptions.AlertImportant.ToTextStyle(preferNullColors),
        AlertNote = spectreOptions.AlertNote.ToTextStyle(preferNullColors),
        AlertPanelBorder = spectreOptions.AlertPanelBorder.ToRuleBorder(),
        AlertTip = spectreOptions.AlertTip.ToTextStyle(preferNullColors),
        AlertWarning = spectreOptions.AlertWarning.ToTextStyle(preferNullColors),
        Bold = spectreOptions.Bold.ToTextStyle(preferNullColors),
        Citation = spectreOptions.Citation.ToTextStyle(preferNullColors),
        CodeBlock = spectreOptions.CodeBlock.ToTextStyle(preferNullColors),
        CodeInLine = spectreOptions.CodeInLine.ToTextStyle(preferNullColors),
        CustomContainer = spectreOptions.CustomContainer.ToTextStyle(preferNullColors),
        CustomContainerInfo = spectreOptions.CustomContainerInfo.ToTextStyle(preferNullColors),
        CustomContainerInline = spectreOptions.CustomContainerInline.ToTextStyle(preferNullColors),
        DefinitionItem = spectreOptions.DefinitionItem.ToTextStyle(preferNullColors),
        DefinitionList = spectreOptions.DefinitionList.ToTextStyle(preferNullColors),
        DefinitionTerm = spectreOptions.DefinitionTerm.ToTextStyle(preferNullColors),
        Emojis = spectreOptions.Emojis,
        FencedCodeBlockInfo = spectreOptions.FencedCodeBlockInfo.ToTextStyle(preferNullColors),
        FigureCaption = spectreOptions.FigureCaption.ToTextStyle(preferNullColors),
        Footer = spectreOptions.Footer.ToTextStyle(preferNullColors),
        Footnote = spectreOptions.Footnote.ToTextStyle(preferNullColors),
        FootnoteGroup = spectreOptions.FootnoteGroup.ToTextStyle(preferNullColors),
        FootnoteLink = spectreOptions.FootnoteLink.ToTextStyle(preferNullColors),
        Header = spectreOptions.Header.ToHeaderStyle(),
        Headers = [.. spectreOptions.Headers.Select(h => h.ToHeaderStyle())],
        HtmlBlock = spectreOptions.HtmlBlock.ToTextStyle(preferNullColors),
        HtmlInline = spectreOptions.HtmlInline.ToTextStyle(preferNullColors),
        IncludeDebug = spectreOptions.IncludeDebug,
        Inserted = spectreOptions.Inserted.ToTextStyle(preferNullColors),
        Italic = spectreOptions.Italic.ToTextStyle(preferNullColors),
        Marked = spectreOptions.Marked.ToTextStyle(preferNullColors),
        MathBlock = spectreOptions.MathBlock.ToTextStyle(preferNullColors),
        MathBlockLabel = spectreOptions.MathBlockLabel.ToTextStyle(preferNullColors),
        MathBlockLabelText = spectreOptions.MathBlockLabelText,
        MathInline = spectreOptions.MathInline.ToTextStyle(preferNullColors),
        QuotedBlock = spectreOptions.QuotedBlock.ToTextStyle(preferNullColors),
        ShowFencedCodeBlockInfo = spectreOptions.ShowFencedCodeBlockInfo,
        SmartyPants = spectreOptions.SmartyPants,
        Strikethrough = spectreOptions.Strikethrough.ToTextStyle(preferNullColors),
        Subscript = spectreOptions.Subscript.ToTextStyle(preferNullColors),
        Superscript = spectreOptions.Superscript.ToTextStyle(preferNullColors),
        TableBorder = spectreOptions.TableBorder.ToTextTableBorder(),
        TableBorderStyle = spectreOptions.TableBorderStyle.ToTextStyle(preferNullColors),
        TableExpand = spectreOptions.TableExpand,
        ThematicBreak = spectreOptions.ThematicBreak.ToTextStyle(preferNullColors),
        UnknownDelimiterChar = spectreOptions.UnknownDelimiterChar.ToTextStyle(preferNullColors),
        UnknownDelimiterContent = spectreOptions.UnknownDelimiterContent.ToTextStyle(preferNullColors),
        UseTerminalHyperlinks = spectreOptions.UseTerminalHyperlinks,
        WrapHeader = spectreOptions.WrapHeader,
        YamlFrontMatter = spectreOptions.YamlFrontMatter.ToTextStyle(preferNullColors)
    };

    private static readonly SpectreDisplayOptions c_defaultSpectreOptions = new();
}