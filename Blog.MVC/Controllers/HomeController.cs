using Blog.Application.Interfaces;
using Blog.Infrastructure.Security;
using Blog.MVC.Mapping;
using Blog.MVC.Mapping.Article;
using Blog.MVC.ViewModels;
using Blog.MVC.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Blog.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticleService _articleService;

        public HomeController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authenticatedUserId = User.Identity?.IsAuthenticated == true
                ? User.GetAuthenticatedUserId()
                : (Guid?)null;

            var getHomePageDataResult = await _articleService.GetHomePageDataAsync(authenticatedUserId);

            if (getHomePageDataResult.IsSuccess)
            {
                var homePageDataDto = getHomePageDataResult.Value;
                var viewModel = new HomeIndexViewModel
                {
                    LatestArticles = homePageDataDto.LatestArticles
                        .Select(article => article.ToPreviewViewModel())
                        .ToList(),

                    TopRatedArticles = homePageDataDto.TopRatedArticles
                        .Select(article => article.ToPreviewViewModel())
                        .ToList(),

                    RecentlyCommentedArticles = homePageDataDto.RecentlyCommentedArticles
                        .Select(article => article.ToPreviewViewModel())
                        .ToList()
                };

                return View(viewModel);
            }
            else
            {
                return getHomePageDataResult.Errors.First().ToErrorActionResult();
            }
        }

        [HttpGet]
        public IActionResult Search(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return RedirectToAction("Index", "Articles");
            }

            return RedirectToAction("Index", "Articles", new { search = searchQuery });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
