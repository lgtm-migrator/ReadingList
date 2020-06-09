﻿using AngleSharp.Html.Parser;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AddBook.Business.Search
{
    internal sealed class FnacSiteBookSearch : IBookSearch
    {
        private readonly HttpClient httpClient;

        public FnacSiteBookSearch()
        {
            httpClient = InstanciateHttpClient();
        }

        public async Task<Result<Book>> Search(string isbn)
        {
            try
            {
                var parser = new HtmlParser();
                var htmlDoc = parser.ParseDocument(await httpClient.GetStringAsync($"http://recherche.fnac.com/SearchResult/ResultList.aspx?SCat=2!1&sft=1&Search={isbn}"));
                var noResultDivs = htmlDoc.QuerySelectorAll("div.txt_c.noResults.mrg_b_xlg");
                if (noResultDivs.Any())
                {
                    return Result<Book>.Fail("Data not found.");
                }

                var bookLink = htmlDoc.QuerySelector("a.js-minifa-title");
                if (bookLink == null)
                {
                    return Result<Book>.Fail("Book infos link not found.");
                }

                htmlDoc = parser.ParseDocument(await httpClient.GetStringAsync(bookLink.Attributes["href"].Value));

                var characteristics = htmlDoc.QuerySelector("section[id='Characteristics']");

                var author = WebUtility.HtmlDecode(characteristics?.GetElementsByTagName("div")?
                    .Where(node => node.GetElementsByTagName("dt")?.First().TextContent == "Auteur")?
                    .Select(node => node.GetElementsByTagName("dd")?.First().TextContent)?.First()).Trim();

                var editor = WebUtility.HtmlDecode(characteristics?.GetElementsByTagName("div")?
                    .Where(node => node.GetElementsByTagName("dt")?.First().TextContent == "Editeur")?
                    .Select(node => node.GetElementsByTagName("dd")?.First().TextContent)?.First()).Trim();

                var coverUrl = htmlDoc.QuerySelector("img.js-ProductVisuals-imagePreview")?.Attributes["src"]?.Value;

                var title = WebUtility.HtmlDecode(htmlDoc.QuerySelector("h1.f-productHeader-Title")?.TextContent?.Trim());

                var summary = WebUtility.HtmlDecode(HtmlToMarkdown.Convert(htmlDoc.QuerySelector(".summaryStrate__raw")));

                return Result<Book>.Success(new Book { Isbn = isbn, Title = title, Author = author, Editor = editor, CoverUrl = coverUrl, Summary = summary });
            }
            catch (Exception ex)
            {
                return Result<Book>.Fail(ex.ToString());
            }
        }

        private HttpClient InstanciateHttpClient()
        {
            var httpClient = new HttpClient(new HttpClientHandler
                                    {
                                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                                    });
            httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US");
            httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Gecko/20100101 Firefox/65.0");
            // FIXME add dynamic referer?
            return httpClient;
        }
    }
}