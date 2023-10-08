using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;
using nonogram_solver_tests;

namespace nonogram_solver_benchmarking
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ManualConfig()
                /* .WithOptions(ConfigOptions.DisableOptimizationsValidator) */
                .AddValidator(JitOptimizationsValidator.DontFailOnError)
                .AddLogger(ConsoleLogger.Default)
                .AddColumnProvider(DefaultColumnProviders.Instance);
            _ = BenchmarkSwitcher.FromAssembly(typeof(PreProcessingTests).Assembly).Run(args, config);
        }
    }
}
