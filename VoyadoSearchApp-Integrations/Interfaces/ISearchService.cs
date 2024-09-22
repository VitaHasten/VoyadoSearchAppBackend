using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VoyadoSearchApp_Integrations.Dto;

namespace VoyadoSearchApp_Integrations.Interfaces
{
    public interface ISearchService
    {        
        Task<long> GetTotalSearchHits(string term);
    }
}
