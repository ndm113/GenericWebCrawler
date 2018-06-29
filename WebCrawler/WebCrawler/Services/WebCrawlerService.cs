using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebCrawler.Services
{
    public class WebCrawlerService : IWebCrawlerService
    {
        private IWebContentRetrievalService webContentRetrievalService;
        private ILinksExtractionService linksExtractionService;        
        /// <summary>
        /// Miliseconds to wait before atempting to get an item from the queue, if the queue was empty
        /// </summary>
        private readonly int QUEUE_EMPTY_WAIT_TIMEOUT = 2000;
        /// <summary>
        /// Number of times to check if the queue is empty, before terminating the crawl thread
        /// </summary>
        private readonly int QUEUE_EMPTY_RETRY_COUNTER = 3;
        /// <summary>
        /// Number of threads used to crawl a website, per request
        /// </summary>
        private readonly int THREAD_COUNT = 4;
        /// <summary>
        /// Thread wait interval
        /// </summary>
        private readonly TimeSpan QueueEmptyWaitInterval;

        public WebCrawlerService(IWebContentRetrievalService webContentRetrievalService,
            ILinksExtractionService linksExtractionService)
        {
            this.webContentRetrievalService = webContentRetrievalService;
            this.linksExtractionService = linksExtractionService;
            QueueEmptyWaitInterval = TimeSpan.FromMilliseconds(QUEUE_EMPTY_WAIT_TIMEOUT);
        }

        /// <summary>
        /// Crawls a website based off the provided url
        /// the main domain is extracted from the url and used 
        /// to limit the pages crawled
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public ConcurrentDictionary<string, IEnumerable<Uri>> CrawlWebsite(string domain) {
            ConcurrentDictionary<string, IEnumerable<Uri>> crawledPages = new ConcurrentDictionary<string, IEnumerable<Uri>>();
            ConcurrentDictionary<Uri, byte> enqueuedPages = new ConcurrentDictionary<Uri, byte>();
            ConcurrentQueue<Uri> pagesToCrawl = new ConcurrentQueue<Uri>();
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var mainUri = new Uri(domain);
            pagesToCrawl.Enqueue(mainUri);
            enqueuedPages.TryAdd(mainUri, 0);

            //spawn tasks for crawling content
            var cancelationToken = cancellationTokenSource.Token;
            var crawlerTasks = new Task[THREAD_COUNT];
            for (int i = 0; i < crawlerTasks.Length; i++) {
                crawlerTasks[i] = new Task(async () => await Crawl(crawledPages, pagesToCrawl, enqueuedPages, domain, cancelationToken), TaskCreationOptions.LongRunning);
                crawlerTasks[i].Start();
            }
                        
            try
            {
                Task.WaitAll(crawlerTasks);
            }
            catch (AggregateException aggregateException)
            {
                aggregateException.Handle(exception => HandleOperationCanceled(exception, cancelationToken));
            }            

            return crawledPages;
        }

        private static bool HandleOperationCanceled(Exception exception, CancellationToken cancellationToken)
        {
            return exception is OperationCanceledException operationCanceled
                && operationCanceled.CancellationToken == cancellationToken;
        }

        private async Task Crawl(ConcurrentDictionary<string, IEnumerable<Uri>> siteMap, ConcurrentQueue<Uri> pagesToCrawl, ConcurrentDictionary<Uri, byte> enqueuedPages, string domain, CancellationToken cancellationToken) {
            short waitCounter = 0;
            while (cancellationToken.IsCancellationRequested == false)
            {                
                var queueNotEmpty = pagesToCrawl.TryDequeue(out Uri pageUri);
                if (queueNotEmpty == false)
                {                    
                    if (waitCounter < QUEUE_EMPTY_RETRY_COUNTER)
                    {
                        waitCounter++;                                                
                        await Task.Delay(QueueEmptyWaitInterval, cancellationToken);
                        continue;
                        
                    }
                    else {                        
                        break;
                    }
                }
                else {
                    waitCounter = 0;                    
                }       
                
                var pageLinks = CrawlPage(domain, pageUri.OriginalString);

                var linkAdded = siteMap.TryAdd(pageUri.OriginalString, pageLinks);
                if (linkAdded == false)
                {
                    //raise an error, we shouldn't be crawling the same page multiple times
                    throw new InvalidOperationException("Page with url: " + pageUri + "Has already been crawled");
                }

                AddLinksToCrawlQueue(siteMap, pagesToCrawl, enqueuedPages, pageLinks);
                //TODO: add logging that a page has been crawled? maybe debug level keep track of the contents of the queue                
            }
        }

        /// <summary>
        /// Crawls a web page, returns a collection of the links present on the page
        /// </summary>
        /// <param name="domain">The domain to limit the crawling to</param>
        /// <param name="pageUrl">The page to crawl</param>
        /// <returns></returns>
        private IEnumerable<Uri> CrawlPage(string domain, string pageUrl) {
            HtmlDocument page = webContentRetrievalService.GetHtmlContentFromUrl(pageUrl);
            var pageLinks = linksExtractionService.ExtractLinksFromDocument(page).Distinct();
            pageLinks = FilterOutNonDomainUrls(pageLinks, domain);
            
            return pageLinks;
        }

        /// <summary>
        /// Takes a collection of uri's representing pages
        /// and adds them to the crawl queue, if they
        /// have not been crawled yet
        /// </summary>
        /// <param name="siteMap">A collection of pages that have been crawled so far </param>
        /// <param name="enqueuedPages">A collection of pages that are either crawled or are to be crawled</param>
        /// <param name="links">A list of links to be crawled if not already crawled</param>
        private void AddLinksToCrawlQueue(ConcurrentDictionary<string, IEnumerable<Uri>> siteMap,
            ConcurrentQueue<Uri> pagesToCrawl, 
            ConcurrentDictionary<Uri, byte> enqueuedPages, 
            IEnumerable<Uri> links) {
            foreach (Uri link in links) {
                //TODO: will need to format the web page uri to remove the www. appended, otherwise the same page can be crawled multiple times
                if (enqueuedPages.TryAdd(link, 0)) {                    
                    pagesToCrawl.Enqueue(link);                    
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
