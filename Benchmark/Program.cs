using System.Linq;
using AtomReaderNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark;

[MemoryDiagnoser]
public class AtomReaderBenchmark
{
    private string _largeText = null!;

    [GlobalSetup]
    public void Setup()
    {
        var lines = Enumerable.Range(0, 50000).Select(i => $"Line {i}: The quick brown fox jumps over the lazy dog.");
        _largeText = string.Join("\n", lines);
    }

    [Benchmark]
    public int ReadAllAtoms()
    {
        using var reader = new AtomReaderNet.AtomReader(_largeText);
        int count = 0;
        while (!reader.EndOfStream)
        {
            var atom = reader.Read();
            count += atom.Value;
        }
        return count;
    }
}

[MemoryDiagnoser]
public class AtomEqualsBenchmark
{
    private Atom _atom1 = new Atom(1, 1, 'a');
    private Atom _atom2 = new Atom(1, 2, 'a');
    private object _atom2Object = new Atom(1, 2, 'a');
    private char _charVal = 'a';
    private object _charValObject = 'a';

    [Benchmark(Baseline = true)]
    public bool EqualsObject_Atom()
    {
        return _atom1.Equals(_atom2Object);
    }

    [Benchmark]
    public bool EqualsTyped_Atom()
    {
        return _atom1.Equals(_atom2);
    }

    [Benchmark]
    public bool EqualsObject_Char()
    {
        return _atom1.Equals(_charValObject);
    }

    [Benchmark]
    public bool EqualsTyped_Char()
    {
        return _atom1.Equals(_charVal);
    }
}

[MemoryDiagnoser]
public class AtomStringGetHashCodeBenchmark
{
    private readonly AtomString _atomString = new AtomString(new[]
    {
        new Atom(1, 1, 'H'),
        new Atom(1, 2, 'e'),
        new Atom(1, 3, 'l'),
        new Atom(1, 4, 'l'),
        new Atom(1, 5, 'o'),
        new Atom(1, 6, ','),
        new Atom(1, 7, ' '),
        new Atom(1, 8, 'W'),
        new Atom(1, 9, 'o'),
        new Atom(1, 10, 'r'),
        new Atom(1, 11, 'l'),
        new Atom(1, 12, 'd'),
        new Atom(1, 13, '!')
    });

    [Benchmark]
    public int GetHashCode_Repeated()
    {
        return _atomString.GetHashCode();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<AtomReaderBenchmark>();
    }
}
