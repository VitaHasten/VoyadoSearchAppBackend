using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using VoyadoSearchApp_Integrations.Dto;
using VoyadoSearchApp_Integrations.Interfaces;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace VoyadoSearchApp.Tests.IntegrationsTests.Controllers
{
    public class SearchControllerIntegrationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory = factory;

        [Fact]
        public async Task PerformSearch_ReturnsOk_WhenSearchIsSuccessful()
        {
            // Arrange
            var googleServiceMock = new Mock<ISearchService>();
            googleServiceMock.Setup(s => s.GetTotalSearchHits(It.IsAny<string>())).ReturnsAsync(100);

            var bingServiceMock = new Mock<ISearchService>();
            bingServiceMock.Setup(s => s.GetTotalSearchHits(It.IsAny<string>())).ReturnsAsync(50);

            var factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {                    
                    services.RemoveAll<ISearchService>();
                    services.AddSingleton<ISearchService>(googleServiceMock.Object);
                    services.AddSingleton<ISearchService>(bingServiceMock.Object);
                });
            });

            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Search?searchString=test");

            // Assert
            response.EnsureSuccessStatusCode();
            var searchResponse = await response.Content.ReadAsAsync<SearchResponseDto>();

            Assert.NotNull(searchResponse);
            Assert.True(searchResponse.Success);
            Assert.Equal(150, searchResponse.TotalSumOfHits);
        }

        [Fact]
        public async Task PerformSearch_ReturnsBadRequest_WhenSearchStringIsInvalid()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/Search?searchString=");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
