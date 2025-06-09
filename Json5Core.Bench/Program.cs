using fastJSON5;

namespace Json5Core.Bench;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<BenchmarkDemo>();
    }
}

[MemoryDiagnoser]
public class BenchmarkDemo
{
    [Benchmark]
    public void New()
    {
        string s = "[{\"foo\":\"'[0]\\\"{}\\u1234\\r\\n\",\"bar\":12222,\"coo\":\"some' string\",\"dir\":\"C:\\\\folder\\\\\"}]";
        Json5.Beautify(s);
    }

    [Benchmark(Baseline = true)]
    public void Old()
    {
        string s = "[{\"foo\":\"'[0]\\\"{}\\u1234\\r\\n\",\"bar\":12222,\"coo\":\"some' string\",\"dir\":\"C:\\\\folder\\\\\"}]";
        JSON5.Beautify(s);
    }
}
