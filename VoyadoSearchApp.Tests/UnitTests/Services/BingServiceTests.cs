using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System.Net;
using VoyadoSearchApp_Integrations.Services;
using Microsoft.Extensions.Configuration;
using VoyadoSearchApp.Logic.Services;
using Newtonsoft.Json;
using Xunit;

namespace VoyadoSearchApp.Tests.UnitTests.Services
{
    public class BingServiceTests
    {
        [Fact]
        public async Task GetTotalSearchHits_ReturnsCorrectHits()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"webPages\": {\"totalEstimatedMatches\": 500}}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.bing.microsoft.com/")
            };

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config["BingSearch:ApiKey"]).Returns("dummy-api-key");

            var bingService = new BingService(httpClient, configuration.Object, Mock.Of<ILogger<BingService>>());

            // Act
            var result = await bingService.GetTotalSearchHits("test query");

            // Assert
            Assert.Equal(500, result);
        }

        [Fact]
        public async Task GetTotalSearchHits_HandlesInvalidJsonResponse()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ invalid json }")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.bing.microsoft.com/")
            };

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config["BingSearch:ApiKey"]).Returns("dummy-api-key");

            var bingService = new BingService(httpClient, configuration.Object, Mock.Of<ILogger<BingService>>());

            // Act & Assert
            await Assert.ThrowsAsync<JsonReaderException>(() => bingService.GetTotalSearchHits("test query"));
        }


        [Fact]
        public async Task GetTotalSearchHits_HandlesApiError()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid API request")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.bing.microsoft.com/")
            };

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config["BingSearch:ApiKey"]).Returns("dummy-api-key");

            var bingService = new BingService(httpClient, configuration.Object, Mock.Of<ILogger<BingService>>());

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => bingService.GetTotalSearchHits("test query"));
        }
    }
}
