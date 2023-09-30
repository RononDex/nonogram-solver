
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace nonogram_solver_tests.Benchmarking;

[SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net70, launchCount: 5, warmupCount: 5, iterationCount: 5)]
[MemoryDiagnoser]
public class SmallDuck
{

		[GlobalSetup]
		public void Setup()
		{
				File.Copy("samples/duck.in", "nonogram.in", true);
		}

		[Benchmark]
		public void SmallDuckExample()
		{
				NonoGramSolver.Main();
		}

		[GlobalCleanup]
		public void OutputInfo()
		{
				Console.WriteLine($"Found {NonoGramSolver.solutions.Count} solution");
		}
}
