# AtomReader

`AtomReader` is a high-performance .NET library designed for forward-only reading of text streams while preserving precise zero-indexed line and column position tracking for every character read. It is ideal for building parsers, lexers, visualizers, compiler frontends, or any text-processing tool where accurate source location metadata is required.

## Key Features

- **Character Position Tracking**: Every character is returned as an `Atom` structure holding its char value along with its 0-indexed `Line` and `Column` coordinates.
- **Line Ending Handling**: Automatically handles Unix (`\n`), Mac (`\r`), and Windows (`\r\n`) newline conventions.
- **`AtomString` Abstraction**: Immutable sequence of `Atom`s maintaining full start and end position range info (`FromLine`, `FromColumn`, `ToLine`, `ToColumn`) with zero-allocation equality operations against regular strings.
- **Flexible Data Sources**: Read from `string`, `Stream`, or `TextReader`.
- **Configurable Buffering**: Internal buffer size can be adjusted to balance memory usage and I/O performance (defaults to 4 KB, max 2 MB).
- **Zero Overhead Char Conversions**: Implicit operators allow `Atom` to be seamlessly evaluated or assigned as `char`, and `AtomString` as `string` or `char[]`.

## Installation

Install via NuGet Package Manager CLI:

```bash
dotnet add package JST.AtomReader
```

Or via Package Manager Console in Visual Studio:

```powershell
Install-Package JST.AtomReader
```

## Quick Start

### Basic Reading with `AtomReader`

```csharp
using System;
using AtomReaderNet;

string input = "Hello\nWorld!";
using var reader = new AtomReader(input);

while (!reader.EndOfStream)
{
    Atom atom = reader.Read();
    Console.WriteLine($"Char: '{atom.Value}' | Line: {atom.Line}, Column: {atom.Column}");
}
```

### Reading Line by Line

```csharp
using System;
using AtomReaderNet;

string input = "Line 1\r\nLine 2";
using var reader = new AtomReader(input);

while (!reader.EndOfStream)
{
    foreach (Atom atom in reader.ReadLine())
    {
        Console.Write(atom.Value);
    }
}
```

### Using `AtomString` for Tokens

`AtomString` groups a sequence of `Atom`s (such as tokens identified during lexing) while retaining the starting and ending positions of the sequence.

```csharp
using System;
using System.Collections.Generic;
using AtomReaderNet;

var atoms = new List<Atom>
{
    new Atom(line: 0, column: 0, 'f'),
    new Atom(line: 0, column: 1, 'o'),
    new Atom(line: 0, column: 2, 'o')
};

var atomStr = new AtomString(atoms);

Console.WriteLine($"Text: {atomStr}");                  // Output: foo
Console.WriteLine($"Start: Line {atomStr.FromLine}, Col {atomStr.FromColumn}"); // Start: Line 0, Col 0
Console.WriteLine($"End:   Line {atomStr.ToLine}, Col {atomStr.ToColumn}");     // End:   Line 0, Col 2

// Zero-allocation string comparison
bool matches = (atomStr == "foo"); // true
```

## API Overview

### `AtomReader`

- `AtomReader(string source)` / `AtomReader(Stream source)` / `AtomReader(TextReader source)`
- `Atom Read()`: Reads and returns the next character atom. Throws `EndOfStreamException` if at end of stream.
- `Atom Peek()`: Returns the next character atom without consuming it.
- `IEnumerable<Atom> ReadLine()`: Reads character atoms until the end of the current line (includes newline characters).
- `IEnumerable<Atom> ReadToEnd()`: Reads all remaining character atoms.
- `AtomReader Precache()`: Pre-caches the next block of characters into the internal buffer.
- `bool EndOfStream`: Returns `true` if all characters have been read.
- `int ReadCount`: Gets the total number of characters consumed so far.
- `int BufferSize`: Gets or sets the reading buffer size in bytes (1 to 2,097,152; default 4096).

### `Atom`

- `int Line`: 0-indexed line number.
- `int Column`: 0-indexed column number.
- `char Value`: The character value.
- Helper properties: `IsWhiteSpace`, `IsNumber`, `IsDigit`, `IsLetter`, `IsLower`, `IsUpper`, `IsAscii`, `IsBetween(char, char)`.
- Helper methods: `ToLower()`, `ToUpper()`.
- Implicit conversions and equality operators for `char`.

### `AtomString`

- `int FromLine`, `int FromColumn`: Position of the first character atom.
- `int ToLine`, `int ToColumn`: Position of the last character atom.
- `int Length`: Character count.
- Implicit conversions to `string` and `char[]`.
- Efficient allocation-free equality comparison operators against `AtomString` and `string`.

## License

This project is licensed under the [MIT License](LICENSE).
