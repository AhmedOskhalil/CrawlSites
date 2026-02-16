namespace Crawl.Models
{
    public class ArticleContent
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string PublishedDate { get; set; }
        public string Text { get; set; }

        public string[]? Images { get; set; }   

        public string[]? VideoUrls { get; set; }

        public List<RelatedArticle>? RelatedArticles { get; set; }


    }
}
