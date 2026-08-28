using System;
using System.Collections.Generic;
using System.IO;

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
        public bool EndOfStream
        {
            get
            {
                if (cacheCount > 0)
                {
                    return false;
                }
                if (isEndOfStream)
                {
                    return true;
                }
                FillCache();
                return cacheCount == 0;
            }
        }

        /// <summary>
        /// Returns the number of characters read (this far)
        /// </summary>
        public int ReadCount { get; private set; }

        /// <summary>
        /// Sets the buffer size used for reading, defaults to 4096
        /// </summary>
        public int BufferSize
        {
            get => bufferSize;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Buffer size must be greater than zero.");
                }
                // 2 MB maximum buffer size to prevent OOM DoS
                if (value > 2 * 1024 * 1024)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Buffer size must not exceed 2MB.");
                }
                bufferSize = value;
            }
        }

        private int bufferSize = 4096;
        private Atom[] cache = Array.Empty<Atom>();
        private int cacheHead;
        private int cacheCount;
        private int line;
        private int column;

        private readonly TextReader source;
        private char[]? _buffer;
        private bool isEndOfStream;

        /// <summary>
        /// Constructs a reader from a given string
        /// </summary>
        public AtomReader(string source)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = new StringReader(source);
        }

        /// <summary>
        /// Constructs a reader from a given Stream
        /// </summary>
        public AtomReader(Stream source)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = new StreamReader(source);
        }

        /// <summary>
        /// Constructs a reader from a given TextReader
        /// </summary>
        public AtomReader(TextReader source)
        {
            ArgumentNullException.ThrowIfNull(source);
            this.source = source;
        }

        /// <summary>
        /// Peeks the next Atom from the reader
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if attempting to read past the end of data</exception>
        public Atom Peek()
        {
            EnsureCache();
            return cache[cacheHead];
        }

        /// <summary>
        /// Reads the next Atom from the reader
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if attempting to read past the end of data</exception>
        public Atom Read()
        {
            ReadCount++;
            EnsureCache();
            cacheCount--;
            return cache[cacheHead++];
        }

        /// <summary>
        /// Ensures that a chunk of characters are read and cached from the source
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if attempting to read past the end of data</exception>
        public AtomReader Precache()
        {
            EnsureCache();
            return this;
        }

        /// <summary>
        /// Read all remaining Atoms
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if attempting to read past the end of data</exception>
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
        /// <exception cref="EndOfStreamException">Thrown if attempting to read past the end of data</exception>
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
            if (cacheCount > 0)
            {
                return;
            }

            if (EndOfStream)
            {
                throw new EndOfStreamException();
            }
        }

        private void FillCache()
        {
            if (isEndOfStream)
            {
                return;
            }

            if (_buffer == null || _buffer.Length != BufferSize)
            {
                _buffer = new char[BufferSize];
            }

            if (cache.Length < BufferSize)
            {
                cache = new Atom[BufferSize];
            }

            var buffer = _buffer;

            var read = source.ReadBlock(buffer, 0, buffer.Length);
            if (read == 0)
            {
                isEndOfStream = true;
                return;
            }

            cacheHead = 0;
            cacheCount = 0;

            for (var i = 0; i < read; i++)
            {
                cache[cacheCount++] = new Atom(line, column, buffer[i]);
                column++;

                if (buffer[i] == '\r' || buffer[i] == '\n')
                {
                    if (buffer[i] == '\r' && i < read - 1 && buffer[i + 1] == '\n')
                    {
                        cache[cacheCount++] = new Atom(line, column, buffer[++i]);
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
