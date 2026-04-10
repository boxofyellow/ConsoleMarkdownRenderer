# Usage for ConsoleMarkdownRender.Example

This little application will display the contents of markdown files, and accepts the following optional command line arguments.

| Command Line Argument | Description |
| - | - |
| unnamed | The path to where the markdown content can be found. It can be a relative (or absolute) file path, or any valid Uri. If not provided, this defaults to the contents of `data/example.md`; unless `--web` is provided, it will use the default content from the web. |
| `-i`/`--ignore--links` | Used to suppress the list of links found within the document. |
| `-d`/`--include-debug` | Include debug information |
| `-r`/`--remove-header-wrap` | Remove the `#` that wrap headers |
| `-w`/`--web` | When specified (and `--path` is not) content from [the source repo](https://github.com/boxofyellow/ConsoleMarkdownRenderer) will be displayed |

