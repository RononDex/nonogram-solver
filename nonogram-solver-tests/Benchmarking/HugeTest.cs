
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace nonogram_solver_tests.Benchmarking;

[SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net70, launchCount: 5, warmupCount: 5, iterationCount: 5)]
[MemoryDiagnoser]
public class HugeTest
{

    [GlobalSetup]
    public void Setup()
    {
        File.Copy("samples/huge.in", "nonogram.in", true);
    }

    [Benchmark]
    public void HugeExample()
    {
        NonoGramSolver.Main();
    }
}
