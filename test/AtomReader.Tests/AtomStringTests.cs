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
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(26)]
    [DataRow(1000)]
    public void ImplicitConversionToString_LengthHandling(int count)
    {
        var atoms = Enumerable.Range(0, count)
            .Select(i => new Atom(i / 10, i % 10, (char)('a' + (i % 26))))
            .ToArray();

        var atomStr = new AtomString(atoms);
        string convertedString = atomStr;

        Assert.AreEqual(count, convertedString.Length);
        Assert.AreEqual(atomStr.Length, convertedString.Length);

        string expectedString = new string(atoms.Select(a => a.Value).ToArray());
        Assert.AreEqual(expectedString, convertedString);
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
    public void Operators_Equality_AtomString_NullHandling()
    {
#pragma warning disable CS8600, CS8602, CS8604, CS8625
        AtomString nullStr1 = null;
        AtomString nullStr2 = null;
        var validStr = new AtomString(new[] { new Atom(0, 0, 'a') });

        Assert.IsTrue(nullStr1 == nullStr2);
        Assert.IsFalse(nullStr1 != nullStr2);

        Assert.IsFalse(validStr == nullStr1);
        Assert.IsFalse(nullStr1 == validStr);

        Assert.IsTrue(validStr != nullStr1);
        Assert.IsTrue(nullStr1 != validStr);
#pragma warning restore CS8600, CS8602, CS8604, CS8625
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
    public void Operators_Equality_String_Nulls()
    {
#pragma warning disable CS8600, CS8602, CS8604, CS8625
        AtomString nullAtomStr = null;
        AtomString atomStr = new AtomString(new[] { new Atom(0, 0, 'a'), new Atom(0, 1, 'b') });
        string nullStr = null;
        string str = "ab";

        // Both null
        Assert.IsTrue(nullAtomStr == nullStr);
        Assert.IsFalse(nullAtomStr != nullStr);

        // Left null, Right non-null
        Assert.IsFalse(nullAtomStr == str);
        Assert.IsTrue(nullAtomStr != str);

        // Left non-null, Right null
        Assert.IsFalse(atomStr == nullStr);
        Assert.IsTrue(atomStr != nullStr);
#pragma warning restore CS8600, CS8602, CS8604, CS8625
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
        var atoms1 = new[] { new Atom(0, 0, 'h'), new Atom(0, 1, 'i') };
        var atoms2 = new[] { new Atom(1, 0, 'h'), new Atom(1, 1, 'i') };
        var str1 = new AtomString(atoms1);
        var str2 = new AtomString(atoms2);
        Assert.AreEqual(str1.GetHashCode(), str2.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_ReturnsCachedValueOnRepeatedCalls()
    {
        var atoms = new[] { new Atom(0, 0, 't'), new Atom(0, 1, 'e'), new Atom(0, 2, 's'), new Atom(0, 3, 't') };
        var str = new AtomString(atoms);

        int hash1 = str.GetHashCode();
        int hash2 = str.GetHashCode();
        int hash3 = str.GetHashCode();

        Assert.AreEqual(hash1, hash2);
        Assert.AreEqual(hash1, hash3);
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

    [TestMethod]
    public void ImplicitConversionToString_NullReturnsNull()
    {
        AtomString? s = null;
        string? x = s;
        Assert.IsNull(x);
    }

    [TestMethod]
    public void ImplicitConversionToCharArray_NullReturnsNull()
    {
        AtomString? s = null;
        char[]? x = s;
        Assert.IsNull(x);
    }
}
