#pragma warning disable MSTEST0037
using System;
using System.IO;
using System.Linq;
using AtomReaderNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AtomReader.Tests;

[TestClass]
public class AtomReaderTests
{
    [TestMethod]
    public void EndOfStream_MultipleCalls_ReturnsSameResult()
    {
        var input = "a";
        using var reader = new AtomReaderNet.AtomReader(input);

        Assert.IsFalse(reader.EndOfStream);
        Assert.IsFalse(reader.EndOfStream);

        var a = reader.Read();
        Assert.AreEqual('a', a.Value);

        Assert.IsTrue(reader.EndOfStream);
        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void ReadCount_TracksNumberOfReadCharacters()
    {
        var input = "a\r\nbc";
        using var reader = new AtomReaderNet.AtomReader(input);

        Assert.AreEqual(0, reader.ReadCount);

        reader.Peek();
        Assert.AreEqual(0, reader.ReadCount);

        reader.Precache();
        Assert.AreEqual(0, reader.ReadCount);

        _ = reader.Read(); // 'a'
        Assert.AreEqual(1, reader.ReadCount);

        _ = reader.ReadLine().ToArray(); // consumes '\r\n'
        Assert.AreEqual(3, reader.ReadCount);

        _ = reader.ReadToEnd().ToArray(); // consumes 'b', 'c'
        Assert.AreEqual(5, reader.ReadCount);
    }

    [TestMethod]
    public void Read_SingleCharacters_TracksLineAndColumn()
    {
        var input = "abc";
        using var reader = new AtomReaderNet.AtomReader(input);

        Assert.IsFalse(reader.EndOfStream);
        Assert.AreEqual(0, reader.ReadCount);

        var a1 = reader.Read();
        Assert.AreEqual('a', a1.Value);
        Assert.AreEqual(0, a1.Line);
        Assert.AreEqual(0, a1.Column);
        Assert.AreEqual(1, reader.ReadCount);

        var a2 = reader.Read();
        Assert.AreEqual('b', a2.Value);
        Assert.AreEqual(0, a2.Line);
        Assert.AreEqual(1, a2.Column);
        Assert.AreEqual(2, reader.ReadCount);

        var a3 = reader.Read();
        Assert.AreEqual('c', a3.Value);
        Assert.AreEqual(0, a3.Line);
        Assert.AreEqual(2, a3.Column);
        Assert.AreEqual(3, reader.ReadCount);

        Assert.IsTrue(reader.EndOfStream);
        Assert.ThrowsExactly<EndOfStreamException>(() => reader.Read());
    }

    [TestMethod]
    public void Read_WithLF_TracksLineAndColumn()
    {
        var input = "a\nb";
        using var reader = new AtomReaderNet.AtomReader(input);

        var a1 = reader.Read(); // 'a'
        Assert.AreEqual(0, a1.Line);
        Assert.AreEqual(0, a1.Column);

        var a2 = reader.Read(); // '\n'
        Assert.AreEqual('\n', a2.Value);
        Assert.AreEqual(0, a2.Line);
        Assert.AreEqual(1, a2.Column);

        var a3 = reader.Read(); // 'b'
        Assert.AreEqual('b', a3.Value);
        Assert.AreEqual(1, a3.Line);
        Assert.AreEqual(0, a3.Column);
    }

    [TestMethod]
    public void Read_WithCR_TracksLineAndColumn()
    {
        var input = "a\rb";
        using var reader = new AtomReaderNet.AtomReader(input);

        reader.Read(); // 'a'
        var a2 = reader.Read(); // '\r'
        Assert.AreEqual('\r', a2.Value);
        Assert.AreEqual(0, a2.Line);
        Assert.AreEqual(1, a2.Column);

        var a3 = reader.Read(); // 'b'
        Assert.AreEqual('b', a3.Value);
        Assert.AreEqual(1, a3.Line);
        Assert.AreEqual(0, a3.Column);
    }

    [TestMethod]
    public void Read_WithCRLF_TracksLineAndColumn()
    {
        var input = "a\r\nb";
        using var reader = new AtomReaderNet.AtomReader(input);

        reader.Read(); // 'a'

        var a2 = reader.Read(); // '\r'
        Assert.AreEqual('\r', a2.Value);
        Assert.AreEqual(0, a2.Line);
        Assert.AreEqual(1, a2.Column);

        var a3 = reader.Read(); // '\n'
        Assert.AreEqual('\n', a3.Value);
        Assert.AreEqual(0, a3.Line);
        Assert.AreEqual(2, a3.Column);

        var a4 = reader.Read(); // 'b'
        Assert.AreEqual('b', a4.Value);
        Assert.AreEqual(1, a4.Line);
        Assert.AreEqual(0, a4.Column);
    }

    [TestMethod]
    public void Peek_ReturnsNextWithoutConsuming()
    {
        var input = "abc";
        using var reader = new AtomReaderNet.AtomReader(input);

        var a1 = reader.Peek();
        Assert.AreEqual('a', a1.Value);
        Assert.AreEqual(0, reader.ReadCount);

        var a2 = reader.Read();
        Assert.AreEqual('a', a2.Value);
        Assert.AreEqual(1, reader.ReadCount);
    }

    [TestMethod]
    public void Peek_ThrowsEndOfStreamException_WhenEmpty()
    {
        var input = "";
        using var reader = new AtomReaderNet.AtomReader(input);

        Assert.ThrowsExactly<EndOfStreamException>(() => reader.Peek());
    }

    [TestMethod]
    public void Peek_EmptyStream_ThrowsEndOfStreamException()
    {
        using var stream = new MemoryStream();
        using var reader = new AtomReaderNet.AtomReader(stream);

        Assert.ThrowsExactly<EndOfStreamException>(() => reader.Peek());
    }

    [TestMethod]
    public void ReadToEnd_ReadsRemainingAtoms()
    {
        var input = "hello";
        using var reader = new AtomReaderNet.AtomReader(input);

        var a1 = reader.Read(); // consume 'h'

        var rest = reader.ReadToEnd().ToArray();
        Assert.AreEqual(4, rest.Length);
        Assert.AreEqual("ello", new string(rest.Select(a => a.Value).ToArray()));
        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void ReadLine_ReadsUntilLF()
    {
        var input = "hello\nworld";
        using var reader = new AtomReaderNet.AtomReader(input);

        var line1 = reader.ReadLine().ToArray();
        Assert.AreEqual("hello\n", new string(line1.Select(a => a.Value).ToArray()));

        var line2 = reader.ReadLine().ToArray();
        Assert.AreEqual("world", new string(line2.Select(a => a.Value).ToArray()));
    }

    [TestMethod]
    public void ReadLine_ReadsUntilCR()
    {
        var input = "hello\rworld";
        using var reader = new AtomReaderNet.AtomReader(input);

        var line1 = reader.ReadLine().ToArray();
        Assert.AreEqual("hello\r", new string(line1.Select(a => a.Value).ToArray()));

        var line2 = reader.ReadLine().ToArray();
        Assert.AreEqual("world", new string(line2.Select(a => a.Value).ToArray()));
    }

    [TestMethod]
    public void ReadLine_ReadsUntilCRLF()
    {
        var input = "hello\r\nworld";
        using var reader = new AtomReaderNet.AtomReader(input);

        var line1 = reader.ReadLine().ToArray();
        Assert.AreEqual("hello\r\n", new string(line1.Select(a => a.Value).ToArray()));

        var line2 = reader.ReadLine().ToArray();
        Assert.AreEqual("world", new string(line2.Select(a => a.Value).ToArray()));
    }

    [TestMethod]
    public void ReadLine_MultipleCalls()
    {
        var input = "a\nb\r\nc\r";
        using var reader = new AtomReaderNet.AtomReader(input);

        var line1 = reader.ReadLine().ToArray();
        Assert.AreEqual("a\n", new string(line1.Select(a => a.Value).ToArray()));

        var line2 = reader.ReadLine().ToArray();
        Assert.AreEqual("b\r\n", new string(line2.Select(a => a.Value).ToArray()));

        var line3 = reader.ReadLine().ToArray();
        Assert.AreEqual("c\r", new string(line3.Select(a => a.Value).ToArray()));
    }

    [TestMethod]
    public void ReadLine_EmptyStream_ReturnsEmpty()
    {
        var input = "";
        using var reader = new AtomReaderNet.AtomReader(input);

        var line = reader.ReadLine().ToArray();
        Assert.AreEqual(0, line.Length);
    }

    [TestMethod]
    public void ReadLine_ExhaustedStream_ReturnsEmpty()
    {
        var input = "hello";
        using var reader = new AtomReaderNet.AtomReader(input);

        var line1 = reader.ReadLine().ToArray();
        Assert.AreEqual("hello", new string(line1.Select(a => a.Value).ToArray()));

        var line2 = reader.ReadLine().ToArray();
        Assert.AreEqual(0, line2.Length);
    }

    [TestMethod]
    public void BufferBoundary_Test()
    {
        var input = "abcdefghijklmnopqrstuvwxyz";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 5
        };

        var all = reader.ReadToEnd().ToArray();
        Assert.AreEqual(26, all.Length);
        Assert.AreEqual(input, new string(all.Select(a => a.Value).ToArray()));
    }

    [TestMethod]
    public void BufferBoundary_CRLF_SpanningBuffer()
    {
        var input = "1234\r\n5678";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 5 // \r is at index 4 (last char in first block), \n is at index 0 in second block
        };

        var line1 = reader.ReadLine().ToArray();
        Assert.AreEqual("1234\r\n", new string(line1.Select(a => a.Value).ToArray()));

        var line2 = reader.ReadLine().ToArray();
        Assert.AreEqual("5678", new string(line2.Select(a => a.Value).ToArray()));
    }

    [TestMethod]
    public void Precache_Test()
    {
        var input = "abcd";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 2
        };

        var self = reader.Precache();
        Assert.AreSame(reader, self);

        Assert.AreEqual('a', reader.Read().Value);
        Assert.AreEqual('b', reader.Read().Value);
        Assert.AreEqual('c', reader.Read().Value);
        Assert.AreEqual('d', reader.Read().Value);
        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void Precache_Empty_ThrowsEndOfStreamException()
    {
        using var reader = new AtomReaderNet.AtomReader("");
        Assert.ThrowsExactly<EndOfStreamException>(() => reader.Precache());
    }

    [TestMethod]
    public void Precache_Exhausted_ThrowsEndOfStreamException()
    {
        using var reader = new AtomReaderNet.AtomReader("a");
        reader.Read();
        Assert.ThrowsExactly<EndOfStreamException>(() => reader.Precache());
    }

    [TestMethod]
    public void Precache_InputMatchingBufferSize_PrecachesSuccessfully()
    {
        var input = "12345";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 5
        };

        var self = reader.Precache();
        Assert.AreSame(reader, self);

        // Precache called again when cache is already populated should succeed without error
        reader.Precache();

        Assert.IsFalse(reader.EndOfStream);

        var result = new string(reader.ReadToEnd().Select(a => a.Value).ToArray());
        Assert.AreEqual(input, result);
        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void Precache_ExactMultipleOfBufferSize_PrecachesSuccessfully()
    {
        var input = "1234567890";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 5
        };

        // First block
        reader.Precache();
        for (int i = 0; i < 5; i++)
        {
            Assert.AreEqual(input[i], reader.Read().Value);
        }

        // Second block matching boundary
        reader.Precache();
        for (int i = 5; i < 10; i++)
        {
            Assert.AreEqual(input[i], reader.Read().Value);
        }

        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void Constructors_Test()
    {
        // stream
        using var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("stream"));
        using var readerStream = new AtomReaderNet.AtomReader(ms);
        Assert.AreEqual("stream", new string(readerStream.ReadToEnd().Select(a => a.Value).ToArray()));

        // textreader
        using var tr = new StringReader("textreader");
        using var readerTr = new AtomReaderNet.AtomReader(tr);
        Assert.AreEqual("textreader", new string(readerTr.ReadToEnd().Select(a => a.Value).ToArray()));
    }

    [TestMethod]
    public void Constructor_NullStream_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604, CS8625
        Stream nullStream = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => new AtomReaderNet.AtomReader(nullStream));
#pragma warning restore CS8600, CS8604, CS8625
    }

    [TestMethod]
    public void Constructors_NullSource_ThrowsArgumentNullException()
    {
#pragma warning disable CS8600, CS8604
        string nullString = null;
        Stream nullStream = null;
        TextReader nullTextReader = null;

        Assert.ThrowsExactly<ArgumentNullException>(() => new AtomReaderNet.AtomReader(nullString));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AtomReaderNet.AtomReader(nullStream));
        Assert.ThrowsExactly<ArgumentNullException>(() => new AtomReaderNet.AtomReader(nullTextReader));
#pragma warning restore CS8600, CS8604
    }

    [TestMethod]
    public void Dispose_DisposesUnderlyingSource()
    {
        var textReader = new DisposableTextReader("test");
        var reader = new AtomReaderNet.AtomReader(textReader);

        Assert.IsFalse(textReader.IsDisposed);
        reader.Dispose();
        Assert.IsTrue(textReader.IsDisposed);
    }

    [TestMethod]
    public void Read_DisposedReader_ThrowsObjectDisposedException()
    {
        var reader = new AtomReaderNet.AtomReader("test");
        reader.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => reader.Read());
    }

    [TestMethod]
    public void Read_LargeFile_ReadsAllAtomsAndTracksPositions()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var lineLength = 50;
            var numLines = 200; // 200 * 51 chars = 10,200 chars (> 4096 default buffer)
            var sb = new System.Text.StringBuilder();
            for (var l = 0; l < numLines; l++)
            {
                var lineText = $"Line {l:D4}: " + new string((char)('a' + (l % 26)), lineLength - 11);
                sb.Append(lineText).Append('\n');
            }
            var expectedText = sb.ToString();
            File.WriteAllText(tempFile, expectedText);

            using var stream = File.OpenRead(tempFile);
            using var reader = new AtomReaderNet.AtomReader(stream);

            var atoms = reader.ReadToEnd().ToArray();

            Assert.AreEqual(expectedText.Length, atoms.Length);
            Assert.AreEqual(expectedText.Length, reader.ReadCount);
            Assert.IsTrue(reader.EndOfStream);

            var readText = new string(atoms.Select(a => a.Value).ToArray());
            Assert.AreEqual(expectedText, readText);

            var currentLine = 0;
            var currentColumn = 0;
            for (var i = 0; i < atoms.Length; i++)
            {
                Assert.AreEqual(currentLine, atoms[i].Line, $"Mismatch at index {i}");
                Assert.AreEqual(currentColumn, atoms[i].Column, $"Mismatch at index {i}");

                if (atoms[i].Value == '\n')
                {
                    currentLine++;
                    currentColumn = 0;
                }
                else
                {
                    currentColumn++;
                }
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public void Read_LargeFile_WithReadLine_ReadsAllLinesCorrectly()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var numLines = 300;
            var lines = new string[numLines];
            for (var l = 0; l < numLines; l++)
            {
                lines[l] = $"Line {l:D4}: The quick brown fox jumps over the lazy dog\r\n";
            }
            var expectedText = string.Concat(lines);
            File.WriteAllText(tempFile, expectedText);

            using var stream = File.OpenRead(tempFile);
            using var reader = new AtomReaderNet.AtomReader(stream);

            var lineCount = 0;
            while (!reader.EndOfStream)
            {
                var lineAtoms = reader.ReadLine().ToArray();
                if (lineAtoms.Length == 0)
                {
                    break;
                }
                var lineStr = new string(lineAtoms.Select(a => a.Value).ToArray());
                Assert.AreEqual(lines[lineCount], lineStr, $"Mismatch at line {lineCount}");
                lineCount++;
            }

            Assert.AreEqual(numLines, lineCount);
            Assert.IsTrue(reader.EndOfStream);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [TestMethod]
    public void EndOfStream_ExactBufferMultiple_ReturnsCorrectStateAtBoundaries()
    {
        var input = "1234567890"; // 10 chars
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 5
        };

        Assert.IsFalse(reader.EndOfStream);

        // Read first 4 characters (cache size becomes 1)
        for (int i = 0; i < 4; i++)
        {
            reader.Read();
            Assert.IsFalse(reader.EndOfStream);
        }

        // Read 5th character ('5'): cache is now empty, source has "67890" remaining
        var atom5 = reader.Read();
        Assert.AreEqual('5', atom5.Value);
        Assert.IsFalse(reader.EndOfStream);

        // Read 6th character ('6'): cache refilled with 5 chars, 1 consumed, source reached EOF (-1)
        var atom6 = reader.Read();
        Assert.AreEqual('6', atom6.Value);
        Assert.IsFalse(reader.EndOfStream);

        // Read chars 7, 8, 9
        for (int i = 0; i < 3; i++)
        {
            reader.Read();
            Assert.IsFalse(reader.EndOfStream);
        }

        // Read 10th character ('0'): cache becomes empty, source is at EOF
        var atom10 = reader.Read();
        Assert.AreEqual('0', atom10.Value);
        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void EndOfStream_BufferBoundaryWithCRLF_AtExactBoundary()
    {
        // BufferSize = 5. First block: "1234\r". Second block: "\n5678".
        var input = "1234\r\n5678";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 5
        };

        Assert.IsFalse(reader.EndOfStream);

        // Read "1234"
        for (int i = 0; i < 4; i++)
        {
            reader.Read();
            Assert.IsFalse(reader.EndOfStream);
        }

        // Read '\r' (5th char) - boundary reached, cache empty
        var cr = reader.Read();
        Assert.AreEqual('\r', cr.Value);
        Assert.IsFalse(reader.EndOfStream);

        // Read '\n' (6th char) - cache replenished with "\n5678"
        var lf = reader.Read();
        Assert.AreEqual('\n', lf.Value);
        Assert.IsFalse(reader.EndOfStream);

        // Read "567"
        for (int i = 0; i < 3; i++)
        {
            reader.Read();
            Assert.IsFalse(reader.EndOfStream);
        }

        // Read '8' - end of stream reached
        var last = reader.Read();
        Assert.AreEqual('8', last.Value);
        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void EndOfStream_WhenSourceIsStream_ExactBufferBoundary()
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes("12345678"); // 8 bytes
        using var stream = new MemoryStream(bytes);
        using var reader = new AtomReaderNet.AtomReader(stream)
        {
            BufferSize = 4
        };

        Assert.IsFalse(reader.EndOfStream);

        // Read 4 bytes from first buffer
        for (int i = 0; i < 4; i++)
        {
            reader.Read();
            if (i < 3)
            {
                Assert.IsFalse(reader.EndOfStream);
            }
        }
        // At byte 4, cache is empty, but source stream still has 4 bytes
        Assert.IsFalse(reader.EndOfStream);

        // Read remaining 4 bytes
        for (int i = 0; i < 4; i++)
        {
            reader.Read();
        }

        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void EndOfStream_PeekAtBufferBoundary()
    {
        var input = "abcdef";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 3
        };

        // Read 3 chars ('a', 'b', 'c') to drain first buffer block
        Assert.AreEqual('a', reader.Read().Value);
        Assert.AreEqual('b', reader.Read().Value);
        Assert.AreEqual('c', reader.Read().Value);

        // Cache is now empty, source position is at 'd'
        Assert.IsFalse(reader.EndOfStream);

        // Peek should load next buffer block ("def") and return 'd'
        var peeked = reader.Peek();
        Assert.AreEqual('d', peeked.Value);
        Assert.IsFalse(reader.EndOfStream);

        // Read remaining
        Assert.AreEqual('d', reader.Read().Value);
        Assert.AreEqual('e', reader.Read().Value);
        Assert.AreEqual('f', reader.Read().Value);

        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void EndOfStream_PrecacheAtBufferBoundary()
    {
        var input = "123456";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 3
        };

        // Consume first buffer block
        for (int i = 0; i < 3; i++)
        {
            reader.Read();
        }

        Assert.IsFalse(reader.EndOfStream);

        // Precache next block
        reader.Precache();
        Assert.IsFalse(reader.EndOfStream);

        for (int i = 0; i < 3; i++)
        {
            reader.Read();
        }

        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void EndOfStream_ReadLineAtBufferBoundary()
    {
        var input = "abc\ndef\n"; // line 1: 4 chars ("abc\n"), line 2: 4 chars ("def\n")
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 4
        };

        Assert.IsFalse(reader.EndOfStream);

        var line1 = reader.ReadLine().ToArray();
        Assert.AreEqual("abc\n", new string(line1.Select(a => a.Value).ToArray()));
        Assert.IsFalse(reader.EndOfStream);

        var line2 = reader.ReadLine().ToArray();
        Assert.AreEqual("def\n", new string(line2.Select(a => a.Value).ToArray()));
        Assert.IsTrue(reader.EndOfStream);
    }

    [TestMethod]
    public void EndOfStream_SingleBuffer_ExactFill()
    {
        var input = "abc";
        using var reader = new AtomReaderNet.AtomReader(input)
        {
            BufferSize = 3
        };

        Assert.IsFalse(reader.EndOfStream);

        Assert.AreEqual('a', reader.Read().Value);
        Assert.IsFalse(reader.EndOfStream);

        Assert.AreEqual('b', reader.Read().Value);
        Assert.IsFalse(reader.EndOfStream);

        Assert.AreEqual('c', reader.Read().Value);
        Assert.IsTrue(reader.EndOfStream);
    }
}

public class DisposableTextReader : StringReader
{
    public bool IsDisposed { get; private set; }

    public DisposableTextReader(string s) : base(s)
    {
    }

    protected override void Dispose(bool disposing)
    {
        IsDisposed = true;
        base.Dispose(disposing);
    }
}
