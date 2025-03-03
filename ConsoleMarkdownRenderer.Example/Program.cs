﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
    }

    class ExampleCommand : AsyncCommand<ExampleSettings>
    {
        public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] ExampleSettings settings)
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

            await Displayer.DisplayMarkdownAsync(
                uri,
                options,
                allowFollowingLinks: !settings.IgnoreLinks);
            return 0;
        }
    }
}
