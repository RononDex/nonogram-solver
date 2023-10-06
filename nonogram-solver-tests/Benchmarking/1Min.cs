using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace nonogram_solver_tests.Benchmarking;

[SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net70, launchCount: 3, warmupCount: 1, iterationCount: 3)]
[MemoryDiagnoser]
public class OneMin
{

		[GlobalSetup]
		public void Setup()
		{
				File.Copy("samples/1min.in", "nonogram.in", true);
		}

		[Benchmark]
		public void OneMinExample()
		{
				NonoGramSolver.Main();
		}
}
