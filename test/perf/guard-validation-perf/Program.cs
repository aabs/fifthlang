using BenchmarkDotNet.Running;

namespace GuardValidationPerf;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<GuardValidationBenchmarks>();
    }
}
