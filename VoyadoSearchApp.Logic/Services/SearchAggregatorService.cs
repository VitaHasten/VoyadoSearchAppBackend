using Microsoft.Extensions.Logging;
using System.Diagnostics;
using VoyadoSearchApp.Logic.Interfaces;
using VoyadoSearchApp_Integrations.Dto;
using VoyadoSearchApp_Integrations.Services;

namespace VoyadoSearchApp.Logic.Services
{
    public class SearchAggregatorService(ISearchServiceFactory searchServiceFactory, ILogger<SearchAggregatorService> logger) : ISearchAggregatorService
    {
        private readonly ISearchServiceFactory _searchServiceFactory = searchServiceFactory;
        private static readonly char[] separator = [' '];
        private readonly ILogger<SearchAggregatorService> _logger = logger;

        public async Task<SearchResponseDto> AggregateSearchResults(string query)
        {
            var stopwatch = Stopwatch.StartNew();
            var response = new SearchResponseDto();

            try
            {
                var searchTerms = query.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                var searchEngines = _searchServiceFactory.GetAllSearchEngineNames();
                long googleHits = 0;
                long bingHits = 0;

                foreach (var term in searchTerms)
                {
                    foreach (var engine in searchEngines)
                    {
                        var searchService = _searchServiceFactory.CreateSearchService(engine);
                        var hits = await searchService.GetTotalSearchHits(term);

                        if (engine.Equals("google", StringComparison.OrdinalIgnoreCase))
                        {
                            googleHits += hits;
                        }
                        else if (engine.Equals("bing", StringComparison.OrdinalIgnoreCase))
                        {
                            bingHits += hits;
                        }
                    }
                }

                stopwatch.Stop();
                
                response.Success = true;
                response.NumberOfGoogleHits = googleHits;
                response.NumberOfBingHits = bingHits;
                response.TotalSumOfHits = googleHits + bingHits;
                response.ResponseTime = (int)stopwatch.ElapsedMilliseconds;                

                _logger.LogInformation("Total aggregation time for query '{query}' took {stopwatch.ElapsedMilliseconds} ms", query, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {                
                stopwatch.Stop();
                response.Success = false;
                response.ErrorResponseString = $"An error occurred during the search: {ex.Message}";
                response.ResponseTime = (int)stopwatch.ElapsedMilliseconds;

                _logger.LogError(ex, "An error occurred while aggregating search results for query: {query}", query);
            }

            return response;
        }
    }
}
