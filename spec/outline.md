# Document outline: ATX headings

The V1 outline recognizes ATX headings with one to six opening hashes followed
by a space or tab. Zero to three leading ASCII spaces are allowed. Four spaces
or a leading tab are not accepted as heading indentation.

An optional final group containing only hashes is removed from the displayed
title when separated from the title by spaces or tabs. Embedded hashes such as
`C#`, `Text#`, or `Text ### remains` stay unchanged. Empty titles are omitted.
Headings inside fenced code blocks remain excluded.

Each entry retains the UTF-16 position of the original line start, including
indentation. Parsing does not modify document content. LF and CRLF documents
produce equivalent titles and levels with their respective original positions.

Setext headings and full nested Markdown container parsing remain outside this
V1 subset.
