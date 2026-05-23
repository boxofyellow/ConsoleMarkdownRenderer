---
title: An Example Document
author: ConsoleMarkdownRenderer
tags:
  - markdown
  - console
---

# An Example header

This is an example markdown file that demos different supported features

1. Ordered lists
1. Here is an item with sub nodes
   1. These are ordered
      - These items are an un ordered list
      - A second item
   1. more items
1. Check box items
   - [x] Checked
   - [ ] Unchecked
1. Item with formatting
   - **bold**
   - _italic_
   - ~~strike through~~
   - ~subscript~
   - ^superscript^
   - ++inserted++
   - ==marked==
   - `inline code`
   - inline <b>html</b> tags <br/>
1. Links
   * With a label: [www.google.com](https://www.google.com)
   * no formatting at all: www.bing.com
   * no formatting but contains schema: http://github.com
   * link to this file [example](example.md)
   * other file [nest.md](sub/nest.md)
   * [](sub/lists.md)
   * [](sub/text.txt)
   * https://github.com/xoofx/markdig
   * https://spectreconsole.net/
   * https://github.com/adamfisher/RomanNumeral
   * [test gist](https://gist.githubusercontent.com/boxofyellow/dbddb3d120cdd806afb5e3bad8b069e3/raw/cd401aed633da852d7acfa758d8bdea76c02004b/gistfile1.txt)
   * [markdown on the web](https://raw.githubusercontent.com/boxofyellow/ConsoleMarkdownRenderer/main/ConsoleMarkdownRenderer.Example/data/example.md)
   * Just links <https://www.reddit.com/>
1. Images
   - ![](sub/xray.jpg)
   - ![image from the web](https://gist.githubusercontent.com/boxofyellow/dbddb3d120cdd806afb5e3bad8b069e3/raw/257ca135b5936416389f2ff8996e4693a36dce0e/img.jpg)

# Block formatting

## tables

| header column 1 | header column two | other column |
| - | - | - |
| (1,1) | (1,2) | 3 |
| (one,2 )| (1,two) | three |

## Code Blocks

```csharp
// C# example - use --show-code-info to see the language info
Console.WriteLine("Hello, World!");
```

```
This is a code block
It has a few lines
1. Item with formatting, that is not rendered
   - **bold**
   - _italic_
   - ~~strike through~~
   - ~subscript~
   - ^superscript^
   - ++inserted++
   - ==marked==
```

## Format blocks

    And this should
    Be Considered
    Blocked Formatted
    1. Item with formatting
       - **bold**
       - _italic_
       - ~~strike through~~
       - ~subscript~
       - ^superscript^
       - ++inserted++
       - ==marked==

## Format quote block

> And this a quote
> It has a few lines
> That has Some lists
> 1. Item one in the list
> 1. Item two with some `code`
>    - Child Item
> 1. Item three
>    - A Child item with [a link](https://www.some.place.com)
> 1. Items with formatting
>    - **bold**
>    - _italic_
>    - ~~strike through~~
>    - ~subscript~
>    - ^superscript^
>    - ++inserted++
>    - ==marked==

## Footnotes

Footnote references are placed inline[^example] and the rendered footnotes appear at the bottom of the document[^longer-footnote]. The same footnote can be referenced multiple times[^example].

[^example]: A short footnote.

[^longer-footnote]: A longer footnote with **bold**, *italic*, and `inline code` content.

## Custom Containers (Admonitions)

Custom containers represent admonitions / callouts commonly used in technical documentation.

:::note
This is a *note* admonition with **bold** content.
:::

:::warning
A multi-line warning that contains:

- a list item
- another item with `inline code`
:::

A paragraph with an inline ::tag inline container:: example.

## Abbreviations

The HTML specification is maintained by the W3C and uses CSS for styling.

*[HTML]: HyperText Markup Language
*[W3C]: World Wide Web Consortium
*[CSS]: Cascading Style Sheets

## Definition Lists

Term
:   A word or phrase to be defined.

Markdown
:   A lightweight markup language for creating formatted text.
:   Widely used in documentation and README files.

HTML
:   HyperText Markup Language.
:   The standard language for creating web pages.

## Figures

Figures group an image (or other content) with an optional caption.

^^^ A diagram illustrating the Markdown figure syntax.
![diagram](http://example.com/diagram.png)
^^^

## Thematic Break (Horizontal Rule)

---

## HTML Entities

Here are some common HTML entities: &amp; &lt; &gt; &copy; &reg; &euro; &hellip;

## Emojis

Emoji shortcodes and text smileys are substituted with their Unicode equivalents: :smile: :rocket: :burrito: :-) :-(

Inside code spans (`:smile:`) and code blocks, the original text is preserved:

```
:smile: :-)
```

## SmartyPants

ASCII punctuation in prose is rewritten with typographic equivalents: straight "quotes" become curly quotes, `--` becomes an en-dash, `---` becomes an em-dash, and trailing `...` becomes an ellipsis. She said "Hello" -- this is 'great' --- with so on...

Inside code spans (`"verbatim" -- 'no change' ...`) and code blocks, the original ASCII punctuation is preserved:

```
"verbatim" -- 'no change' ...
```

## HTML (Just gets blocked out)

<table>
    <thead>
        <tr>
            <th>h1</th>
            <th>h2</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td>d1</td>
            <td>d2</td>
        </tr>
        <tr>
            <td>D1</td>
            <td>D2</td>
        </tr>
    </tbody>
</table>

Some text after the block

## Mathematics (LaTeX source rendered verbatim)

Markdig parses inline math like $E = mc^2$ and block math like

$$
\int_0^1 x^2 dx = \frac{1}{3}
$$

into AST nodes. Terminals cannot typeset LaTeX, so the raw source is rendered with a distinctive style.

### TODOs

- [x] Code Blocks
  - ~~making the background adhere to a block shape~~ https://github.com/spectreconsole/spectre.console/issues/517
  - ~~Inline code blocks should not get the formatting of the parents, maybe quote parent blocks...~~ maybe I'll do this later
- [x] Debug Mode
- [x] Links
- [x] Quote blocks
- [x] Emojis :burrito:
- [x] HTML
- [x] text formatting
- [x] Basic Tables
- [x] Headers
  - ~~Should we use some ASCII Art~~ getting to text is not that easy
- [x] Test crazy nesting
- [x] Lists
  - ~~Should lists use different numbering options as you nest lists~~ We could alternative, maybe we do that later 
- [x] Footnotes
- [x] Custom containers (admonitions)
- [x] Abbreviations
- [x] Figures
- [x] Mathematics (inline `$...$` and block `$$...$$` LaTeX source)
- [x] Footers
- [x] YAML front matter (rendered as a styled block via `DisplayOptions.YamlFrontMatter`)
- [ ] One to always leave unchecked

And here is the end

^^ Document footer rendered by Markdig's `UseFooters()` extension — useful for *attribution*, **citations**, or other metadata.
^^ Each footer line is prefixed with `^^` in the Markdown source.