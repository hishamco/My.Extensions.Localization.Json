using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LocalizationSample.FunctionalTest
{
    public class LocalizationSampleSpec: IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public LocalizationSampleSpec(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }
        
        [Fact]
        public async Task Response_should_return_hello()
        {
            var response = await _client.GetAsync("/").ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            responseString.Should().Contain("Hello");
        }
        
        [Fact]
        public async Task Response_should_return_bonjour()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "");
            requestMessage.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue("fr-FR"));
            
            var response = await _client.SendAsync(requestMessage).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            responseString.Should().Contain("Bonjour");
        }
    }
}
