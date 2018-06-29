using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawler.DTO
{
    public class SiteMapResponseDTO
    {
        public bool RequestSuccessful { get; set; }
        public ConcurrentDictionary<string, IEnumerable<Uri>> SiteMap { get; set; }       
        public string ErrorMessage { get; set;}
    }
}
