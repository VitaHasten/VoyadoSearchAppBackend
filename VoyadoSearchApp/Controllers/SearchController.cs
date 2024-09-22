using Microsoft.AspNetCore.Mvc;
using System.Numerics;
using VoyadoSearchApp.Logic.Interfaces;
using VoyadoSearchApp_Integrations.Dto;
using VoyadoSearchApp_Integrations.Interfaces;

namespace VoyadoSearchApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController(ISearchAggregatorService searchAggregatorService) : ControllerBase
    {
        private readonly ISearchAggregatorService _searchAggregatorService = searchAggregatorService;        

        [HttpGet()]
        public async Task<ActionResult<SearchResponseDto>> PerformSearch(string searchString)
        {
            // This value needs to match the same value in frontend
            var MAX_LETTERS_INPUTFIELD = 75;

            if (string.IsNullOrEmpty(searchString) || searchString.Length > MAX_LETTERS_INPUTFIELD)
            {
                return BadRequest(new ValidationProblemDetails
                {
                    Title = "Invalid input",
                    Detail = "The search string must not be empty and cannot exceed the maximum allowed length.",
                    Status = StatusCodes.Status400BadRequest,
                    Extensions =
                    {
                        ["MaxLength"] = MAX_LETTERS_INPUTFIELD
                    }
                });
            }

            var searchResponse = await _searchAggregatorService.AggregateSearchResults(searchString); 
            
            if (searchResponse == null || !searchResponse.Success ) 
            {
                return BadRequest(searchResponse);
            }

            return Ok(searchResponse);
        }
    }
}