using Blog.MVC.ViewModels.Articles;

namespace Blog.MVC.ViewModels.Home
{
    public class HomeIndexViewModel
    {
        public required IReadOnlyCollection<ArticlePreviewViewModel> LatestArticles { get; init; }
        public required IReadOnlyCollection<ArticlePreviewViewModel> TopRatedArticles { get; init; }
        public required IReadOnlyCollection<ArticlePreviewViewModel> RecentlyCommentedArticles { get; init; }
    }
}
