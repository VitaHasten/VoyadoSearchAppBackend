using Microsoft.Extensions.DependencyInjection;
using VoyadoSearchApp.Logic.Interfaces;
using VoyadoSearchApp_Integrations.Interfaces;
using VoyadoSearchApp_Integrations.Services;

namespace VoyadoSearchApp.Logic.Services
{
    public class SearchServiceFactory(IServiceProvider serviceProvider) : ISearchServiceFactory
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public ISearchService CreateSearchService(string searchEngine)
        {
            return searchEngine.ToLower() switch
            {
                "google" => _serviceProvider.GetRequiredService<GoogleService>(),
                "bing" => _serviceProvider.GetRequiredService<BingService>(),
                _ => throw new ArgumentException("Invalid search engine")
            };
        }

        public List<string> GetAllSearchEngineNames()
        {
            return ["google", "bing"];
        }
    }
}