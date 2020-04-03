using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LocalizationSample.Mvc.FunctionalTest
{
    public class LocalizationMvcSampleTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public LocalizationMvcSampleTests(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Theory]
        [InlineData("en-US", "Privacy Policy")]
        [InlineData("fr-FR", "Politique de confidentialité")]
        public async Task LocalizePrivacyView(string culture, string expected)
        {
            // Arrange
            var url = "/Home/Privacy";

            // Act
            var response = await _client.GetAsync($"{url}/?culture-{culture}&ui-culture={culture}");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains(expected, content);
        }
    }
}
