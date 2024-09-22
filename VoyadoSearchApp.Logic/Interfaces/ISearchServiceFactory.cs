using VoyadoSearchApp_Integrations.Interfaces;

namespace VoyadoSearchApp.Logic.Interfaces
{
    public interface ISearchServiceFactory
    {
        ISearchService CreateSearchService(string searchEngine);
        public List<string> GetAllSearchEngineNames();
    }
}
