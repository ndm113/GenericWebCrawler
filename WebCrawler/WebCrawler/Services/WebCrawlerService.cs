using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawler.Services
{
    public class WebCrawlerService : IWebCrawlerService
    {
        private IWebContentRetrievalService webContentRetrievalService;
        private ILinksExtractionService linksExtractionService;

        public WebCrawlerService(IWebContentRetrievalService webContentRetrievalService,
            ILinksExtractionService linksExtractionService)
        {
            this.webContentRetrievalService = webContentRetrievalService;
            this.linksExtractionService = linksExtractionService;
        }

        public Dictionary<string, IEnumerable<Uri>> CrawlWebsite(string domain) {
            Dictionary<string, IEnumerable<Uri>> crawledPages = new Dictionary<string, IEnumerable<Uri>>();
            Dictionary<Uri, bool> pagesToCrawl = new Dictionary<Uri, bool>();
            pagesToCrawl.Add(new Uri(domain), true);
            while (pagesToCrawl.Count > 0) {
                Uri pageUri = pagesToCrawl.First().Key;
                pagesToCrawl.Remove(pageUri);
                var pageLinks = CrawlPage(domain, pageUri.OriginalString, crawledPages);                
                AddLinksToCrawlQueue(crawledPages, pagesToCrawl, pageLinks);
                //TODO: add logging that a page has been crawled? maybe debug level keep track of the contents of the queue                
            }

            return crawledPages;
        }

        private IEnumerable<Uri> CrawlPage(string domain, string pageUrl, Dictionary<string, IEnumerable<Uri>> crawledPages) {
            HtmlDocument page = webContentRetrievalService.GetHtmlContentFromUrl(pageUrl);
            var pageLinks = linksExtractionService.ExtractLinksFromDocument(page).Distinct();
            pageLinks = FilterOutNonDomainUrls(pageLinks, domain);
            if (crawledPages.ContainsKey(pageUrl))
            {
                //raise an error, we shouldn't be crawling the same page multiple times
                throw new InvalidOperationException("Page with url: " + pageUrl + "Has already been crawled");                
            }
            else {
                crawledPages.Add(pageUrl, pageLinks);
            }
            return pageLinks;
        }

        private void AddLinksToCrawlQueue(Dictionary<string, IEnumerable<Uri>> crawledPages, Dictionary<Uri, bool> pagesToCrawl, IEnumerable<Uri> links) {
            foreach (Uri link in links) {
                //TODO: will need to format the web page uri to remove the www. appended, otherwise the same page can be crawled multiple times
                if (crawledPages.ContainsKey(link.OriginalString) == false && pagesToCrawl.ContainsKey(link) == false) {
                    pagesToCrawl.Add(link, true);
                }
            }
        }

        private IEnumerable<Uri> FilterOutNonDomainUrls(IEnumerable<Uri> links, string domain) {
            Uri uriForDomain = new Uri(domain);
            return links.Where(link => DoesLinkBelongToDomain(link, uriForDomain));
        }

        private bool DoesLinkBelongToDomain(Uri link, Uri domain) {
            var linkHostName = TrimHostName(link.Host);
            var domainHostName = TrimHostName(domain.Host);
            //Will also crawl sub domains this way
            return linkHostName.EndsWith(domainHostName);
        }

        private string TrimHostName(string hostName) {
            if (hostName.StartsWith("www."))
            {
                hostName = hostName.Substring(4);
            }
            return hostName;
        }
    }
}
