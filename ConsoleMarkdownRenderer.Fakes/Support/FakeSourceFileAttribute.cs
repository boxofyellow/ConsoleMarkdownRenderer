namespace BoxOfYellow.ConsoleMarkdownRenderer.Fakes.Support
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Struct |
        AttributeTargets.Interface | AttributeTargets.Enum |
        AttributeTargets.Delegate,
        AllowMultiple = false,
        Inherited = false)]
    [FakeSourceFile]
    internal sealed class FakeSourceFileAttribute([System.Runtime.CompilerServices.CallerFilePath] string filePath = "") : Attribute
    {
        public string FilePath { get; } = filePath;
    }
}