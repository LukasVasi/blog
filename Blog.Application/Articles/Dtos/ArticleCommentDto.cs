using Blog.Application.Users.Dtos;

namespace Blog.Application.Articles.Dtos
{
    public record ArticleCommentDto
    {
        public required Guid Id { get; init; }

        public required string Text { get; init; }

        public required DateTime CreatedAt { get; init; }

        public required bool IsHidden { get; init; }

        public required Guid ArticleId { get; init; }

        public string? ArticleTitle { get; init; }

        public required Guid CommentatorId { get; init; }

        public string? CommentatorUsername { get; init; }

        public IReadOnlyCollection<ArticleCommentReportDto>? Reports { get; init; }
    }
}
