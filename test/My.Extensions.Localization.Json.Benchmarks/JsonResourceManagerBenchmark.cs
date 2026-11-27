using BenchmarkDotNet.Attributes;
using My.Extensions.Localization.Json.Internal;
using System.Globalization;
using System.Text;

namespace My.Extensions.Localization.Json.Benchmarks;

[MemoryDiagnoser]
public class JsonResourceManagerBenchmark
{
    private static readonly string s_resourcesDirectoryName = "Resources";
    private static readonly string[] s_resourcesPaths = [ s_resourcesDirectoryName ];
    private static readonly CultureInfo s_frenchCulture = CultureInfo.GetCultureInfo("fr-FR");

    private static string s_resourcesFileName;
    private static string s_resourcesFilePath;

    private static JsonResourceManager s_jsonResourceManager;

    [Params(10, 100, 1_000, 10_000, 100_000, 1_000_000)]
    public int Keys;

    [GlobalSetup]
    public void Setup()
    {
        var builder = new StringBuilder("{");
        for (int i = 1; i <= Keys; i++)
        {
            builder.AppendLine($"""  "Hello, {i}": "Bonjour, {i}""");
        }

        builder.AppendLine("}");

        s_resourcesFileName = $"Resource_{Keys}.fr-FR.json";
        s_resourcesFilePath = Path.Combine(s_resourcesDirectoryName, s_resourcesFileName);

        File.WriteAllText(s_resourcesFilePath, builder.ToString());

        s_jsonResourceManager = new JsonResourceManager(s_resourcesPaths, s_resourcesFileName);
    }

    [Benchmark]
    public void EvaluateGetResourceSetWithoutCultureFallback()
        => s_jsonResourceManager.GetResourceSet(s_frenchCulture, tryParents: false);

    [Benchmark]
    public void EvaluateGetResourceSetWithCultureFallback()
        => s_jsonResourceManager.GetResourceSet(s_frenchCulture, tryParents: true);

    [GlobalCleanup]
    public void Cleanup()
    {
        if (File.Exists(s_resourcesFilePath))
        {
            File.Delete(s_resourcesFilePath);
        }
    }
}
