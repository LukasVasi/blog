using Blog.Application.Articles.Enums;

namespace Blog.MVC.ViewModels.Articles
{
    public partial class ArticleIndexViewModel
    {
        public ArticleSortOrder SortOrder { get; set; } = ArticleSortOrder.Relevance;

        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        public Guid? AuthorId { get; set; }
        public string? AuthorUsername { get; set; }

        public string? SearchQuery { get; set; } = null;
        public ArticleSearchType SearchType { get; set; } = ArticleSearchType.All;

        public int TotalPages =>
            (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public bool IsSearching => !string.IsNullOrWhiteSpace(SearchQuery);
        public bool HasAuthor => AuthorId != null && AuthorId != Guid.Empty;

        public List<ArticlePreviewViewModel> ArticlePreviews { get; set; } = new List<ArticlePreviewViewModel>();
    }
}
