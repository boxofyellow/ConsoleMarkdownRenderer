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

### TODOs

- [x] Code Blocks
  - ~~making the background adhere to a block shape~~ https://github.com/spectreconsole/spectre.console/issues/517
  - ~~Inline code blocks should not get the formatting of the parents, maybe quote parent blocks...~~ maybe I'll do this later
- [x] Debug Mode
- [x] Links
- [x] Quote blocks
- [x] Emojis :burrito: This worked out of the box...
- [x] HTML
- [x] text formatting
- [x] Basic Tables
- [x] Headers
  - ~~Should we use some ASCII Art~~ getting to text is not that easy
- [x] Test crazy nesting
- [x] Lists
  - ~~Should lists use different numbering options as you nest lists~~ We could alternative, maybe we do that later 
- [ ] One to always leave unchecked

And here is the end