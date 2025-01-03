
[![Build Status](https://github.com/boxofyellow/ConsoleMarkdownRenderer/actions/workflows/ci.yml/badge.svg)](https://github.com/boxofyellow/ConsoleMarkdownRenderer/actions/workflows/ci.yml) [![codecov](https://codecov.io/gh/boxofyellow/ConsoleMarkdownRenderer/branch/main/graph/badge.svg?token=2VSOFO21BN)](https://codecov.io/gh/boxofyellow/ConsoleMarkdownRenderer) [![NuGet](https://img.shields.io/nuget/v/BoxOfYellow.ConsoleMarkdownRenderer.svg)](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer)

# _I_ have markdown files, _you_ have markdown files we _all_ have markdown files...

We create them to document various parts of projects.  Sometimes that documentation would be helpful _while_ folks are using those projects.  And thats where this library comes in.  This Library provides support for displaying markdown within the console and provides a simple navigation list of links and images within the document.  When items from the list are selected their content will be shown inline when possible (aka it's another markdown file, or it's an image and the console appears to be using [iTerm2]((https://iterm2.com/)))

I will totally admit `README.md` files and response that is displayed with `--help` are not 100% interchangeable, but there is a lot of overlap :slightly_smiling_face:

## Using it is simple
Just call the one public method from the static [Displayer.cs](Displayer.cs) class called `DisplayMarkdownAsync` it accepts the following parameters

| name | type | description | required/default |
| - | - | - | - |
| `uri` | `Uri` | The [Uri](https://en.wikipedia.org/wiki/Uniform_Resource_Identifier) that is either a file containing your markdown, or the web address where said content can be downloaded | Yes |
| `options` | `DisplayOptions` | Properties and styles to apply to the Markdown elements | no / `null` |
| `allowFollowingLinks` | `bool` | A flag, when set to true, the list of links will be provided, when false the list is omitted | no / `true` |

It has a second overload

| name | type | description | required/default |
| - | - | - | - |
| `text` | `string` | the text to display | Yes |
| `uriBase` | `Uri` | The [Uri](https://en.wikipedia.org/wiki/Uniform_Resource_Identifier) base for all links | no / the current working directory |
| `options` | `DisplayOptions` | Properties and styles to apply to the Markdown elements | no / `null` |
| `allowFollowingLinks` | `bool` | A flag, when set to true, the list of links will be provided, when false the list is omitted | no / `true` |

Checkout [ConsoleMarkdownRenderer.Example](ConsoleMarkdownRenderer.Example) to see it in use
![](docs/example.png)

## Default Styling

The defaults for the Styling for the Markdown elements can be seen in the examples listed above.  The details for that style can be changed by creating an instances of [DisplayOptions](DisplayOptions.cs) and overwriting any that you see fit.

This object is more or less a bag of styles to use for the various parts of you markdown document.  There are few exceptions

| name | type | description | default
| - | - | - | - |
| `Headers` | `List<Style>` | Used as overrides of `Header`, an order lists of styles to use for different level of headers | fall back to `Header` / empty |
| `WrapHeader` | `bool` | When `true`, will wrap Headers with `#`'s to denote the level | yes / `true` |
| `IncludeDebug` | `bool` | When `true` will display all content within in boxes to help visualize how the content is being interpreted by the tool | off / `false` |

## Supporting packages 

It's also important to give credit where credit is due, this library is really just glue for the following packages
- [Markdig](https://www.nuget.org/packages/Markdig/) for parsing the markdown
- [Spectre.Console](https://www.nuget.org/packages/Spectre.Console/) for display rich formatting within the console
- [RomanNumeral](https://www.nuget.org/packages/RomanNumeral/) for minimal roman numeral processing

## Contributing

Contributions are welcome, please see [CONTRIBUTING.md](CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md)
