namespace Blog.MVC.ViewModels.Admin
{
    public class ReportedCommentPreviewViewModel
    {
        public required Guid CommentId { get; init; }
        public required string PreviewText { get; init; }
        public required int ReportCount { get; init; }
        public required DateTime LastReportedAt { get; init; }
    }
}
