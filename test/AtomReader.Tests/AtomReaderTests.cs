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
}
