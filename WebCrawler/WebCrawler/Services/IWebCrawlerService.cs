using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawler.Services
{
    public interface IWebCrawlerService
    {
        Dictionary<string, IEnumerable<Uri>> CrawlWebsite(string domain);
    }
}
