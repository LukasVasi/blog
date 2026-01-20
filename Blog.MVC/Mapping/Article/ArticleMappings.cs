using Blog.Application.Articles.Dtos;
using Blog.MVC.ViewModels.Articles;

namespace Blog.MVC.Mapping.Article
{
    public static class ArticleMappings
    {
        public static ArticlePreviewViewModel ToPreviewViewModel(this ArticleDto dto)
        {
            return new ArticlePreviewViewModel
            {
                Id = dto.Id,
                Title = dto.Title,
                CreatedAt = dto.CreatedAt,
                AuthorId = dto.Author.Id,
                AuthorUsername = dto.Author.Username,
                Rating = dto.Rating,
                CommentCount = dto.Comments.Count,
                LastCommentedAt = dto.Comments
                    .Select(c => (DateTime?)c.CreatedAt)
                    .Max()
            };
        }

        public static ArticleCommentPartialViewModel ToArticleCommentPartialViewModel(this ArticleCommentDto dto)
        {
            var articleCommentViewModel = new ArticleCommentViewModel
            {
                Id = dto.Id,
                Text = dto.Text,
                CreatedAt = dto.CreatedAt,
                IsHidden = dto.IsHidden,
                ArticleId = dto.ArticleId,
                CommentatorId = dto.CommentatorId,
                CommentatorUsername = dto.CommentatorUsername!
            };

            var reportViewModel = new ArticleCommentReportViewModel
            {
                CommentId = dto.Id
            };

            if(dto.Reports == null)
            {
                throw new ArgumentNullException("Reports need to be included to convert comment to partial view model.");
            }

            var reporterIds = dto.Reports.
                Select(report => report.ReporterId)
                .ToList();

            return new ArticleCommentPartialViewModel
            {
                Comment = articleCommentViewModel,
                Report = reportViewModel,
                ReporterIds = reporterIds
            };
        }
    }
}
