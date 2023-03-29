using System.Diagnostics.CodeAnalysis;

namespace AtomReaderNet
{
    /// <summary>
    /// Represents a character and its position in a file. Note that any equality comparison will ignore line and column values, and only compare char values
    /// </summary>
    public readonly struct Atom
    {
        /// <summary>
        /// The line from where the Atom was read
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// The column from where the Atom was read
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// The char value of the atom
        /// </summary>
        public char Value { get; }

        /// <summary>
        /// Given a position and a value, constructs an atom
        /// </summary>
        public Atom(int line, int column, char value)
        {
            Line = line;
            Column = column;
            Value = value;
        }

        /// <summary>
        /// Returns true if the Atom represents a whitespace
        /// </summary>
        public bool IsWhiteSpace => char.IsWhiteSpace(Value);

        /// <summary>
        /// Returns true if the Atom represents a number
        /// </summary>
        public bool IsNumber => char.IsNumber(Value);

#if NET6_0_OR_GREATER
        /// <summary>
        /// Returns true if the Atom represents an ASCII value
        /// </summary>
        public bool IsAscii => char.IsAscii(Value);
#endif

        /// <summary>
        /// Returns true if the Atom represents a digit
        /// </summary>
        public bool IsDigit => char.IsDigit(Value);

        /// <summary>
        /// Returns true if the Atom represents a letter
        /// </summary>
        public bool IsLetter => char.IsLetter(Value);

        /// <summary>
        /// Returns true if the Atom represents a lower case letter
        /// </summary>
        public bool IsLower => char.IsLower(Value);

        /// <summary>
        /// Returns true if the Atom represents an upper case letter
        /// </summary>
        public bool IsUpper => char.IsUpper(Value);

#if NET7_0_OR_GREATER

        /// <summary>
        /// Returns true if the Atom is at or within the bounds
        /// </summary>
        public bool IsBetween(char lowerBound, char upperBound) =>
            char.IsBetween(Value, lowerBound, upperBound);
#endif

        /// <summary>
        /// Converts the atom to lower case, with the same line and column as the original
        /// </summary>
        public Atom ToLower() => new Atom(Line, Column, char.ToLower(Value));


        /// <summary>
        /// Converts the atom to upper case, with the same line and column as the original
        /// </summary>
        public Atom ToUpper() => new Atom(Line, Column, char.ToUpper(Value));

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <inheritdoc/>
#if NET6_0_OR_GREATER
        public override bool Equals([NotNullWhen(true)] object? obj)
#else
        public override bool Equals(object obj)
#endif
        {
            return (obj is Atom a && a.Value == Value) || (obj is char c && c == this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Value} ({Column}@{Line})";
        }

        /// <summary>
        /// Converts the Atom to a char value
        /// </summary>
        public static implicit operator char(Atom a) => a.Value;

        /// <summary>
        /// Compares the <see cref="Value"/> properties of the two instances
        /// </summary>
        public static bool operator ==(Atom a, Atom b) => a.Value == b.Value;

        /// <summary>
        /// Compares the <see cref="Value"/> properties of the two instances
        /// </summary>
        public static bool operator !=(Atom a, Atom b) => a.Value != b.Value;

        /// <summary>
        /// Compares the <see cref="Value"/> properties of the Atom with the char value
        /// </summary>
        public static bool operator ==(Atom a, char c) => a.Value == c;

        /// <summary>
        /// Compares the <see cref="Value"/> properties of the Atom with the char value
        /// </summary>
        public static bool operator !=(Atom a, char c) => a.Value != c;
    }
}
