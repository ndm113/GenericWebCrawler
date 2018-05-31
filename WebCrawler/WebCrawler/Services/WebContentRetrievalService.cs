using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawler.Services
{
    public class WebContentRetrievalService : IWebContentRetrievalService
    {
        private HtmlWeb webContentRetriever;
        public WebContentRetrievalService() {
            webContentRetriever = new HtmlWeb();
        }

        /// <summary>
        /// Retrieves the html content for a web page, 
        /// given an url identifying it.
        /// </summary>
        /// <param name="webPageUrl">The url of the page to download.</param>
        /// <returns>The html content of the url</returns>
        public HtmlDocument GetHtmlContentFromUrl (string webPageUrl) {
            HtmlDocument webPage = webContentRetriever.Load(webPageUrl);
            return webPage;
        }
    }
}
