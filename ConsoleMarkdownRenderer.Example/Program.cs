using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ConsoleMarkdownRenderer.Example
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var command = new CommandApp<ExampleCommand>();
            command.Configure(x => x.UseStrictParsing().PropagateExceptions());
            try
            {
                return command.Run(args);
            }
            catch(Exception ex)
            {
                AnsiConsole.WriteException(ex);
                await Displayer.DisplayMarkdownAsync(new Uri(Path.Combine(AppContext.BaseDirectory, "usage.md")));
                return -1;
            }
        }
    }

    class ExampleSettings : CommandSettings
    {
        [CommandArgument(0, "[path]")]
        public string? Path { get; init; }

        [CommandOption("-i|--ignore-links")]
        [DefaultValue(false)]
        public bool IgnoreLinks { get; init; }

        [CommandOption("-d|--include-debug")]
        [DefaultValue(false)]
        public bool IncludeDebug { get; init; }

        [CommandOption("-r|--remove-header-wrap")]
        [DefaultValue(false)]
        public bool RemoveHeaderWrap { get; init; }

        [CommandOption("-w|--web")]
        [DefaultValue(false)]
        public bool UseWeb { get; init; }

        [CommandOption("--render-only")]
        [DefaultValue(false)]
        [Description("Use the IMarkdownRenderer API to render without interactive display")]
        public bool RenderOnly { get; init; }
    }

    class ExampleCommand : AsyncCommand<ExampleSettings>
    {
        protected override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] ExampleSettings settings, CancellationToken cancellationToken)
        {
            var path = settings.Path
                ?? (settings.UseWeb
                    ? "https://raw.githubusercontent.com/boxofyellow/ConsoleMarkdownRenderer/main/ConsoleMarkdownRenderer.Example/data/example.md"
                    : Path.Combine(AppContext.BaseDirectory, "data", "example.md"));

            Uri uri;
            try
            {
                uri = new Uri(path);
            }
            catch (UriFormatException)
            {
                uri = new Uri(Path.GetFullPath(path));
            }

            DisplayOptions options = new()
            {
                IncludeDebug = settings.IncludeDebug,
                WrapHeader = !settings.RemoveHeaderWrap,
            };

            if (settings.RenderOnly)
            {
                // Demo: Using the IMarkdownRenderer interface (instance-based API)
                IMarkdownRenderer renderer = new MarkdownDisplayer();
                var text = await File.ReadAllTextAsync(uri.IsFile ? uri.LocalPath : path, cancellationToken);
                var result = renderer.RenderMarkdown(text, options);

                AnsiConsole.WriteLine(result.RenderedText);
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine($"[bold]Found {result.Links.Count} link(s):[/]");
                foreach (var link in result.Links)
                {
                    AnsiConsole.MarkupLine($"  - {Markup.Escape(link.ToString())}");
                }

                if (result.UnhandledTypes is not null && result.UnhandledTypes.Count > 0)
                {
                    AnsiConsole.MarkupLine($"[yellow]Unhandled types: {string.Join(", ", result.UnhandledTypes.Select(t => t.Name))}[/]");
                }
            }
            else
            {
                // Demo: Using the IMarkdownDisplayer interface (instance-based API)
                IMarkdownDisplayer displayer = new MarkdownDisplayer();
                await displayer.DisplayMarkdownAsync(
                    uri,
                    options,
                    allowFollowingLinks: !settings.IgnoreLinks);
            }

            return 0;
        }
    }
}
