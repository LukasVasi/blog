using Blog.Application.Articles.Dtos;
using Blog.Application.Users.Mapping;
using Blog.Domain.Entities.Article;

namespace Blog.Application.Articles.Mapping
{
    public static class ArticleMapper
    {
        public static ArticleDto ToDto(this Article article, Guid? authenticatedUserId)
        {
            return new ArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Image = article.Image?.ToDto(),
                Text = article.Text,
                CreatedAt = article.CreatedAt,
                Author = article.Author.ToDto(),
                Rating = article.Ratings.Sum(rating => (int)rating.Value),
                AuthenticatedUserRating = authenticatedUserId == null
                        ? null
                        : article.Ratings
                            .Where(rating => rating.UserId == authenticatedUserId)
                            .Select(rating => (ArticleRatingValue?)rating.Value)
                            .FirstOrDefault(),
                Comments = article.Comments?
                    .Select(comment => comment.ToDto())
                    .ToList()
            };
        }

        public static ArticleImageDto ToDto(this ArticleImage image)
        {
            return new ArticleImageDto
            {
                Id = image.Id,
                FileName = image.FileName,
                OriginalFileName = image.OriginalFileName,
                CreatedAt = image.CreatedAt
            };
        }

        public static ArticleCommentDto ToDto(this ArticleComment comment)
        {
            return new ArticleCommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                IsHidden = comment.IsHidden,
                ArticleId = comment.ArticleId,
                ArticleTitle = comment.Article?.Title,
                CommentatorId = comment.UserId,
                CommentatorUsername = comment.User?.Username,
                Reports = comment.Reports?
                    .Select(report => report.ToDto())
                    .ToList()
            };
        }

        public static ArticleCommentReportDto ToDto(this ArticleCommentReport report)
        {
            return new ArticleCommentReportDto
            {
                Id = report.Id,
                Reason = report.Reason,
                Description = report.Description,
                CreatedAt = report.CreatedAt,
                ReporterId = report.ReporterId,
                ReporterUsername = report.Reporter?.Username,
                CommentId = report.CommentId
            };
        }
    }
}
