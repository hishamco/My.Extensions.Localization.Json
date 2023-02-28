using BenchmarkDotNet.Running;

namespace My.Extensions.Localization.Json.Benchmarks;

public static class Program
{
    public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
