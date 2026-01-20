namespace Blog.MVC.ViewModels.Admin
{
    public class ReportedCommentListItemViewModel
    {
        public required Guid CommentId { get; init; }
        public required string PreviewText { get; init; }

        public required Guid CommentatorId { get; init; }
        public required string CommentatorUsername { get; init; }

        public required Guid ArticleId { get; init; }
        public required string ArticleTitle { get; init; }

        public required DateTime CreatedAt { get; init; }
        public required int ReportCount { get; init; }

        public required bool IsHidden { get; init; }
    }
}
