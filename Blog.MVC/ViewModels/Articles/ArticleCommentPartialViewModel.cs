namespace Blog.MVC.ViewModels.Articles
{
    public class ArticleCommentPartialViewModel
    {
        public required ArticleCommentViewModel Comment { get; init; }

        public required ArticleCommentReportViewModel Report { get; init; }

        /// <summary>
        /// The Ids of the users that have reported this article comment.
        /// </summary>
        public required IReadOnlyCollection<Guid> ReporterIds { get; init; }
    }
}
