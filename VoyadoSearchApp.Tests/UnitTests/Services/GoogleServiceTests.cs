using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VoyadoSearchApp_Integrations.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;

namespace VoyadoSearchApp.Tests.UnitTests.Services
{
    public class GoogleServiceTests
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
                    Content = new StringContent("{\"searchInformation\": {\"totalResults\": \"1000\"}}")
                });
            
            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://www.googleapis.com/customsearch/v1")
            };
            
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config["GoogleSearch:ApiKey"]).Returns("dummy-api-key");
            configuration.Setup(config => config["GoogleSearch:Cx"]).Returns("dummy-cx-id");

            var googleService = new GoogleService(httpClient, configuration.Object, Mock.Of<ILogger<GoogleService>>());

            // Act
            var result = await googleService.GetTotalSearchHits("test query");

            // Assert
            Assert.Equal(1000, result);
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
                BaseAddress = new Uri("https://www.googleapis.com/customsearch/v1")
            };

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config["GoogleSearch:ApiKey"]).Returns("dummy-api-key");
            configuration.Setup(config => config["GoogleSearch:Cx"]).Returns("dummy-cx-id");

            var googleService = new GoogleService(httpClient, configuration.Object, Mock.Of<ILogger<GoogleService>>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => googleService.GetTotalSearchHits("test query"));
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
                BaseAddress = new Uri("https://www.googleapis.com/customsearch/v1")
            };

            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config["GoogleSearch:ApiKey"]).Returns("dummy-api-key");
            configuration.Setup(config => config["GoogleSearch:Cx"]).Returns("dummy-cx-id");

            var googleService = new GoogleService(httpClient, configuration.Object, Mock.Of<ILogger<GoogleService>>());

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => googleService.GetTotalSearchHits("test query"));
        }
    }
}
