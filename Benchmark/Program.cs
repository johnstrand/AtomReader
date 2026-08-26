using AtomReaderNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark;

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

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<AtomEqualsBenchmark>();
    }
}
