using System.Diagnostics.CodeAnalysis;

namespace AtomReaderNet;

/// <summary>
/// Represents a character and its position in a file. Note that any equality comparison will ignore line and column values, and only compare char values
/// </summary
public struct Atom
{
    public int Line { get; }
    public int Column { get; }
    public char Value { get; }

    public Atom(int line, int column, char value)
    {
        Line = line;
        Column = column;
        Value = value;
    }

    public bool IsWhiteSpace => char.IsWhiteSpace(Value);
    public bool IsNumber => char.IsNumber(Value);
    public bool IsAscii => char.IsAscii(Value);
    public bool IsDigit => char.IsDigit(Value);
    public bool IsLetter => char.IsLetter(Value);
    public bool IsLower => char.IsLower(Value);
    public bool IsUpper => char.IsUpper(Value);

    public bool IsBetween(char lowerBound, char upperBound) =>
        char.IsBetween(Value, lowerBound, upperBound);

    public Atom ToLower() => new(Line, Column, char.ToLower(Value));

    public Atom ToUpper() => new(Line, Column, char.ToUpper(Value));

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return (obj is Atom a && a.Value == Value) || (obj is char c && c == this);
    }

    public override string ToString()
    {
        return $"{Value} ({Column}@{Line})";
    }

    public static implicit operator char(Atom a) => a.Value;

    public static bool operator ==(Atom a, Atom b) => a.Value == b.Value;

    public static bool operator !=(Atom a, Atom b) => a.Value != b.Value;

    public static bool operator ==(Atom a, char c) => a.Value == c;

    public static bool operator !=(Atom a, char c) => a.Value != c;
}

public class AtomString
{
    public int FromLine { get; } = -1;
    public int FromColumn { get; } = -1;
    public int ToLine { get; } = -1;
    public int ToColumn { get; } = -1;

    private Atom[] chars;

    public AtomString(IEnumerable<Atom> atoms)
    {
        chars = atoms.ToArray();

        if (chars.Length > 0)
        {
            FromLine = chars[0].Line;
            FromColumn = chars[0].Column;

            ToLine = chars[^1].Line;
            ToColumn = chars[^1].Column;
        }
    }

    public int Length => chars.Length;

    public override int GetHashCode()
    {
        return ((string)this).GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return (obj is AtomString a && a == this) || (obj is string s && s == this);
    }

    public override string ToString()
    {
        return (string)this;
    }

    public static implicit operator string(AtomString s) =>
        new string(s.chars.Select(c => c.Value).ToArray());

    public static bool operator ==(AtomString a, AtomString b) => ((string)a) == ((string)b);

    public static bool operator !=(AtomString a, AtomString b) => !(a == b);

    public static bool operator ==(AtomString a, string b) => ((string)a) == b;

    public static bool operator !=(AtomString a, string b) => !(a == b);
}

public class AtomReader : IDisposable
{
    public bool EndOfStream => cache.Count == 0 && source.Peek() == -1;

    private Queue<Atom> cache = new();
    private int line;
    private int column;

    private readonly TextReader source;

    public AtomReader(string source)
    {
        this.source = new StringReader(source);
    }

    public AtomReader(Stream source)
    {
        this.source = new StreamReader(source);
    }

    public AtomReader(TextReader source)
    {
        this.source = source;
    }

    public Atom Peek()
    {
        EnsureCache();
        return cache.Peek();
    }

    public Atom Read()
    {
        EnsureCache();
        return cache.Dequeue();
    }

    public AtomReader Precache()
    {
        EnsureCache();
        return this;
    }

    public IEnumerable<Atom> ReadToEnd()
    {
        while (!EndOfStream)
        {
            yield return Read();
        }
    }

    public IEnumerable<Atom> ReadLine()
    {
        while (!EndOfStream)
        {
            var next = Read();
            yield return next;
            if (next == '\r' || next == '\n')
            {
                if (next == '\r' && !EndOfStream && Peek() == '\n')
                {
                    yield return Read();
                }

                yield break;
            }
        }
    }

    private void EnsureCache()
    {
        if (cache.Count > 0)
        {
            return;
        }

        if (EndOfStream)
        {
            throw new EndOfStreamException();
        }

        var buffer = new char[4096];

        var read = source.ReadBlock(buffer, 0, buffer.Length);
        for (var i = 0; i < read; i++)
        {
            cache.Enqueue(new(line, column, buffer[i]));
            column++;

            if (buffer[i] == '\r' || buffer[i] == '\n')
            {
                if (buffer[i] == '\r' && i < buffer.Length - 1 && buffer[i + 1] == '\n')
                {
                    cache.Enqueue(new(line, column, buffer[++i]));
                }

                line++;
                column = 0;
            }
        }
    }

    public void Dispose()
    {
        ((IDisposable)source).Dispose();
    }
}
