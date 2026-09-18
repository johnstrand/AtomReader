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
    [DataRow(' ', true)]
    [DataRow('\t', true)]
    [DataRow('\n', true)]
    [DataRow('\r', true)]
    [DataRow('\v', true)]
    [DataRow('\f', true)]
    [DataRow('\u00A0', true)] // Non-breaking space
    [DataRow('\u1680', true)] // Ogham space mark
    [DataRow('\u2000', true)] // En quad
    [DataRow('\u2001', true)] // Em quad
    [DataRow('\u2002', true)] // En space
    [DataRow('\u2003', true)] // Em space
    [DataRow('\u2004', true)] // Three-per-em space
    [DataRow('\u2005', true)] // Four-per-em space
    [DataRow('\u2006', true)] // Six-per-em space
    [DataRow('\u2007', true)] // Figure space
    [DataRow('\u2008', true)] // Punctuation space
    [DataRow('\u2009', true)] // Thin space
    [DataRow('\u200A', true)] // Hair space
    [DataRow('\u2028', true)] // Line separator
    [DataRow('\u2029', true)] // Paragraph separator
    [DataRow('\u202F', true)] // Narrow no-break space
    [DataRow('\u205F', true)] // Medium mathematical space
    [DataRow('\u3000', true)] // Ideographic space
    [DataRow('a', false)]
    [DataRow('1', false)]
    [DataRow('ö', false)]
    [DataRow('\u200B', false)] // Zero-width space (not categorized as whitespace by Char.IsWhiteSpace)
    [DataRow('\uFEFF', false)] // Zero-width no-break space / BOM
    public void IsWhiteSpace_Test(char c, bool expected)
    {
        Assert.AreEqual(expected, new Atom(0, 0, c).IsWhiteSpace);
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
    public void ToLower_CultureInvariant_Test()
    {
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
            var atom = new Atom(1, 2, 'I').ToLower();
            Assert.AreEqual('i', atom.Value);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [TestMethod]
    public void ToUpper_CultureInvariant_Test()
    {
        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("tr-TR");
            var atom = new Atom(1, 2, 'i').ToUpper();
            Assert.AreEqual('I', atom.Value);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
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

    [TestMethod]
    public void IEquatable_Atom_Test()
    {
        IEquatable<Atom> a = new Atom(1, 2, 'x');
        Atom b = new Atom(3, 4, 'x');
        Atom c = new Atom(1, 2, 'y');

        Assert.IsTrue(a.Equals(b));
        Assert.IsFalse(a.Equals(c));
    }

    [TestMethod]
    public void IEquatable_Char_Test()
    {
        IEquatable<char> a = new Atom(1, 2, 'x');

        Assert.IsTrue(a.Equals('x'));
        Assert.IsFalse(a.Equals('y'));
    }
}
