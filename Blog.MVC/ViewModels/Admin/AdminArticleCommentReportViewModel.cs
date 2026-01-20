using Blog.Domain.Enums;

namespace Blog.MVC.ViewModels.Admin
{
    public class AdminArticleCommentReportViewModel
    {
        public required ArticleCommentReportReason Reason { get; init; }
        public string? Description { get; init; }
        public required Guid ReporterId { get; init; }
        public required string ReporterUsername { get; init; }
        public required DateTime CreatedAt { get; init; }
    }
}
