using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling;

/// <summary>
/// Represents a color for console text rendering.
/// Can be a named color or an RGB value.
/// </summary>
[SourceFile]
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

    public static TextColor FromRgb(byte r, byte g, byte b) => new(r, g, b);

    internal static TextColor FromNamed(NamedColor named) => new(named);

    internal bool IsDefault() => this == Default;

    public static TextColor Default { get; } = new(NamedColor.Default);

    // Standard 16 ANSI colors
    public static TextColor Black { get; } = new(NamedColor.Black);
    public static TextColor Maroon { get; } = new(NamedColor.Maroon);
    public static TextColor Green { get; } = new(NamedColor.Green);
    public static TextColor Olive { get; } = new(NamedColor.Olive);
    public static TextColor Navy { get; } = new(NamedColor.Navy);
    public static TextColor Purple { get; } = new(NamedColor.Purple);
    public static TextColor Teal { get; } = new(NamedColor.Teal);
    public static TextColor Silver { get; } = new(NamedColor.Silver);
    public static TextColor Grey { get; } = new(NamedColor.Grey);
    public static TextColor Red { get; } = new(NamedColor.Red);
    public static TextColor Lime { get; } = new(NamedColor.Lime);
    public static TextColor Yellow { get; } = new(NamedColor.Yellow);
    public static TextColor Blue { get; } = new(NamedColor.Blue);
    public static TextColor Fuchsia { get; } = new(NamedColor.Fuchsia);
    public static TextColor Aqua { get; } = new(NamedColor.Aqua);
    public static TextColor White { get; } = new(NamedColor.White);

    // Reds, pinks, oranges
    public static TextColor DarkRed { get; } = new(NamedColor.DarkRed);
    public static TextColor Red1 { get; } = new(NamedColor.Red1);
    public static TextColor IndianRed { get; } = new(NamedColor.IndianRed);
    public static TextColor Salmon1 { get; } = new(NamedColor.Salmon1);
    public static TextColor LightSalmon1 { get; } = new(NamedColor.LightSalmon1);
    public static TextColor LightCoral { get; } = new(NamedColor.LightCoral);
    public static TextColor OrangeRed1 { get; } = new(NamedColor.OrangeRed1);
    public static TextColor DarkOrange { get; } = new(NamedColor.DarkOrange);
    public static TextColor Orange1 { get; } = new(NamedColor.Orange1);
    public static TextColor Tan { get; } = new(NamedColor.Tan);
    public static TextColor SandyBrown { get; } = new(NamedColor.SandyBrown);
    public static TextColor Pink1 { get; } = new(NamedColor.Pink1);
    public static TextColor HotPink { get; } = new(NamedColor.HotPink);
    public static TextColor DeepPink1 { get; } = new(NamedColor.DeepPink1);
    public static TextColor Magenta1 { get; } = new(NamedColor.Magenta1);
    public static TextColor MediumVioletRed { get; } = new(NamedColor.MediumVioletRed);

    // Yellows
    public static TextColor Gold1 { get; } = new(NamedColor.Gold1);
    public static TextColor Yellow1 { get; } = new(NamedColor.Yellow1);
    public static TextColor Khaki1 { get; } = new(NamedColor.Khaki1);
    public static TextColor Wheat1 { get; } = new(NamedColor.Wheat1);
    public static TextColor LightYellow3 { get; } = new(NamedColor.LightYellow3);
    public static TextColor LightGoldenrod1 { get; } = new(NamedColor.LightGoldenrod1);
    public static TextColor GreenYellow { get; } = new(NamedColor.GreenYellow);

    // Greens
    public static TextColor DarkGreen { get; } = new(NamedColor.DarkGreen);
    public static TextColor Green1 { get; } = new(NamedColor.Green1);
    public static TextColor SeaGreen1 { get; } = new(NamedColor.SeaGreen1);
    public static TextColor SpringGreen1 { get; } = new(NamedColor.SpringGreen1);
    public static TextColor MediumSpringGreen { get; } = new(NamedColor.MediumSpringGreen);
    public static TextColor LightGreen { get; } = new(NamedColor.LightGreen);
    public static TextColor PaleGreen1 { get; } = new(NamedColor.PaleGreen1);
    public static TextColor DarkSeaGreen { get; } = new(NamedColor.DarkSeaGreen);

    // Cyans / aquas
    public static TextColor Cyan1 { get; } = new(NamedColor.Cyan1);
    public static TextColor Turquoise2 { get; } = new(NamedColor.Turquoise2);
    public static TextColor MediumTurquoise { get; } = new(NamedColor.MediumTurquoise);
    public static TextColor DarkTurquoise { get; } = new(NamedColor.DarkTurquoise);
    public static TextColor LightSeaGreen { get; } = new(NamedColor.LightSeaGreen);
    public static TextColor Aquamarine1 { get; } = new(NamedColor.Aquamarine1);

    // Blues
    public static TextColor NavyBlue { get; } = new(NamedColor.NavyBlue);
    public static TextColor DarkBlue { get; } = new(NamedColor.DarkBlue);
    public static TextColor Blue1 { get; } = new(NamedColor.Blue1);
    public static TextColor RoyalBlue1 { get; } = new(NamedColor.RoyalBlue1);
    public static TextColor DodgerBlue1 { get; } = new(NamedColor.DodgerBlue1);
    public static TextColor SteelBlue { get; } = new(NamedColor.SteelBlue);
    public static TextColor CornflowerBlue { get; } = new(NamedColor.CornflowerBlue);
    public static TextColor SkyBlue1 { get; } = new(NamedColor.SkyBlue1);
    public static TextColor LightSkyBlue1 { get; } = new(NamedColor.LightSkyBlue1);
    public static TextColor LightSteelBlue { get; } = new(NamedColor.LightSteelBlue);
    public static TextColor DeepSkyBlue1 { get; } = new(NamedColor.DeepSkyBlue1);
    public static TextColor DeepSkyBlue4 { get; } = new(NamedColor.DeepSkyBlue4);

    // Purples / violets
    public static TextColor SlateBlue1 { get; } = new(NamedColor.SlateBlue1);
    public static TextColor BlueViolet { get; } = new(NamedColor.BlueViolet);
    public static TextColor DarkViolet { get; } = new(NamedColor.DarkViolet);
    public static TextColor MediumPurple { get; } = new(NamedColor.MediumPurple);
    public static TextColor MediumOrchid { get; } = new(NamedColor.MediumOrchid);
    public static TextColor Violet { get; } = new(NamedColor.Violet);
    public static TextColor Orchid { get; } = new(NamedColor.Orchid);
    public static TextColor Plum1 { get; } = new(NamedColor.Plum1);
    public static TextColor Thistle1 { get; } = new(NamedColor.Thistle1);

    // Greys
    public static TextColor Grey0 { get; } = new(NamedColor.Grey0);
    public static TextColor Grey30 { get; } = new(NamedColor.Grey30);
    public static TextColor Grey50 { get; } = new(NamedColor.Grey50);
    public static TextColor Grey62 { get; } = new(NamedColor.Grey62);
    public static TextColor Grey70 { get; } = new(NamedColor.Grey70);
    public static TextColor Grey85 { get; } = new(NamedColor.Grey85);
    public static TextColor Grey100 { get; } = new(NamedColor.Grey100);

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
        => IsRgb ? HashCode.Combine(IsRgb, R, G, B)
                 : HashCode.Combine(IsRgb, Named);

    public override string ToString()
        => IsRgb ? $"rgb({R},{G},{B})" : Named.ToString();

    public static bool TryParseColor(string? colorString, [NotNullWhen(true)] out TextColor? color)
    {
        color = null;
        if (string.IsNullOrWhiteSpace(colorString))
        {
            return false;
        }

        if (colorString.Equals(nameof(Default), StringComparison.OrdinalIgnoreCase))
        {
            color = Default;
            return true;
        }

        if (DisplayMappings.Colors.Forward.TryGetValue(colorString, out var namedColor))
        {
            color = namedColor;
            return true;
        }

        if (TryFromHex(colorString, out var hexColor))
        {
            color = hexColor;
            return true;
        }

        if (Utilities.TryParseRgb(colorString, out var r, out var g, out var b))
        {
            color = FromRgb(r, g, b);
            return true;
        }

        return false;
    }

    private static bool TryFromHex(string hex, [NotNullWhen(true)] out TextColor? color)
    {
        color = null;
        // Lifted and shifted from Spectre.Console's Color.FromHex()
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        // 3 digit hex codes are expanded to 6 digits
        // by doubling each digit, conform to CSS color codes
        if (hex.Length == 3)
        {
            hex = string.Concat(hex.Select(c => new string(c, 2)));
        }

        if (hex.Length < 6)
        {
            return false;
        }

        if (byte.TryParse(hex.AsSpan(0, 2), NumberStyles.HexNumber, provider: null, out var r)
        && byte.TryParse(hex.AsSpan(2, 2), NumberStyles.HexNumber, provider: null, out var g)
        && byte.TryParse(hex.AsSpan(4, 2), NumberStyles.HexNumber, provider: null, out var b))
        {
            color = FromRgb(r, g, b);
            return true;
        }

        return false;
    }
}

/// <summary>
/// Named colors mapped to Spectre.Console's <see cref="Spectre.Console.Color"/> palette.
/// Each value matches a public static property on <see cref="Spectre.Console.Color"/> with
/// the same name; lower-casing the value also yields a valid Spectre.Console markup color
/// name (e.g. <c>NamedColor.DarkOrange</c> ↔ <c>Color.DarkOrange</c> ↔ markup <c>"darkorange"</c>).
/// </summary>
[SourceFile]
public enum NamedColor
{
    Default,

    // Standard 16 ANSI colors
    Black,
    Maroon,
    Green,
    Olive,
    Navy,
    Purple,
    Teal,
    Silver,
    Grey,
    Red,
    Lime,
    Yellow,
    Blue,
    Fuchsia,
    Aqua,
    White,

    // Reds, pinks, oranges
    DarkRed,
    Red1,
    IndianRed,
    Salmon1,
    LightSalmon1,
    LightCoral,
    OrangeRed1,
    DarkOrange,
    Orange1,
    Tan,
    SandyBrown,
    Pink1,
    HotPink,
    DeepPink1,
    Magenta1,
    MediumVioletRed,

    // Yellows
    Gold1,
    Yellow1,
    Khaki1,
    Wheat1,
    LightYellow3,
    LightGoldenrod1,
    GreenYellow,

    // Greens
    DarkGreen,
    Green1,
    SeaGreen1,
    SpringGreen1,
    MediumSpringGreen,
    LightGreen,
    PaleGreen1,
    DarkSeaGreen,

    // Cyans / aquas
    Cyan1,
    Turquoise2,
    MediumTurquoise,
    DarkTurquoise,
    LightSeaGreen,
    Aquamarine1,

    // Blues
    NavyBlue,
    DarkBlue,
    Blue1,
    RoyalBlue1,
    DodgerBlue1,
    SteelBlue,
    CornflowerBlue,
    SkyBlue1,
    LightSkyBlue1,
    LightSteelBlue,
    DeepSkyBlue1,
    DeepSkyBlue4,

    // Purples / violets
    SlateBlue1,
    BlueViolet,
    DarkViolet,
    MediumPurple,
    MediumOrchid,
    Violet,
    Orchid,
    Plum1,
    Thistle1,

    // Greys
    Grey0,
    Grey30,
    Grey50,
    Grey62,
    Grey70,
    Grey85,
    Grey100,
}
