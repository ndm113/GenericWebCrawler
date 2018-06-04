# GenericWebCrawler
A simple web crawler that returns a site map.

To crawl a website start up the project, and hit the following endpoint:
POST <server:port>/api/crawling/crawl/ (Content-Type: application/json)
with a payload body such as:
{ "url": "https://hirespace.com/"}

The response will be an object similar to:
{
    "https://hirespace.com/": [
        "https://venues.hirespace.com",
        "https://pro.hirespace.com",
        "https://hirespace.com/EventLAB/",
        "https://hirespace.com/blog/tag/conferences/",
        "https://hirespace.com/blog/tag/weddings/",
        "https://hirespace.com/blog/tag/unusual/",
        "https://hirespace.com/blog/tag/parties/",
        "https://hirespace.com/blog/tag/meetings/",
        "https://hirespace.com/blog/tag/private-dining/"
    ],
    "https://venues.hirespace.com": [],
    "https://pro.hirespace.com": [],
    "https://hirespace.com/EventLAB/": [
        "https://hirespace.com/",
        "https://hirespace.com/Policies#privacy"
    ],
	....	
}
	
	
The crawler will crawl links leading to subdomains as well, this can be changed in the code by
modifying the DoesLinkBelongToDomain method in the WebCrawlerService class.

Performance is very poor at the moment, as the pages are not crawled with a multi-threaded approach.
