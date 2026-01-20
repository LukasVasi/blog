using Blog.Application.Articles.Enums;

namespace Blog.Application.Articles.Requests
{
    public record GetArticlesRequest
    {
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required ArticleSortOrder SortOrder { get; init; }

        public string? SearchQuery { get; init; } = null;
        public ArticleSearchType SearchType { get; set; } = ArticleSearchType.All;

        public Guid? AuthorId { get; init; } = null;
    }
}
