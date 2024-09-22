using Microsoft.Extensions.Logging;
using Moq;
using VoyadoSearchApp.Logic.Interfaces;
using VoyadoSearchApp.Logic.Services;
using VoyadoSearchApp_Integrations.Interfaces;
using Xunit;

namespace VoyadoSearchApp.Tests.UnitTests.Services
{
    public  class SearchServiceTests
    {
        [Fact]
        public async Task AggregateSearchResults_SumsResultsFromGoogleAndBing_ForMultipleSearchTerms()
        {
            // Arrange
            var mockSearchServiceFactory = new Mock<ISearchServiceFactory>();
            
            var mockGoogleService = new Mock<ISearchService>();
            mockGoogleService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>())).ReturnsAsync(50);
            
            var mockBingService = new Mock<ISearchService>();
            mockBingService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>())).ReturnsAsync(15);
            
            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("google")).Returns(mockGoogleService.Object);
            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("bing")).Returns(mockBingService.Object);
            mockSearchServiceFactory.Setup(factory => factory.GetAllSearchEngineNames()).Returns(["google", "bing"]);

            var searchAggregatorService = new SearchAggregatorService(mockSearchServiceFactory.Object, Mock.Of<ILogger<SearchAggregatorService>>());

            // Act           
            var result = await searchAggregatorService.AggregateSearchResults("hello world");

            // Assert
            
            Assert.True(result.Success);
            Assert.Equal(100, result.NumberOfGoogleHits); 
            Assert.Equal(30, result.NumberOfBingHits);   
            Assert.Equal(130, result.TotalSumOfHits);     
        }


        [Fact]
        public async Task AggregateSearchResults_CalculatesResponseTime()
        {
            // Arrange
            var mockSearchServiceFactory = new Mock<ISearchServiceFactory>();

            var mockGoogleService = new Mock<ISearchService>();
            var mockBingService = new Mock<ISearchService>();

            // Needed to add a delay in order to get this test to work, otherwise the responsetime was too close to 0 milliseconds.

            mockGoogleService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    Task.Delay(50).Wait(); 
                    return 100;
                });

            
            mockBingService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>()))
                .ReturnsAsync(() =>
                {
                    Task.Delay(50).Wait(); 
                    return 50;
                });

            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("google")).Returns(mockGoogleService.Object);
            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("bing")).Returns(mockBingService.Object);
            mockSearchServiceFactory.Setup(factory => factory.GetAllSearchEngineNames()).Returns(["google", "bing"]);

            var searchAggregatorService = new SearchAggregatorService(mockSearchServiceFactory.Object, Mock.Of<ILogger<SearchAggregatorService>>());

            // Act
            var result = await searchAggregatorService.AggregateSearchResults("test query");

            // Assert
            Assert.True(result.Success);
            Assert.True(result.ResponseTime > 0);
        }


        [Fact]
        public async Task AggregateSearchResults_HandlesErrorWhenGoogleServiceFails()
        {
            // Arrange
            var mockSearchServiceFactory = new Mock<ISearchServiceFactory>();
            
            var mockGoogleService = new Mock<ISearchService>();
            mockGoogleService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>())).ThrowsAsync(new Exception("Google API error"));

            var mockBingService = new Mock<ISearchService>();
            mockBingService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>())).ReturnsAsync(50);

            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("google")).Returns(mockGoogleService.Object);
            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("bing")).Returns(mockBingService.Object);
            mockSearchServiceFactory.Setup(factory => factory.GetAllSearchEngineNames()).Returns(["google", "bing"]);

            var searchAggregatorService = new SearchAggregatorService(mockSearchServiceFactory.Object, Mock.Of<ILogger<SearchAggregatorService>>());

            // Act
            var result = await searchAggregatorService.AggregateSearchResults("test query");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("An error occurred during the search", result.ErrorResponseString);
        }


        [Fact]
        public async Task AggregateSearchResults_HandlesErrorWhenBingServiceFails()
        {
            // Arrange
            var mockSearchServiceFactory = new Mock<ISearchServiceFactory>();
            
            var mockGoogleService = new Mock<ISearchService>();
            mockGoogleService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>())).ReturnsAsync(100);
            
            var mockBingService = new Mock<ISearchService>();
            mockBingService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>())).ThrowsAsync(new Exception("Bing API error"));

            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("google")).Returns(mockGoogleService.Object);
            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("bing")).Returns(mockBingService.Object);
            mockSearchServiceFactory.Setup(factory => factory.GetAllSearchEngineNames()).Returns(["google", "bing"]);

            var searchAggregatorService = new SearchAggregatorService(mockSearchServiceFactory.Object, Mock.Of<ILogger<SearchAggregatorService>>());

            // Act
            var result = await searchAggregatorService.AggregateSearchResults("test query");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("An error occurred during the search", result.ErrorResponseString);
        }


        [Fact]
        public async Task AggregateSearchResults_ThrowsExceptionForInvalidSearchEngine()
        {
            // Arrange
            var mockSearchServiceFactory = new Mock<ISearchServiceFactory>();
            
            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService(It.Is<string>(s => s == "invalid"))).Throws(new ArgumentException("Invalid search engine"));
            mockSearchServiceFactory.Setup(factory => factory.GetAllSearchEngineNames()).Returns(["invalid"]);

            var searchAggregatorService = new SearchAggregatorService(mockSearchServiceFactory.Object, Mock.Of<ILogger<SearchAggregatorService>>());

            // Act
            var result = await searchAggregatorService.AggregateSearchResults("test query");

            // Assert
            Assert.False(result.Success);
            Assert.Contains("An error occurred during the search", result.ErrorResponseString);
        }

        [Fact]
        public async Task AggregateSearchResults_LogsInformation_OnSuccessfulSearch()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SearchAggregatorService>>();
            var mockSearchServiceFactory = new Mock<ISearchServiceFactory>();

            var mockGoogleService = new Mock<ISearchService>();
            var mockBingService = new Mock<ISearchService>();

            mockGoogleService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>())).ReturnsAsync(100);
            mockBingService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>())).ReturnsAsync(50);

            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("google")).Returns(mockGoogleService.Object);
            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("bing")).Returns(mockBingService.Object);
            mockSearchServiceFactory.Setup(factory => factory.GetAllSearchEngineNames()).Returns(["google", "bing"]);

            var searchAggregatorService = new SearchAggregatorService(mockSearchServiceFactory.Object, mockLogger.Object);

            // Act
            await searchAggregatorService.AggregateSearchResults("test query");

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Total aggregation time for query")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }


        [Fact]
        public async Task AggregateSearchResults_LogsError_OnFailedSearch()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<SearchAggregatorService>>();
            var mockSearchServiceFactory = new Mock<ISearchServiceFactory>();

            var mockGoogleService = new Mock<ISearchService>();
            mockGoogleService.Setup(service => service.GetTotalSearchHits(It.IsAny<string>()))
                             .ThrowsAsync(new Exception("Google API error"));

            mockSearchServiceFactory.Setup(factory => factory.CreateSearchService("google"))
                                    .Returns(mockGoogleService.Object);
            mockSearchServiceFactory.Setup(factory => factory.GetAllSearchEngineNames())
                                    .Returns(["google"]);

            var searchAggregatorService = new SearchAggregatorService(mockSearchServiceFactory.Object, mockLogger.Object);

            // Act
            await searchAggregatorService.AggregateSearchResults("test query");

            // Assert
            mockLogger.Verify(
                x => x.Log<It.IsAnyType>(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while aggregating search results")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);  
        }
    }
}
