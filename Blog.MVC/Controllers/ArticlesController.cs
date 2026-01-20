using Blog.Application.Articles.Enums;
using Blog.Application.Articles.Requests;
using Blog.Application.Errors;
using Blog.Application.Interfaces;
using Blog.Domain.Entities.Article;
using Blog.Infrastructure.Security;
using Blog.MVC.Mapping;
using Blog.MVC.Mapping.Article;
using Blog.MVC.Utility;
using Blog.MVC.ViewModels.Articles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Blog.MVC.Controllers
{
    [Authorize]
    [Route("articles")]
    public class ArticlesController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ArticlesController> _logger;

        public ArticlesController(IArticleService articleService, IFileStorageService fileStorageService, ILogger<ArticlesController> logger) 
        {
            _articleService = articleService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index(
            int page = 1, 
            int pageSize = 10, 
            string? search = null, 
            ArticleSearchType searchType = ArticleSearchType.All, 
            ArticleSortOrder sortOrder = ArticleSortOrder.Relevance,
            Guid? authorId = null)
        {
            var getArticlesRequest = new GetArticlesRequest()
            {
                Page = page,
                PageSize = pageSize,
                SortOrder = sortOrder,
                SearchQuery = search,
                SearchType = searchType,
                AuthorId = authorId
            };

            Guid? authenticatedUserId = (User.Identity != null && User.Identity.IsAuthenticated)
                ? User.GetAuthenticatedUserId()
                : null;

            var getArticlesResult = await _articleService.GetArticlesAsync(getArticlesRequest, authenticatedUserId);

            if (getArticlesResult.IsSuccess)
            {
                var articlePreviewViewModels = getArticlesResult.Value.Articles.Items
                    .Select(articleDto => articleDto.ToPreviewViewModel())
                    .ToList();

                var indexArticlesViewModel = new ArticleIndexViewModel
                {
                    ArticlePreviews = articlePreviewViewModels,
                    TotalCount = getArticlesResult.Value.Articles.TotalCount,
                    Page = getArticlesResult.Value.Articles.Page,
                    PageSize = getArticlesResult.Value.Articles.PageSize,
                    SortOrder = getArticlesResult.Value.SortOrder,
                    SearchQuery = getArticlesResult.Value.SearchQuery,
                    SearchType = getArticlesResult.Value.SearchType,
                    AuthorId = getArticlesResult.Value.AuthorId,
                    AuthorUsername = getArticlesResult.Value.AuthorUsername
                };

                return View(indexArticlesViewModel);
            }
            else
            {
                return getArticlesResult.Errors.First().ToErrorActionResult();
            }
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            Guid? authenticatedUserId = (User.Identity != null && User.Identity.IsAuthenticated)
                ? User.GetAuthenticatedUserId()
                : null;

            var getArticleResult = await _articleService.GetArticleByIdAsync(id, authenticatedUserId);

            if (getArticleResult.IsSuccess)
            {
                var articleDto = getArticleResult.Value;
                var articleDetailsViewModel = new ArticleDetailsViewModel
                {
                    Id = articleDto.Id,
                    Title = articleDto.Title,
                    ImageUrl = articleDto.Image?.Url,
                    Text = articleDto.Text,
                    CreatedAt = articleDto.CreatedAt,
                    AuthorId = articleDto.Author.Id,
                    AuthorUsername = articleDto.Author.Username,
                    Rating = articleDto.Rating,
                    AuthenticatedUserRating = articleDto.AuthenticatedUserRating,
                    Comments = articleDto.Comments
                        .Select(articleCommentDto => articleCommentDto.ToArticleCommentPartialViewModel())
                        .ToList()
                };

                return View(articleDetailsViewModel);
            }
            else
            {
                return getArticleResult.Errors.First().ToErrorActionResult();
            }
        }

        [Authorize(Policy = "RequireEditor")]
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            var createArticleViewModel = new CreateArticleViewModel();
            return View(createArticleViewModel);
        }

        [Authorize(Policy = "RequireEditor")]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateArticleViewModel createArticleViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "An error has been encountered when creating article.";
                return View(createArticleViewModel);
            }

            var authenticatedUserId = User.GetAuthenticatedUserId();

            var createArticleRequest = new CreateArticleRequest()
            {
                Title = createArticleViewModel.Title,
                ImageId = createArticleViewModel.Image.ImageId,
                Text = createArticleViewModel.Text
            };

            var createArticleResult = await _articleService.CreateArticleAsync(createArticleRequest, authenticatedUserId);

            if (createArticleResult.IsSuccess)
            {
                TempData["Success"] = "Successfully created a new article!";
                return RedirectToAction(nameof(Details), new { id = createArticleResult.Value });
            }
            else
            {
                if (createArticleResult.Errors.All(error => error is ValidationError))
                {
                    ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, createArticleResult);
                    TempData["Error"] = "One or more validation errors occured when creating article.";
                    return View(createArticleViewModel);
                }
                else if (createArticleResult.Errors.Count == 1)
                {
                    var error = createArticleResult.Errors.First();
                    TempData["Error"] = error.Message;
                    return error.ToErrorActionResult();
                }
                else
                {
                    _logger.LogError("Unexpected error state in article creation.");
                    TempData["Error"] = "An unexpected error has occured when creating article.";
                    return View("Error");
                }
            }
        }

        [Authorize(Policy = "RequireEditor")]
        [HttpGet("{id:guid}/update")]
        public async Task<IActionResult> Update(Guid id)
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var getArticleResult = await _articleService.GetArticleByIdAsync(id, authenticatedUserId);
            if (getArticleResult.IsSuccess)
            {
                var articleDto = getArticleResult.Value;

                if (articleDto.Author.Id != authenticatedUserId)
                {
                    TempData["Error"] = "User is not allowed to edit this article.";
                    return Forbid();
                }

                var updateArticleViewModel = new UpdateArticleViewModel()
                {
                    Id = id,
                    Title = articleDto.Title,
                    Image = new ArticleImageUploadFormViewModel()
                    {
                        ImageId = articleDto.Image?.Id,
                        CurrentImageUrl = articleDto.Image?.Url,
                    },
                    Text = articleDto.Text
                };

                return View(updateArticleViewModel);
            }
            else
            {
                var error = getArticleResult.Errors.First();
                TempData["Error"] = error.Message;
                return error.ToErrorActionResult();
            }
        }

        [Authorize(Policy = "RequireEditor")]
        [HttpPost("{id}/update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid id, UpdateArticleViewModel updateArticleViewModel)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "One or more validation errors occured when updating article.";
                return View(updateArticleViewModel);
            }

            var authenticatedUserId = User.GetAuthenticatedUserId();

            var updateArticleRequest = new UpdateArticleRequest()
            {
                Id = id,
                Title = updateArticleViewModel.Title,
                ImageId = updateArticleViewModel.Image.ImageId,
                Text = updateArticleViewModel.Text
            };

            var updateArticleResult = await _articleService.UpdateArticleAsync(updateArticleRequest, authenticatedUserId);

            if (updateArticleResult.IsSuccess)
            {
                TempData["Success"] = "Successfully updated the article!";
                return RedirectToAction(nameof(Details), new { id = updateArticleResult.Value.Id });
            }
            else
            {
                if (updateArticleResult.Errors.All(error => error is ValidationError))
                {
                    ResultErrorToModelErrorMapper.AddResultErrorsToModelState(ModelState, updateArticleResult);
                    TempData["Error"] = "One or more validation errors occured when updating article.";
                    return View(updateArticleViewModel);
                }
                else if (updateArticleResult.Errors.Count == 1)
                {
                    return updateArticleResult.Errors.First().ToErrorActionResult();
                }
                else
                {
                    _logger.LogError("Unexpected error state in article update.");
                    TempData["Error"] = "An unexpected error has occured when updating article.";
                    return View("Error");
                }
            }
        }

        [Authorize(Policy = "RequireEditor")]
        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var deleteArticleResult = await _articleService.DeleteArticleAsync(id, authenticatedUserId);
            if (deleteArticleResult.IsSuccess)
            {
                TempData["Success"] = "Successfully deleted the article!";
                return RedirectToAction(nameof(Index), new { authorId = authenticatedUserId });
            }
            else
            {
                if (deleteArticleResult.Errors.Count == 1)
                {
                    var error = deleteArticleResult.Errors.First();
                    TempData["Error"] = error.Message;
                    return error.ToErrorActionResult();
                }
                else
                {
                    _logger.LogError("Unexpected error state in article deletion.");
                    TempData["Error"] = "An unexpected error has occured when deleting article.";
                    return View("Error");
                }
            }
        }

        [Authorize(Policy = "RequireEditor")]
        [HttpPost("images")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadArticleImage(IFormFile articleImage)
        {
            if (articleImage == null || articleImage.Length == 0)
            {
                return BadRequest("The image file must be provided.");
            }

            if (articleImage.Length > 5 * 1024 * 1024)
            {
                return BadRequest("File size must be less than 5MB.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            var allowedContentTypes = new[]
            {
                "image/jpeg",
                "image/png",
                "image/gif"
            };

            var extension = Path.GetExtension(articleImage.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension) || !allowedContentTypes.Contains(articleImage.ContentType))
            {
                return BadRequest("Invalid file type. Only JPG, PNG, and GIF files are allowed.");
            }

            var articleImageUploadRequest = await articleImage.ToFileUploadRequestAsync();
            var fileUploadResult = await _fileStorageService.UploadFileAsync(articleImageUploadRequest);

            if (fileUploadResult.IsSuccess)
            {
                var uploadedFilePath = fileUploadResult.Value;
                var createArticleImageRequest = new CreateArticleImageRequest
                {
                    FileName = uploadedFilePath,
                    OriginalFileName = articleImageUploadRequest.FileName
                };

                var authenticatedUserId = User.GetAuthenticatedUserId();

                var createArticleImageResult = await _articleService.CreateArticleImageAsync(createArticleImageRequest, authenticatedUserId);
                if (createArticleImageResult.IsSuccess)
                {
                    return Ok(createArticleImageResult.Value);
                }
                else
                {
                    var error = createArticleImageResult.Errors.First();
                    return BadRequest(error.Message);
                }
            }
            else
            {
                var error = fileUploadResult.Errors.First();
                return BadRequest(error.Message);
            }
        }

        #region Ratings

        [Authorize(Policy = "RequireCritic")]
        [HttpPost("{id:guid}/rate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RateArticle(Guid id, int rating)
        {
            if (!Enum.IsDefined(typeof(ArticleRatingValue), rating))
            {
                TempData["Error"] = "Invalid rating value.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var authenticatedUserId = User.GetAuthenticatedUserId();

            var rateArticleResult = await _articleService.RateArticleAsync(id, (ArticleRatingValue)rating, authenticatedUserId);

            if (rateArticleResult.IsSuccess)
            {
                TempData["Success"] = "Your rating has been recorded.";
            }
            else
            {
                var error = rateArticleResult.Errors.First();
                TempData["Error"] = error.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [Authorize(Policy = "RequireCritic")]
        [HttpPost("{id:guid}/delete-rating")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteArticleRating(Guid id)
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var deleteRatingResult = await _articleService.DeleteRatingAsync(id, authenticatedUserId);

            if (deleteRatingResult.IsSuccess)
            {
                TempData["Success"] = "Your rating has been removed.";
            }
            else
            {
                var error = deleteRatingResult.Errors.First();
                TempData["Error"] = error.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        #endregion

        #region Comments

        [Authorize(Policy = "RequireCommentator")]
        [HttpPost("{articleId:guid}/comments/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(Guid articleId, string text)
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var createArticleCommentRequest = new CreateArticleCommentRequest
            {
                Text = text,
                ArticleId = articleId
            };

            var createArticleCommentResult = await _articleService.CreateArticleCommentAsync(createArticleCommentRequest, authenticatedUserId);

            if (createArticleCommentResult.IsSuccess)
            {
                TempData["Success"] = "Your comment has been created.";
            }
            else
            {
                var error = createArticleCommentResult.Errors.First();
                TempData["Error"] = error.Message;
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        [Authorize(Policy = "RequireCommentator")]
        [HttpPost("{articleId:guid}/comments/{commentId:guid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateComment(Guid articleId, Guid commentId, string text)
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var updateArticleCommentRequest = new UpdateArticleCommentRequest
            {
                Text = text,
                CommentId = commentId
            };

            var updateArticleCommentResult = await _articleService.UpdateArticleCommentAsync(updateArticleCommentRequest, authenticatedUserId);

            if (updateArticleCommentResult.IsSuccess)
            {
                TempData["Success"] = "Your comment has been updated.";
            }
            else
            {
                var error = updateArticleCommentResult.Errors.First();
                TempData["Error"] = error.Message;
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        [Authorize(Policy = "RequireCommentator")]
        [HttpPost("{articleId:guid}/comments/{commentId:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(Guid articleId, Guid commentId, string text)
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var deleteArticleCommentResult = await _articleService.DeleteArticleCommentAsync(commentId, authenticatedUserId);

            if (deleteArticleCommentResult.IsSuccess)
            {
                TempData["Success"] = "Your comment has been deleted.";
            }
            else
            {
                var error = deleteArticleCommentResult.Errors.First();
                TempData["Error"] = error.Message;
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        [HttpPost("{articleId:guid}/comments/{commentId:guid}/report")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportComment(
            Guid articleId,
            Guid commentId,
            [FromForm] ArticleCommentReportViewModel reportViewModel)
        {
            var authenticatedUserId = User.GetAuthenticatedUserId();

            var reportArticleCommentRequest = new ReportArticleCommentRequest
            {
                CommentId = commentId,
                Reason = reportViewModel.Reason,
                Description = reportViewModel.Description
            };

            var reportArticleCommentResult = await _articleService.ReportArticleCommentAsync(reportArticleCommentRequest, authenticatedUserId);

            if (reportArticleCommentResult.IsSuccess)
            {
                TempData["Success"] = "Comment has been successfully reported.";
            }
            else
            {
                var error = reportArticleCommentResult.Errors.First();
                TempData["Error"] = error.Message;
            }

            return RedirectToAction(nameof(Details), new { id = articleId });
        }

        #endregion
    }
}
