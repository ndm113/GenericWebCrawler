using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebCrawler.Services
{
    public class LinksExtractionService : ILinksExtractionService
    {
        /// <summary>
        /// Returns the complete list of links contained in the href
        /// attributes of the a tags in the provuded html document
        /// </summary>
        /// <param name="htmlDocument">The html document from which to extract links</param>
        /// <returns>An enumerable containing all link values on the page</returns>
        public IEnumerable<Uri> ExtractLinksFromDocument(HtmlDocument htmlDocument) {
            IEnumerable<HtmlNode> linkNodes = htmlDocument.DocumentNode.Descendants("a");
            IEnumerable<Uri> pageLinks = linkNodes.Where(node => node.Attributes["href"] != null 
            && Uri.IsWellFormedUriString(node.Attributes["href"].Value, UriKind.RelativeOrAbsolute))
                .Select(node => new Uri(node.Attributes["href"].Value));
            

            return pageLinks;
        }
    }
}
