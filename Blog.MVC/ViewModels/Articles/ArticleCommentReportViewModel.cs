using Blog.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Articles
{
    public class ArticleCommentReportViewModel
    {
        public required Guid CommentId { get; init; }

        public ArticleCommentReportReason Reason { get; set; }

        public string? Description { get; set; }
    }
}
