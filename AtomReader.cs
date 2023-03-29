#if !NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
#endif

namespace AtomReaderNet
{
    /// <summary>
    /// A utility class for forward-only reading of characters from a source
    /// </summary>
    public sealed class AtomReader : IDisposable
    {
        /// <summary>
        /// Returns true of the reader has reached the end of the data
        /// </summary>
        public bool EndOfStream => cache.Count == 0 && source.Peek() == -1;

        private readonly Queue<Atom> cache = new Queue<Atom>();
        private int line;
        private int column;

        private readonly TextReader source;

        /// <summary>
        /// Constructs a reader from a given string
        /// </summary>
        public AtomReader(string source)
        {
            this.source = new StringReader(source);
        }

        /// <summary>
        /// Constructs a reader from a given Stream
        /// </summary>
        public AtomReader(Stream source)
        {
            this.source = new StreamReader(source);
        }

        /// <summary>
        /// Constructs a reader from a given TextReader
        /// </summary>
        public AtomReader(TextReader source)
        {
            this.source = source;
        }

        /// <summary>
        /// Peeks the next Atom from the reader
        /// </summary>
        /// <exception cref="EndOfStream">Thrown if attempting to read past the end of data</exception>
        public Atom Peek()
        {
            EnsureCache();
            return cache.Peek();
        }

        /// <summary>
        /// Reads the next Atom from the reader
        /// </summary>
        /// <exception cref="EndOfStream">Thrown if attempting to read past the end of data</exception>
        public Atom Read()
        {
            EnsureCache();
            return cache.Dequeue();
        }

        /// <summary>
        /// Ensures that a chunk of characters are read and cached from the source
        /// </summary>
        /// <exception cref="EndOfStream">Thrown if attempting to read past the end of data</exception>
        public AtomReader Precache()
        {
            EnsureCache();
            return this;
        }

        /// <summary>
        /// Read all remaining Atoms
        /// </summary>
        /// <exception cref="EndOfStream">Thrown if attempting to read past the end of data</exception>
        public IEnumerable<Atom> ReadToEnd()
        {
            while (!EndOfStream)
            {
                yield return Read();
            }
        }

        /// <summary>
        /// Read all remaining Atoms until next CR, LF, or CRLF character(s). The end-of-line characters will be included
        /// </summary>
        /// <exception cref="EndOfStream">Thrown if attempting to read past the end of data</exception>
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
                cache.Enqueue(new Atom(line, column, buffer[i]));
                column++;

                if (buffer[i] == '\r' || buffer[i] == '\n')
                {
                    if (buffer[i] == '\r' && i < buffer.Length - 1 && buffer[i + 1] == '\n')
                    {
                        cache.Enqueue(new Atom(line, column, buffer[++i]));
                    }

                    line++;
                    column = 0;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ((IDisposable)source).Dispose();
        }
    }
}
