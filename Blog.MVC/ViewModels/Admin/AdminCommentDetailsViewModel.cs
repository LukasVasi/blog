using Blog.MVC.ViewModels.Articles;

namespace Blog.MVC.ViewModels.Admin
{
    public class AdminCommentDetailsViewModel
    {
        public required Guid CommentId { get; init; }

        public required string Text { get; init; }
        public required DateTime CreatedAt { get; init; }

        public required Guid CommentatorId { get; init; }
        public required string CommentatorUsername { get; init; }

        public required Guid ArticleId { get; init; }
        public required string ArticleTitle { get; init; }

        public required bool IsHidden { get; init; }

        public required IReadOnlyList<AdminArticleCommentReportViewModel> Reports { get; init; }
    }
}
