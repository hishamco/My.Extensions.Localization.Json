using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
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
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var cookieValue = $"c={culture}|uic={culture}";
            request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cookieValue}");

            // Act
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, content);
        }

        [Fact]
        public async Task LocalizeViewWithPathConventions()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/");
            var culture = "fr-FR";
            var cookieValue = $"c={culture}|uic={culture}";
            request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cookieValue}");

            // Act
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Bienvenu", content);
        }

        [Fact]
        public async Task StringLocalizeShouldWorkWithControllersPrefix()
        {
            // Arrange
            var url = "/Home/Privacy";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var culture = "fr-FR";
            var cookieValue = $"c={culture}|uic={culture}";
            request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cookieValue}");

            // Act
            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Utilisez cette page pour détailler la politique de confidentialité de votre site.", WebUtility.HtmlDecode(content));
        }
    }
}
