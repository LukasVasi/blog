using Blog.Application.Articles.Dtos;

namespace Blog.Application.Admin.Results
{
    public record GetAdminDashboardDataResult
    {
        public required int TotalUsers { get; init; }
        public required int ReportedCommentsCount { get; init; }
        public required int HiddenCommentsCount { get; init; }

        public required IReadOnlyCollection<ArticleCommentDto> RecentlyReportedComments { get; init; }
    }
}
