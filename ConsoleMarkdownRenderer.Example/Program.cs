using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Spectre.Console.Cli;

namespace ConsoleMarkdownRenderer.Example
{
    class Program
    {
        public static int Main(string[] args) => new CommandApp<ExampleCommand>().Run(args);
    }

    class ExampleSettings : CommandSettings
    {
        [CommandArgument(0, "[-p|--path]")]
        public string? Path { get; init; }

        [CommandOption("-i|--ingore-links")]
        [DefaultValue(false)]
        public bool IgnoreLinks { get; init; }

        [CommandOption("-d|--include-debug")]
        [DefaultValue(false)]
        public bool InclideDebug { get; init; }
    }

    class ExampleCommand : Command<ExampleSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] ExampleSettings settings)
        {
            var path = settings.Path ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "data", "example.md"));
            var allowFollingLinks = !settings.IgnoreLinks;
            var includeDebug = settings.InclideDebug;

            Uri uri;
            try
            {
                uri = new Uri(path);
            }
            catch (UriFormatException)
            {
                uri = new Uri(Path.GetFullPath(path));
            }

            if (context.Remaining.Raw.Count > 0 || context.Remaining.Parsed.Count > 0 || (uri.IsFile && !File.Exists(uri.LocalPath)))
            {
                uri = new Uri(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "usage.md")));
                allowFollingLinks = true;
                includeDebug = false;
            }

            Displayer.DisplayMarkdown(
                uri,
                allowFollingLinks,
                includeDebug);
            return 0;
        }
    }
}
