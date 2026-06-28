namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct |
    AttributeTargets.Interface | AttributeTargets.Enum |
    AttributeTargets.Delegate,
    AllowMultiple = false,
    Inherited = false)]
[SpectreSourceFile]
internal sealed class SpectreSourceFileAttribute([System.Runtime.CompilerServices.CallerFilePath] string filePath = "") : Attribute
{
    public string FilePath { get; } = filePath;
}