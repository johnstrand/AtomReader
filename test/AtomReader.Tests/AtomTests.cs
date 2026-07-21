using System;
using AtomReaderNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AtomReader.Tests;

[TestClass]
public class AtomTests
{
    [TestMethod]
    public void Constructor_SetsProperties()
    {
        var atom = new Atom(1, 2, 'a');
        Assert.AreEqual(1, atom.Line);
        Assert.AreEqual(2, atom.Column);
        Assert.AreEqual('a', atom.Value);
    }

    [TestMethod]
    public void IsWhiteSpace_Test()
    {
        Assert.IsTrue(new Atom(0, 0, ' ').IsWhiteSpace);
        Assert.IsFalse(new Atom(0, 0, 'a').IsWhiteSpace);
    }

    [TestMethod]
    public void IsNumber_Test()
    {
        Assert.IsTrue(new Atom(0, 0, '1').IsNumber);
        Assert.IsFalse(new Atom(0, 0, 'a').IsNumber);
    }

    [TestMethod]
    public void IsAscii_Test()
    {
        Assert.IsTrue(new Atom(0, 0, 'a').IsAscii);
        Assert.IsFalse(new Atom(0, 0, 'ö').IsAscii);
    }

    [TestMethod]
    public void IsDigit_Test()
    {
        Assert.IsTrue(new Atom(0, 0, '1').IsDigit);
        Assert.IsFalse(new Atom(0, 0, 'a').IsDigit);
    }

    [TestMethod]
    public void IsLetter_Test()
    {
        Assert.IsTrue(new Atom(0, 0, 'a').IsLetter);
        Assert.IsFalse(new Atom(0, 0, '1').IsLetter);
    }

    [TestMethod]
    public void IsLower_Test()
    {
        Assert.IsTrue(new Atom(0, 0, 'a').IsLower);
        Assert.IsFalse(new Atom(0, 0, 'A').IsLower);
    }

    [TestMethod]
    public void IsUpper_Test()
    {
        Assert.IsTrue(new Atom(0, 0, 'A').IsUpper);
        Assert.IsFalse(new Atom(0, 0, 'a').IsUpper);
    }

    [TestMethod]
    public void IsBetween_Test()
    {
        Assert.IsTrue(new Atom(0, 0, 'c').IsBetween('a', 'z'));
        Assert.IsFalse(new Atom(0, 0, 'A').IsBetween('a', 'z'));
    }

    [TestMethod]
    public void ToLower_Test()
    {
        var atom = new Atom(1, 2, 'A').ToLower();
        Assert.AreEqual('a', atom.Value);
        Assert.AreEqual(1, atom.Line);
        Assert.AreEqual(2, atom.Column);
    }

    [TestMethod]
    public void ToUpper_Test()
    {
        var atom = new Atom(1, 2, 'a').ToUpper();
        Assert.AreEqual('A', atom.Value);
        Assert.AreEqual(1, atom.Line);
        Assert.AreEqual(2, atom.Column);
    }

    [TestMethod]
    public void Operators_Equality()
    {
        var a = new Atom(1, 2, 'x');
        var b = new Atom(3, 4, 'x');
        var c = new Atom(1, 2, 'y');

        Assert.IsTrue(a == b);
        Assert.IsFalse(a == c);
        Assert.IsTrue(a == 'x');
        Assert.IsFalse(a == 'y');

        Assert.IsFalse(a != b);
        Assert.IsTrue(a != c);
        Assert.IsFalse(a != 'x');
        Assert.IsTrue(a != 'y');
    }

    [TestMethod]
    public void ImplicitConversionToChar()
    {
        var atom = new Atom(1, 2, 'c');
        char c = atom;
        Assert.AreEqual('c', c);
    }

    [TestMethod]
    public void ToString_Test()
    {
        var atom = new Atom(1, 2, 'x');
        Assert.AreEqual("x (2@1)", atom.ToString());
    }

    [TestMethod]
    public void GetHashCode_Test()
    {
        var atom = new Atom(1, 2, 'a');
        Assert.AreEqual('a'.GetHashCode(), atom.GetHashCode());
    }

    [TestMethod]
    public void Equals_Test()
    {
        var a = new Atom(1, 2, 'x');
        var b = new Atom(3, 4, 'x');
        var c = new Atom(1, 2, 'y');

        Assert.IsTrue(a.Equals((object)b));
        Assert.IsTrue(a.Equals(b));
        Assert.IsFalse(a.Equals((object)c));
        Assert.IsTrue(a.Equals('x'));
        Assert.IsFalse(a.Equals('y'));
        Assert.IsFalse(a.Equals(null));
        Assert.IsFalse(a.Equals("x"));
    }
}
