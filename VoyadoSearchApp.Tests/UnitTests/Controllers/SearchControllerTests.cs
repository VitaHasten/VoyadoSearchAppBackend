using Microsoft.AspNetCore.Mvc;
using Moq;
using VoyadoSearchApp.Api.Controllers;
using VoyadoSearchApp.Logic.Interfaces;
using VoyadoSearchApp_Integrations.Dto;
using Xunit;

namespace VoyadoSearchApp.Tests.UnitTests.Controllers
{
    public class SearchControllerTests
    {
        [Fact]
        public async Task PerformSearch_ReturnsBadRequest_WhenSearchStringIsNullOrEmpty()
        {
            // Arrange
            var mockSearchAggregatorService = new Mock<ISearchAggregatorService>();
            var controller = new SearchController(mockSearchAggregatorService.Object);

            // Act
            var resultNullSearch = await controller.PerformSearch(null);
            var resultEmptyString = await controller.PerformSearch("");

            // Assert
            var badRequestResultNull = Assert.IsType<BadRequestObjectResult>(resultNullSearch.Result);
            var badRequestResultEmpty = Assert.IsType<BadRequestObjectResult>(resultEmptyString.Result);

            var validationDetailsNull = Assert.IsType<ValidationProblemDetails>(badRequestResultNull.Value);
            var validationDetailsEmpty = Assert.IsType<ValidationProblemDetails>(badRequestResultEmpty.Value);

            Assert.Equal("Invalid input", validationDetailsNull.Title);
            Assert.Equal("The search string must not be empty and cannot exceed the maximum allowed length.", validationDetailsNull.Detail);

            Assert.Equal("Invalid input", validationDetailsEmpty.Title);
            Assert.Equal("The search string must not be empty and cannot exceed the maximum allowed length.", validationDetailsEmpty.Detail);
        }

        [Fact]
        public async Task PerformSearch_ReturnsBadRequest_WhenSearchStringExceedsMaxLength()
        {
            // Arrange
            var mockSearchAggregatorService = new Mock<ISearchAggregatorService>();
            var controller = new SearchController(mockSearchAggregatorService.Object);
            var longSearchString = new string('a', 76);

            // Act
            var result = await controller.PerformSearch(longSearchString);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var validationDetails = Assert.IsType<ValidationProblemDetails>(badRequestResult.Value);

            Assert.Equal("Invalid input", validationDetails.Title);
            Assert.Equal("The search string must not be empty and cannot exceed the maximum allowed length.", validationDetails.Detail);
            Assert.Equal(75, validationDetails.Extensions["MaxLength"]);
        }

        [Fact]
        public async Task PerformSearch_ReturnsOkResult_WithValidSearchString()
        {
            // Arrange
            var mockSearchAggregatorService = new Mock<ISearchAggregatorService>();
            mockSearchAggregatorService.Setup(service => service.AggregateSearchResults(It.IsAny<string>()))
                .ReturnsAsync(new SearchResponseDto { Success = true });

            var controller = new SearchController(mockSearchAggregatorService.Object);

            // Act
            var result = await controller.PerformSearch("valid search");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<SearchResponseDto>(okResult.Value);
            Assert.True(returnValue.Success);
        }

        [Fact]
        public async Task PerformSearch_ReturnsBadRequest_WhenAggregateSearchResultsFails()
        {
            // Arrange
            var mockSearchAggregatorService = new Mock<ISearchAggregatorService>();
            mockSearchAggregatorService.Setup(service => service.AggregateSearchResults(It.IsAny<string>()))
                .ReturnsAsync(new SearchResponseDto { Success = false });

            var controller = new SearchController(mockSearchAggregatorService.Object);

            // Act
            var result = await controller.PerformSearch("test search");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var returnValue = Assert.IsType<SearchResponseDto>(badRequestResult.Value);
            Assert.False(returnValue.Success);
        }
    }
}
