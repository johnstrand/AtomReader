using AtomReaderNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmark;

[MemoryDiagnoser]
public class AtomStringHashCodeBenchmark
{
    private AtomString _atomString = new AtomString(new[]
    {
        new Atom(0, 0, 'h'),
        new Atom(0, 1, 'e'),
        new Atom(0, 2, 'l'),
        new Atom(0, 3, 'l'),
        new Atom(0, 4, 'o'),
        new Atom(0, 5, ' '),
        new Atom(0, 6, 'w'),
        new Atom(0, 7, 'o'),
        new Atom(0, 8, 'r'),
        new Atom(0, 9, 'l'),
        new Atom(0, 10, 'd')
    });

    [Benchmark]
    public int GetHashCode_Benchmark()
    {
        return _atomString.GetHashCode();
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<AtomStringHashCodeBenchmark>();
    }
}
