using Blog.Domain.Enums;

namespace Blog.Domain.Entities.Article
{
    public class ArticleCommentReport
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required ArticleCommentReportReason Reason { get; init; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.Now;

        public required Guid CommentId { get; init; }
        public ArticleComment Comment { get; init; } = null!;

        public required Guid ReporterId { get; init; }
        public User.User Reporter { get; init; } = null!;
    }
}
