using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace nonogram_solver_tests.Benchmarking;

[SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net70, launchCount: 3, warmupCount: 2, iterationCount: 4)]
[MemoryDiagnoser]
public class LargeBunny
{

		[GlobalSetup]
		public void Setup()
		{
				File.Copy("samples/example1.in", "nonogram.in", true);
		}

		[Benchmark]
		public void LargeBunnyExample()
		{
				NonoGramSolver.Main();
		}

		[GlobalCleanup]
		public void OutputInfo()
		{
				Console.WriteLine($"Found {NonoGramSolver.solutions.Count} solution");
		}
}
