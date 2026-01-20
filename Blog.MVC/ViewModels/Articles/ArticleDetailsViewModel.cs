using Blog.Domain.Entities.Article;

namespace Blog.MVC.ViewModels.Articles
{
    public class ArticleDetailsViewModel
    {
        public required Guid Id { get; init; }

        public required string Title { get; init; }

        public required string Text { get; init; }

        public string? ImageUrl { get; init; }

        public required DateTime CreatedAt { get; init; }

        public required Guid AuthorId { get; init; }

        public required string AuthorUsername { get; init; }

        public required int Rating { get; init; }

        public ArticleRatingValue? AuthenticatedUserRating { get; init; }

        public required List<ArticleCommentPartialViewModel> Comments { get; init; }
    }
}
