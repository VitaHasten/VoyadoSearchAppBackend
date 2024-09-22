using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VoyadoSearchApp_Integrations.Interfaces;

namespace VoyadoSearchApp_Integrations.Services
{
    public class BingService(HttpClient httpClient, IConfiguration configuration, ILogger<BingService> logger) : ISearchService
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        private readonly string _apiKey = configuration["BingSearch:ApiKey"] ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ILogger<BingService> _logger = logger;

        public async Task<long> GetTotalSearchHits(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                throw new ArgumentNullException(nameof(query));
            }

            var relativeUri = $"/v7.0/search?q={Uri.EscapeDataString(query)}";

            var request = new HttpRequestMessage(HttpMethod.Get, relativeUri);
            request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

            try
            {
                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                
                dynamic parsedJson = JsonConvert.DeserializeObject(json);

                if (parsedJson != null)
                {
                    long totalHits = parsedJson.webPages.totalEstimatedMatches;

                    _logger.LogInformation("Total Bing-hits for query '{query}': {totalHits}", query, totalHits);

                    return totalHits;
                }

                else return -1;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching Bing.");
                throw;
            }
        }
    }
}