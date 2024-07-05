# Usage for ConsoleMarkdownRender.Example

The little application will display the contents of markdown file, and accepts the follow optional command line argument

| Command Line Argument | Description |
| - | - |
| `-p`/`--path` | The path to where the markdown content can be found, it can be relative (or absolute) file path, or any valid Uri. If not proved this default the contents of `data/example.md` unless the `--web` is provided, in will use the default content from the web |
| `-i`/`--ignore--links` | Used to suppress the list of links found within the document. |
| `-d`/`--include-debug` | Include debug information |
| `-r`/`--remove-header-wrap` | Remove the `#` that wrap headers |
| `-w`/`--web` | When specified (and `--path` is not) content from [the source repo](https://github.com/boxofyellow/ConsoleMarkdownRenderer) will be displayed |

