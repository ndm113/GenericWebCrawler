using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawler.Models
{
    /// <summary>
    /// Represent a request to crawl a website based off the provided url
    /// </summary>
    public class PageRequestDto
    {
        public string Url;
    }
}
