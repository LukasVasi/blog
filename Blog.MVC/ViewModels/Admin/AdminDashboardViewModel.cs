namespace Blog.MVC.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public required int TotalUsers { get; init; }
        public required int ReportedCommentsCount { get; init; }
        public required int HiddenCommentsCount { get; init; }

        public required IReadOnlyCollection<ReportedCommentPreviewViewModel> RecentlyReportedComments { get; init; }
    }

}
