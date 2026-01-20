namespace Blog.MVC.ViewModels.Articles
{
    public class ArticleCommentViewModel
    {
        public required Guid Id { get; init; }

        public required string Text { get; init; }

        public required DateTime CreatedAt { get; init; }

        public required bool IsHidden { get; init; }

        public required Guid ArticleId { get; init; }

        public required Guid CommentatorId { get; init; }

        public required string CommentatorUsername { get; init; }
    }
}
