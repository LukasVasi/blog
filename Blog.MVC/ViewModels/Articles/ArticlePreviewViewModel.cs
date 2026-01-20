namespace Blog.MVC.ViewModels.Articles
{
    public class ArticlePreviewViewModel
    {
        public required Guid Id { get; init; }

        public required string Title { get; init; }

        public required DateTime CreatedAt { get; init; }

        public required Guid AuthorId { get; init; }

        public required string AuthorUsername { get; init; }

        public required int Rating { get; init; }

        public required int CommentCount { get; init; }

        public DateTime? LastCommentedAt { get; init; }
    }
}
