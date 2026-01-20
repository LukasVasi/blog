using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Blog.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Application.Articles.Dtos
{
    public record ArticleCommentReportDto
    {
        public required Guid Id { get; init; }

        public required ArticleCommentReportReason Reason { get; init; }

        public required string? Description { get; init; }

        public DateTime CreatedAt { get; init; }

        public required Guid CommentId { get; init; }

        public required Guid ReporterId { get; init; }

        public string? ReporterUsername { get; init; }
    }
}
