using BenchmarkDotNet.Attributes;
using My.Extensions.Localization.Json.Internal;

namespace My.Extensions.Localization.Json.Benchmarks;

[MemoryDiagnoser]
public class JsonResourceLoaderBenchmark
{
    [Benchmark]
    public void EvaluateLoadingSmallResources() => JsonResourceLoader.Load("Resources\\Small.fr-FR.json");

    [Benchmark]
    public void EvaluateLoadingBigResources() => JsonResourceLoader.Load("Resources\\Big.fr-FR.large.json");
}
