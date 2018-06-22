using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using WebCrawler.Models;
using WebCrawler.Services;

namespace WebCrawler.Controllers
{
    [Route("api/[controller]")]
    public class CrawlingController : Controller
    {
        private IWebContentRetrievalService webContentRetrievalService;
        private ILinksExtractionService linksExtractionService;
        private IWebCrawlerService webCrawlerService;

        public CrawlingController(IWebContentRetrievalService webContentRetrievalService,
            ILinksExtractionService linksExtractionService,
            IWebCrawlerService webCrawlerService) {
            this.webContentRetrievalService = webContentRetrievalService;
            this.linksExtractionService = linksExtractionService;
            this.webCrawlerService = webCrawlerService;
        }
        
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Hello", "World" };
        }


        [HttpPost("crawl/")]
        public ConcurrentDictionary<string, IEnumerable<Uri>> CrawlSite([FromBody]PageRequestDto htmlPageRequest)
        {
            try
            {
                var payload = webCrawlerService.CrawlWebsite(htmlPageRequest.Url);                
                
                return payload;
            }
            catch (Exception ex)
            {
                var stupid = new ConcurrentDictionary<string, IEnumerable<Uri>>();
                stupid.TryAdd(ex.Message, null);
                return stupid;
            }
        }
    }
}
