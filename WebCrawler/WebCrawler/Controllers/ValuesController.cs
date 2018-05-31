using System;
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
    public class ValuesController : Controller
    {
        private IWebContentRetrievalService webContentRetrievalService;
        private ILinksExtractionService linksExtractionService;

        public ValuesController(IWebContentRetrievalService webContentRetrievalService,
            ILinksExtractionService linksExtractionService) {
            this.webContentRetrievalService = webContentRetrievalService;
            this.linksExtractionService = linksExtractionService;
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Hello", "World" };
        }

        [HttpPost("htmlcontent/")]
        public string GetHtmlContent([FromBody]PageRequest htmlPageRequest) {
            try
            {
                HtmlDocument htmlDocument = webContentRetrievalService.GetHtmlContentFromUrl(htmlPageRequest.Url);
                return htmlDocument.DocumentNode.InnerHtml;
            }
            catch (Exception ex) {
                return ex.Message;
            }
        }

        [HttpPost("links/")]
        public string GetLinks([FromBody]PageRequest htmlPageRequest)
        {
            try
            {
                HtmlDocument htmlDocument = webContentRetrievalService.GetHtmlContentFromUrl(htmlPageRequest.Url);
                var links = linksExtractionService.ExtractLinksFromDocument(htmlDocument);
                string cslinks = string.Join("\r\n", links.ToArray());
                return cslinks;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
