using System;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace nonogram_solver_benchmarking
{
    public class Benchmarks
    {
        [Benchmark]
        public void Scenario1()
        {
            NonoGramSolver.Main();
        }
    }
}
