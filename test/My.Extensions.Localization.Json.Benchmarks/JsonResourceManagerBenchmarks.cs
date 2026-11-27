using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using My.Extensions.Localization.Json.Internal;
using System.Globalization;

namespace My.Extensions.Localization.Json.Benchmarks;

[Config(typeof(BenchmarkConfig))]
[MemoryDiagnoser]
public class JsonResourceManagerBenchmarks
{
    private static readonly JsonResourceManager _jsonResourceManager = new("Resources");
    private static readonly CultureInfo _frenchCulture = CultureInfo.GetCultureInfo("fr-FR");

    [Benchmark]
    public void EvaluateGetResourceSetWithoutCultureFallback()
        => _jsonResourceManager.GetResourceSet(_frenchCulture, tryParents: false);

    [Benchmark]
    public void EvaluateGetResourceSetWithCultureFallback()
        => _jsonResourceManager.GetResourceSet(_frenchCulture, tryParents: true);

    private class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            var directory = FindRepositoryRoot();
            var restoreConfigFile = Path.Combine(directory.FullName, "NuGet.Config");

            var targetVersions = new[] { "3.0.0", "3.1.0", "3.2.0", "3.3.0", "3.4.0", "4.0.0", "4.1.0" };
            foreach (var version in targetVersions)
            {
                AddJob(Job.MediumRun
                    .WithId($"v{version}")
                    .WithToolchain(CsProjCoreToolchain.NetCoreApp10_0)
                    .WithMsBuildArguments(
                        "/p:TargetFrameworks=net10.0",
                        $"/p:JsonLocalizationVersion={version}",
                        $"/p:RestoreConfigFile=\"{restoreConfigFile}\""
                    )
                );
            }

            AddJob(Job.MediumRun
                .WithId("Current")
                .AsBaseline()
                .WithToolchain(CsProjCoreToolchain.NetCoreApp10_0)
                .WithMsBuildArguments("/p:TargetFrameworks=net10.0"));
        }

        private static DirectoryInfo FindRepositoryRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "NuGet.Config")))
            {
                directory = directory.Parent;
            }
            
            if (directory == null)
            {
                throw new InvalidOperationException("The repository NuGet.Config could not be found for benchmark restores.");
            }

            return directory;
        }
    }
}
