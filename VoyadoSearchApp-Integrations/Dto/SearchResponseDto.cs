using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VoyadoSearchApp_Integrations.Dto
{
    public class SearchResponseDto
    {
        public bool Success { get; set; }        
        public string? ErrorResponseString { get; set; }
        public long NumberOfGoogleHits { get; set; }
        public long NumberOfBingHits { get; set; }
        public long TotalSumOfHits { get; set; }
        public int ResponseTime { get; set; }
    }
}
