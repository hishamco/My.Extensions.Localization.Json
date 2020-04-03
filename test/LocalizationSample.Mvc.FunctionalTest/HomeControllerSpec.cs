using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LocalizationSample.Mvc.FunctionalTest
{
    public class HomeControllerSpec : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public HomeControllerSpec(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PrivacyPolicy_should_return_Privacy_Policy()
        {
            var response = await _client.GetAsync("/Home/Privacy").ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            result.Should().Contain("Privacy Policy");
        }

        [Fact]
        public async Task PrivacyPolicy_should_return_Politique_de_confidentialité()
        {
            var response = await _client.GetAsync("/Home/Privacy/?culture-fr-FR&ui-culture=fr-FR")
                .ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            result.Should().Contain("Politique de confidentialité");
        }
    }
}
