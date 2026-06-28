namespace BoxOfYellow.ConsoleMarkdownRenderer.Support;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct |
    AttributeTargets.Interface | AttributeTargets.Enum |
    AttributeTargets.Delegate,
    AllowMultiple = false,
    Inherited = false)]
[SourceFile]
internal sealed class SourceFileAttribute([System.Runtime.CompilerServices.CallerFilePath] string filePath = "") : Attribute
{
    public string FilePath { get; } = filePath;
}