using Blog.Application.Articles.Dtos;

namespace Blog.Application.Articles.Results
{
    public record GetHomePageDataResult
    {
        public required List<ArticleDto> LatestArticles { get; init; }
        public required List<ArticleDto> TopRatedArticles { get; init; }
        public required List<ArticleDto> RecentlyCommentedArticles { get; init; }
    }
}
