using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.CsProj;
using My.Extensions.Localization.Json.Internal;
using System.Globalization;

namespace My.Extensions.Localization.Json.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    public class JsonResourceManagerBenchmarks
    {
        private static readonly JsonResourceManager _jsonResourceManager;
        private static readonly CultureInfo _frenchCulture;

        static JsonResourceManagerBenchmarks()
        {
            _jsonResourceManager = new JsonResourceManager("Resources\\fr-FR.json");
            _frenchCulture = CultureInfo.GetCultureInfo("fr-FR");
        }

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
                var baseJob = Job.MediumRun.WithToolchain(CsProjCoreToolchain.NetCoreApp80);

                AddJob(baseJob.WithNuGet("My.Extensions.Localization.Json", "3.0.0"));
                AddJob(baseJob.WithNuGet("My.Extensions.Localization.Json", "3.1.0"));
                AddJob(baseJob.WithNuGet("My.Extensions.Localization.Json", "3.2.0"));
                AddJob(baseJob.WithNuGet("My.Extensions.Localization.Json", "3.3.0"));
            }
        }
    }
}
