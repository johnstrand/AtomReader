using System;
using System.Linq;
using AtomReaderNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AtomReader.Tests;

[TestClass]
public class AtomStringTests
{
    [TestMethod]
    public void Constructor_EmptyThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new AtomString(Array.Empty<Atom>()));
    }

    [TestMethod]
    public void Constructor_SetsBoundsAndLength()
    {
        var atoms = new[]
        {
            new Atom(1, 2, 'a'),
            new Atom(1, 3, 'b'),
            new Atom(2, 0, 'c')
        };

        var str = new AtomString(atoms);

        Assert.AreEqual(3, str.Length);
        Assert.AreEqual(1, str.FromLine);
        Assert.AreEqual(2, str.FromColumn);
        Assert.AreEqual(2, str.ToLine);
        Assert.AreEqual(0, str.ToColumn);
    }

    [TestMethod]
    public void ImplicitConversionToString()
    {
        var atoms = new[] { new Atom(0, 0, 'a'), new Atom(0, 1, 'b') };
        var atomStr = new AtomString(atoms);

        string s = atomStr;
        Assert.AreEqual("ab", s);
    }

    [TestMethod]
    public void ImplicitConversionToCharArray()
    {
        var atoms = new[] { new Atom(0, 0, 'a'), new Atom(0, 1, 'b') };
        var atomStr = new AtomString(atoms);

        char[] arr = atomStr;
        CollectionAssert.AreEqual(new[] { 'a', 'b' }, arr);
    }

    [TestMethod]
    public void Operators_Equality_AtomString()
    {
        var a1 = new AtomString(new[] { new Atom(0, 0, 'a'), new Atom(0, 1, 'b') });
        var a2 = new AtomString(new[] { new Atom(1, 1, 'a'), new Atom(1, 2, 'b') }); // Different lines, same chars
        var a3 = new AtomString(new[] { new Atom(0, 0, 'x') });

        Assert.IsTrue(a1 == a2);
        Assert.IsFalse(a1 == a3);

        Assert.IsFalse(a1 != a2);
        Assert.IsTrue(a1 != a3);
    }

    [TestMethod]
    public void Operators_Equality_String()
    {
        var atomStr = new AtomString(new[] { new Atom(0, 0, 'a'), new Atom(0, 1, 'b') });

        Assert.IsTrue(atomStr == "ab");
        Assert.IsFalse(atomStr == "xy");

        Assert.IsFalse(atomStr != "ab");
        Assert.IsTrue(atomStr != "xy");
    }

    [TestMethod]
    public void ToString_Test()
    {
        var atoms = new[] { new Atom(0, 0, 'f'), new Atom(0, 1, 'o'), new Atom(0, 2, 'o') };
        var str = new AtomString(atoms);
        Assert.AreEqual("foo", str.ToString());
    }

    [TestMethod]
    public void GetHashCode_Test()
    {
        var atoms = new[] { new Atom(0, 0, 'h'), new Atom(0, 1, 'i') };
        var str = new AtomString(atoms);
        Assert.AreEqual("hi".GetHashCode(), str.GetHashCode());
    }

    [TestMethod]
    public void Equals_Test()
    {
        var str1 = new AtomString(new[] { new Atom(0, 0, 'a') });
        var str2 = new AtomString(new[] { new Atom(1, 1, 'a') });
        var str3 = new AtomString(new[] { new Atom(0, 0, 'b') });

        Assert.IsTrue(str1.Equals((object)str2));
        Assert.IsFalse(str1.Equals((object)str3));
        Assert.IsTrue(str1.Equals("a"));
        Assert.IsFalse(str1.Equals("b"));
        Assert.IsFalse(str1.Equals(null));
        Assert.IsFalse(str1.Equals(new object()));
    }
}
