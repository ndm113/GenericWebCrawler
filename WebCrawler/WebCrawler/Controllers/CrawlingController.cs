using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using WebCrawler.DTO;
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
        public SiteMapResponseDTO CrawlSite([FromBody]PageRequestDTO htmlPageRequest)
        {
            SiteMapResponseDTO response = new SiteMapResponseDTO();
            try
            {
                
                var siteMap = webCrawlerService.CrawlWebsite(htmlPageRequest.Url);
                response.RequestSuccessful = true;
                response.SiteMap = siteMap;
            }
            catch (Exception ex)
            {
                response.RequestSuccessful = false;
                response.SiteMap = null;
                response.ErrorMessage = "Sorry, something went wrong while attempting to crawl the website";
                //TODO: add a logging framework
            }
            return response;
        }
    }
}
