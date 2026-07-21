#if !NET6_0_OR_GREATER
using System;
using System.Linq;
using System.Collections.Generic;
#endif

namespace AtomReaderNet
{
    /// <summary>
    /// Represents a sequence of <see cref="Atom"/> elements
    /// </summary>
    public class AtomString
    {
        /// <summary>
        /// The line the string started on
        /// </summary>
        public int FromLine { get; } = -1;

        /// <summary>
        /// The column the string started on
        /// </summary>
        public int FromColumn { get; } = -1;

        /// <summary>
        /// The line the string ended on
        /// </summary>
        public int ToLine { get; } = -1;

        /// <summary>
        /// The column the string ended on
        /// </summary>
        public int ToColumn { get; } = -1;

        private readonly Atom[] chars;

        /// <summary>
        /// Given a list of atoms, construct an <see cref="AtomString"/> instance
        /// </summary>
        /// <param name="atoms">A list of atoms, must contain at least 1 atom</param>
        public AtomString(IEnumerable<Atom> atoms)
        {
            chars = atoms.ToArray();

            if (chars.Length == 0)
            {
                throw new ArgumentException("Atom list may not be empty", nameof(atoms));
            }

            FromLine = chars[0].Line;
            FromColumn = chars[0].Column;

#if NETSTANDARD2_0
            ToLine = chars[chars.Length - 1].Line;
            ToColumn = chars[chars.Length - 1].Column;
#else
            ToLine = chars[^1].Line;
            ToColumn = chars[^1].Column;
#endif
        }

        /// <summary>
        /// The length of the AtomString in characters
        /// </summary>
        public int Length => chars.Length;

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ((string)this).GetHashCode();
        }

        /// <inheritdoc/>
#if NET6_0_OR_GREATER
        public override bool Equals(object? obj)
#else
        public override bool Equals(object obj)
#endif
        {
            return (obj is AtomString a && a == this) || (obj is string s && s == this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return (string)this;
        }

        /// <summary>
        /// Converts the AtomString instance to a string
        /// </summary>
        public static implicit operator string(AtomString s) =>
            new string(s.chars.Select(c => c.Value).ToArray());

        /// <summary>
        /// Converts the AtomString instance to a char array
        /// </summary>
        public static implicit operator char[](AtomString s) => s.chars.Select(c => c.Value).ToArray();

        /// <summary>
        /// Converts both instances to string and compares them
        /// </summary>
        public static bool operator ==(AtomString a, AtomString b) => ((string)a) == ((string)b);

        /// <summary>
        /// Converts both instances to string and compares them
        /// </summary>
        public static bool operator !=(AtomString a, AtomString b) => !(a == b);

        /// <summary>
        /// Converts the AtomString instance to a string and compares with the other string
        /// </summary>
        public static bool operator ==(AtomString a, string b) => ((string)a) == b;

        /// <summary>
        /// Converts the AtomString instance to a string and compares with the other string
        /// </summary>
        public static bool operator !=(AtomString a, string b) => !(a == b);
    }
}
