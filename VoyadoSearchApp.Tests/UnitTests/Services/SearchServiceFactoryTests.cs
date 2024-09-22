using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using VoyadoSearchApp.Logic.Services;
using VoyadoSearchApp_Integrations.Services;
using Xunit;

namespace VoyadoSearchApp.Tests.UnitTests.Services
{
    public class SearchServiceFactoryTests
    {
        [Fact]
        public void CreateSearchService_ReturnsGoogleService_WhenEngineIsGoogle()
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

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config["GoogleSearch:ApiKey"]).Returns("dummy-api-key");
            configuration.Setup(config => config["GoogleSearch:Cx"]).Returns("dummy-cx-id");
            configuration.Setup(config => config["GoogleSearch:BaseAddress"]).Returns("https://www.googleapis.com/customsearch/v1");

            var logger = Mock.Of<ILogger<GoogleService>>();
            
            var googleService = new GoogleService(httpClient, configuration.Object, logger);

            var serviceProvider = new Mock<IServiceProvider>();            
            serviceProvider.Setup(x => x.GetService(typeof(GoogleService))).Returns(googleService);

            var factory = new SearchServiceFactory(serviceProvider.Object);

            // Act
            var result = factory.CreateSearchService("google");

            // Assert
            Assert.IsType<GoogleService>(result);
        }

        [Fact]
        public void CreateSearchService_ReturnsBingService_WhenEngineIsBing()
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

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(config => config["BingSearch:ApiKey"]).Returns("dummy-api-key");
            configuration.Setup(config => config["BingSearch:BaseAddress"]).Returns("https://api.bing.microsoft.com/");
            var logger = Mock.Of<ILogger<BingService>>();
            
            var bingService = new BingService(httpClient, configuration.Object, logger);

            var serviceProvider = new Mock<IServiceProvider>();            
            serviceProvider.Setup(x => x.GetService(typeof(BingService))).Returns(bingService);

            var factory = new SearchServiceFactory(serviceProvider.Object);

            // Act
            var result = factory.CreateSearchService("bing");

            // Assert
            Assert.IsType<BingService>(result);
        }

        [Fact]
        public void CreateSearchService_ThrowsException_ForInvalidSearchEngine()
        {
            // Arrange
            var serviceProvider = new Mock<IServiceProvider>();
            var factory = new SearchServiceFactory(serviceProvider.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => factory.CreateSearchService("invalid"));
        }
    }
}
