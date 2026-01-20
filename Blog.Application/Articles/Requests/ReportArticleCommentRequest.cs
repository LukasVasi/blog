using Blog.Domain.Enums;

namespace Blog.Application.Articles.Requests
{
    public record ReportArticleCommentRequest
    {
        public required Guid CommentId { get; init; }

        public required ArticleCommentReportReason Reason { get; init; }

        public required string? Description { get; init; }
    }
}
