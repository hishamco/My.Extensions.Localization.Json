using BenchmarkDotNet.Attributes;
using My.Extensions.Localization.Json.Internal;
using System.Globalization;

namespace My.Extensions.Localization.Json.Benchmarks;

[MemoryDiagnoser]
public class JsonResourceManagerBenchmark
{
    private static readonly JsonResourceManager _jsonResourceManager;
    private static readonly CultureInfo _frenchCulture;

    static JsonResourceManagerBenchmark()
    {
        _jsonResourceManager = new JsonResourceManager("Resources\\fr-FR.json");
        _frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
    }

    [Benchmark]
    public void EvaluateGetResourceSetWithoutCultureFallback()
    {
        _jsonResourceManager.GetResourceSet(_frenchCulture, tryParents: false);
    }

    [Benchmark]
    public void EvaluateGetResourceSetWithCultureFallback()
    {
        _jsonResourceManager.GetResourceSet(_frenchCulture, tryParents: true);
    }
}
