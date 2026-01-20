namespace Blog.MVC.ViewModels.Admin
{
    public class ReportedCommentsListViewModel
    {
        public required IReadOnlyCollection<ReportedCommentListItemViewModel> Comments { get; init; }
    }
}
