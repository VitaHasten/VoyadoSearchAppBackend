using System.Numerics;
using VoyadoSearchApp_Integrations.Dto;

namespace VoyadoSearchApp.Logic.Interfaces
{
    public interface ISearchAggregatorService
    {
        public Task<SearchResponseDto> AggregateSearchResults(string query);
    }
}
