using Blog.Application.Articles.Dtos;
using Blog.Application.Articles.Enums;

namespace Blog.Application.Articles.Results
{
    public record GetArticlesResult
    {
        public required PagedResult<ArticleDto> Articles { get; init; }

        public required ArticleSortOrder SortOrder { get; init; }

        public string? SearchQuery { get; init; } = null;
        public ArticleSearchType SearchType { get; init; } = ArticleSearchType.All;

        public Guid? AuthorId { get; init; } = null;
        public string? AuthorUsername { get; init; } = null;
    }
}
