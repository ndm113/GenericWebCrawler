using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawler.Services
{
    public interface ILinksExtractionService
    {
        IEnumerable<Uri> ExtractLinksFromDocument(HtmlDocument htmlDocument);
    }
}
