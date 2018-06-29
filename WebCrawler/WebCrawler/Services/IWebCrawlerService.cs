using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WebCrawler.Services
{
    public interface IWebCrawlerService
    {
        ConcurrentDictionary<string, IEnumerable<Uri>> CrawlWebsite(string domain);
    }
}
