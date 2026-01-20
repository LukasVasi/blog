using Blog.Application;
using Blog.Application.Admin.Results;
using Blog.Application.Articles.Dtos;
using Blog.Application.Articles.Enums;
using Blog.Application.Articles.Mapping;
using Blog.Application.Articles.Requests;
using Blog.Application.Articles.Results;
using Blog.Application.Errors;
using Blog.Application.Interfaces;
using Blog.Application.Users.Mapping;
using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Blog.Domain.Enums;
using Blog.Domain.Validation.Article;
using Blog.Infrastructure.Persistence.Context;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Blog.Infrastructure.Services
{
    internal class ArticleService : IArticleService
    {
        private readonly BlogDbContext _context;

        public ArticleService(BlogDbContext context)
        {
            _context = context;
        }

        public async Task<Result<GetAdminDashboardDataResult>> GetAdminDashboardDataAsync(Guid authenticatedUserId)
        {
            var authenticatedUser = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (authenticatedUser == null)
            {
                return Result.Fail(new UnauthorizedError("Authenticated user does not exist."));
            }

            if (!authenticatedUser.Roles.Any(role => role.Id == UserRoleEnum.Admin))
            {
                return Result.Fail(new ForbiddenError("User must be an administrator."));
            }

            var totalUsers = await _context.Users
                .AsNoTracking()
                .CountAsync();

            var hiddenCommentsCount = await _context.ArticleComments
                .AsNoTracking()
                .Where(comment => comment.IsHidden)
                .CountAsync();

            var reportedCommentsQuery = _context.ArticleComments
                .AsNoTracking()
                .Include(comment => comment.Reports)
                .Where(comment => comment.Reports.Any());

            var reportedCommentsCount = await reportedCommentsQuery.CountAsync();

            var recentlyReportedComments = await reportedCommentsQuery
                .OrderByDescending(comment => comment.Reports.Select(report => report.CreatedAt).Max())
                .Take(5)
                .ToListAsync();

            return Result.Ok(new GetAdminDashboardDataResult
            {
                TotalUsers = totalUsers,
                ReportedCommentsCount = reportedCommentsCount,
                HiddenCommentsCount = hiddenCommentsCount,
                RecentlyReportedComments = recentlyReportedComments.Select(comment => comment.ToDto()).ToList()
            });
        }

        public async Task<Result<GetHomePageDataResult>> GetHomePageDataAsync(Guid? authenticatedUserId)
        {
            var baseQuery = _context.Articles
                .AsNoTracking()
                .Include(a => a.Author)
                .Include(a => a.Image)
                .Include(a => a.Ratings)
                .Include(article => article.Comments
                    .Where(comment => !comment.IsHidden));

            var latestArticles = await baseQuery
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToListAsync();

            var topRatedArticles = await baseQuery
                .Where(a => a.Ratings.Any())
                .OrderByDescending(a => a.Ratings
                    .Sum(r => r.Value == ArticleRatingValue.Positive ? 1 : -1))
                .ThenByDescending(a => a.CreatedAt)
                .Take(3)
                .ToListAsync();

            var recentlyCommentedArticles = await baseQuery
                .Where(a => a.Comments.Any())
                .OrderByDescending(a => a.Comments.Max(c => c.CreatedAt))
                .Take(3)
                .ToListAsync();

            return new GetHomePageDataResult
            {
                LatestArticles = latestArticles.Select(a => a.ToDto(authenticatedUserId)).ToList(),
                TopRatedArticles = topRatedArticles.Select(a => a.ToDto(authenticatedUserId)).ToList(),
                RecentlyCommentedArticles = recentlyCommentedArticles.Select(a => a.ToDto(authenticatedUserId)).ToList()
            };
        }

        public async Task<Result<GetArticlesResult>> GetArticlesAsync(GetArticlesRequest getArticlesRequest, Guid? authenticatedUserId)
        {
            IQueryable<Article> query = _context.Articles
                .AsNoTracking()
                .Include(article => article.Author)
                .Include(article => article.Image)
                .Include(article => article.Ratings)
                .Include(article => article.Comments
                    .Where(comment => !comment.IsHidden || (authenticatedUserId != null && comment.UserId == authenticatedUserId)));

            User? author = null;
            if (getArticlesRequest.AuthorId != null)
            {
                author = await _context.Users
                    .AsNoTracking()
                    .Where(user => user.Id == getArticlesRequest.AuthorId)
                    .FirstOrDefaultAsync();

                if (author == null)
                {
                    return Result.Fail(new NotFoundError("Author not found."));
                }

                query = query.Where(article => article.AuthorId == author.Id);
            }

            var searching = !string.IsNullOrWhiteSpace(getArticlesRequest.SearchQuery);
            if (searching)
            {
                var searchTerm = getArticlesRequest.SearchQuery!.Trim();

                query = getArticlesRequest.SearchType switch
                {
                    ArticleSearchType.Title => query.Where(a =>
                        EF.Functions.Like(a.Title, $"%{searchTerm}%")),

                    ArticleSearchType.Text => query.Where(a =>
                        EF.Functions.Like(a.Text, $"%{searchTerm}%")),

                    ArticleSearchType.Author => query.Where(a =>
                        EF.Functions.Like(a.Author.Username, $"%{searchTerm}%")),

                    ArticleSearchType.All => query.Where(a =>
                        EF.Functions.Like(a.Title, $"%{searchTerm}%") ||
                        EF.Functions.Like(a.Text, $"%{searchTerm}%") ||
                        EF.Functions.Like(a.Author.Username, $"%{searchTerm}%")),

                    _ => query
                };
            }

            query = getArticlesRequest.SortOrder switch
            {
                ArticleSortOrder.Oldest => query.OrderBy(article => article.CreatedAt),
                ArticleSortOrder.Latest => query.OrderByDescending(article => article.CreatedAt),
                ArticleSortOrder.Ranking => query.OrderByDescending(article => article.Ratings
                    .Sum(articleRating => articleRating.Value == ArticleRatingValue.Positive ? 1 : -1)
                    )
                    .ThenByDescending(article => article.CreatedAt),
                ArticleSortOrder.Relevance => OrderByRelevence(query, getArticlesRequest.SearchQuery),
                _ => query
            };

            var totalCount = await query.CountAsync();
            var maxPage = Math.Max(1, (int)Math.Ceiling(totalCount / (double)getArticlesRequest.PageSize));
            var page = Math.Clamp(getArticlesRequest.Page, 1, maxPage);

            var articles = await query
                .Skip((page - 1) * getArticlesRequest.PageSize)
                .Take(getArticlesRequest.PageSize)
                .ToListAsync();

            var articleDtos = articles
                .Select(article => article.ToDto(authenticatedUserId))
                .ToList();

            return Result.Ok(new GetArticlesResult
            {
                Articles = new PagedResult<ArticleDto>
                {
                    Items = articleDtos,
                    TotalCount = totalCount,
                    Page = getArticlesRequest.Page,
                    PageSize = getArticlesRequest.PageSize
                },
                SortOrder = getArticlesRequest.SortOrder,
                SearchQuery = getArticlesRequest.SearchQuery,
                SearchType = getArticlesRequest.SearchType,
                AuthorId = author?.Id,
                AuthorUsername = author?.Username
            });
        }

        /// <summary>
        /// Order the query by relevence: 
        /// first orders by exact title match to search query, 
        /// then by title starts with search query, 
        /// then orders by title contains the search query, 
        /// then by text contains, 
        /// then by author username match to search query,
        /// then by rating and finally by date.
        /// <para>
        /// If the search query provided is empty - orders by rating and date.
        /// </para>
        /// </summary>
        /// <param name="query">The query to be ordered.</param>
        /// <param name="searchQuery">
        /// The search query.
        /// </param>
        /// <returns>
        /// The query with the ordering applied.
        /// </returns>
        private static IOrderedQueryable<Article> OrderByRelevence(IQueryable<Article> query, string? searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return query.OrderByDescending(a => a.Ratings.Sum(r => r.Value == ArticleRatingValue.Positive ? 1 : -1))
                    .ThenByDescending(a => a.CreatedAt);
            }
            else
            {
                var searchTerm = searchQuery.Trim();

                return query
                    .OrderByDescending(a => a.Title == searchTerm)
                    .ThenByDescending(a => EF.Functions.Like(a.Title, $"{searchTerm}%"))
                    .ThenByDescending(a => EF.Functions.Like(a.Title, $"%{searchTerm}%"))
                    .ThenByDescending(a => EF.Functions.Like(a.Text, $"%{searchTerm}%"))
                    .ThenByDescending(a => EF.Functions.Like(a.Author.NormalizedUsername, $"%{searchTerm}%"))
                    .ThenByDescending(a => a.Ratings.Sum(r => r.Value == ArticleRatingValue.Positive ? 1 : -1))
                    .ThenByDescending(a => a.CreatedAt);
            }
        }

        public async Task<Result<ArticleDto>> GetArticleByIdAsync(Guid id, Guid? authenticatedUserId)
        {
            var article = await _context.Articles
                .AsNoTracking()
                .Include(article => article.Author)
                .Include(article => article.Image)
                .Include(article => article.Ratings)
                .Include(article => article.Comments
                    .Where(comment => !comment.IsHidden || (authenticatedUserId != null && comment.UserId == authenticatedUserId)))
                    .ThenInclude(comment => comment.User)
                .Include(article => article.Comments)
                        .ThenInclude(comment => comment.Reports)
                .FirstOrDefaultAsync(article => article.Id == id);

            if(article == null)
            {
                return Result.Fail(new NotFoundError("Article not found."));
            }
            else
            {
                var articleDto = article.ToDto(authenticatedUserId);
                return Result.Ok(articleDto);
            }
        }

        public async Task<Result<Guid>> CreateArticleAsync(CreateArticleRequest createArticleRequest, Guid authenticatedUserId)
        {
            var validationErrors = await ValidateCreateArticleRequestAsync(createArticleRequest, authenticatedUserId);

            if (validationErrors.Any())
            {
                return Result.Fail(validationErrors);
            }

            var authenticatedUser = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (authenticatedUser == null)
            {
                return Result.Fail(new UnauthorizedError("Authenticated user does not exist."));
            }

            if (!authenticatedUser.Roles.Any(role => role.Id == UserRoleEnum.Editor))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to create articles."));
            }

            ArticleImage? articleImage = null;
            if (createArticleRequest.ImageId != null)
            {
                articleImage = await _context.ArticleImages
                    .FirstOrDefaultAsync(articleImage =>
                        articleImage.Id == createArticleRequest.ImageId.Value &&
                        articleImage.UserId == authenticatedUserId &&
                        articleImage.ArticleId == null
                        );

                if (articleImage == null)
                {
                    return Result.Fail(new ValidationError("The selected image is invalid or unavailable. Please upload a new image."));
                }
            }

            var article = new Article()
            {
                Title = createArticleRequest.Title,
                Text = createArticleRequest.Text,
                AuthorId = authenticatedUser.Id
            };

            if (articleImage != null)
            {
                articleImage.ArticleId = article.Id;
            }

            using (var createArticleTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Articles.Add(article);

                    await _context.SaveChangesAsync();
                    await createArticleTransaction.CommitAsync();

                    return Result.Ok(article.Id);
                }
                catch
                {
                    await createArticleTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Result<ArticleDto>> UpdateArticleAsync(UpdateArticleRequest updateArticleRequest, Guid authenticatedUserId)
        {
            var validationErrors = await ValidateUpdateArticleRequestAsync(updateArticleRequest, authenticatedUserId);

            if (validationErrors.Any())
            {
                return Result.Fail(validationErrors);
            }

            var authenticatedUser = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (authenticatedUser == null)
            {
                return Result.Fail(new UnauthorizedError("Authenticated user does not exist."));
            }

            if (!authenticatedUser.Roles.Any(role => role.Id == UserRoleEnum.Editor))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to manage articles."));
            }

            var article = await _context.Articles
                .Include(article => article.Image)
                .FirstOrDefaultAsync(article => article.Id == updateArticleRequest.Id);

            if (article == null)
            {
                return Result.Fail(new NotFoundError("Article does not exist."));
            }

            if (article.AuthorId != authenticatedUser.Id)
            {
                return Result.Fail(new ForbiddenError("User is not allowed to update this article."));
            }

            article.Title = updateArticleRequest.Title;
            article.Text = updateArticleRequest.Text;

            var oldImageId = article.Image?.Id;
            var newImageId = updateArticleRequest.ImageId;

            if (oldImageId != newImageId)
            {
                if (article.Image != null)
                {
                    article.Image.ArticleId = null;
                }

                if (newImageId != null)
                {
                    var newImage = await _context.ArticleImages
                        .FirstOrDefaultAsync(articleImage =>
                            articleImage.Id == newImageId.Value &&
                            articleImage.UserId == authenticatedUserId &&
                            articleImage.ArticleId == null
                            );

                    if (newImage == null)
                    {
                        return Result.Fail(new ValidationError("The selected image is invalid or unavailable. Please upload a new image."));
                    }

                    newImage.ArticleId = article.Id;
                    article.Image = newImage;
                }
                else
                {
                    article.Image = null;
                }
            }

            using (var updateArticleTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await updateArticleTransaction.CommitAsync();

                    return Result.Ok(article.ToDto(authenticatedUserId));
                }
                catch
                {
                    await updateArticleTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Result> DeleteArticleAsync(Guid id, Guid authenticatedUserId)
        {
            var authenticatedUser = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (authenticatedUser == null)
            {
                return Result.Fail(new UnauthorizedError("Authenticated user does not exist."));
            }

            if (!authenticatedUser.Roles.Any(role => role.Id == UserRoleEnum.Editor))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to manage articles."));
            }

            var article = await _context.Articles
                .Include(article => article.Image)
                .FirstOrDefaultAsync(article => article.Id == id);

            if (article == null)
            {
                return Result.Fail(new NotFoundError("Article does not exist."));
            }

            if (article.AuthorId != authenticatedUser.Id)
            {
                return Result.Fail(new ForbiddenError("User is not allowed to delete this article."));
            }

            if(article.Image != null)
            {
                article.Image.ArticleId = null;
            }
            _context.Articles.Remove(article);

            using (var deleteArticleTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await deleteArticleTransaction.CommitAsync();

                    return Result.Ok();
                }
                catch
                {
                    await deleteArticleTransaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<Result<ArticleImageDto>> CreateArticleImageAsync(CreateArticleImageRequest createArticleImageRequest, Guid authenticatedUserId)
        {
            var authenticatedUser = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (authenticatedUser == null)
            {
                return Result.Fail(new UnauthorizedError("Authenticated user does not exist."));
            }

            if (!authenticatedUser.Roles.Any(role => role.Id == UserRoleEnum.Editor))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to manage articles."));
            }

            var articleImage = new ArticleImage()
            {
                FileName = createArticleImageRequest.FileName,
                OriginalFileName = createArticleImageRequest.OriginalFileName,
                UserId = authenticatedUser.Id
            };

            _context.ArticleImages.Add(articleImage);
            await _context.SaveChangesAsync();

            return Result.Ok(articleImage.ToDto());
        }

        public async Task<Result> RateArticleAsync(Guid articleId, ArticleRatingValue rating, Guid authenticatedUserId)
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(article => article.Id == articleId);

            if (article == null)
            {
                return Result.Fail(new NotFoundError("Article not found."));
            }

            var user = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (user == null)
            {
                return Result.Fail(new UnauthorizedError("User not found."));
            }

            if (!user.Roles.Any(role => role.Id == UserRoleEnum.Critic))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to rate articles."));
            }

            if(user.Id == article.AuthorId)
            {
                return Result.Fail(new ForbiddenError("Authors are not allowed to rate their own articles."));
            }

            var existingRating = await _context.ArticleRatings
                .FirstOrDefaultAsync(articleRating => articleRating.ArticleId == articleId && articleRating.UserId == user.Id);

            if (existingRating != null)
            {
                existingRating.Value = rating;
            }
            else
            {
                var newRating = new ArticleRating
                {
                    ArticleId = articleId,
                    UserId = user.Id,
                    Value = rating
                };
                _context.ArticleRatings.Add(newRating);
            }

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> DeleteRatingAsync(Guid articleId, Guid authenticatedUserId)
        {
            var user = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (user == null)
            {
                return Result.Fail(new UnauthorizedError("User not found."));
            }

            if (!user.Roles.Any(role => role.Id == UserRoleEnum.Critic))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to manage article ratings."));
            }

            var rating = await _context.ArticleRatings
                .FirstOrDefaultAsync(articleRating => articleRating.ArticleId == articleId && articleRating.UserId == user.Id);

            if (rating == null)
            {
                return Result.Fail(new NotFoundError("Article rating not found."));
            }

            _context.ArticleRatings.Remove(rating);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> CreateArticleCommentAsync(CreateArticleCommentRequest createArticleCommentRequest, Guid authenticatedUserId)
        {
            if (string.IsNullOrWhiteSpace(createArticleCommentRequest.Text))
            {
                return Result.Fail(new ValidationError("Comment text must be provided."));
            }

            var article = await _context.Articles
                .FirstOrDefaultAsync(article => article.Id == createArticleCommentRequest.ArticleId);

            if (article == null)
            {
                return Result.Fail(new NotFoundError("Article not found."));
            }

            var user = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (user == null)
            {
                return Result.Fail(new UnauthorizedError("User not found."));
            }

            if (!user.Roles.Any(role => role.Id == UserRoleEnum.Commentator))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to comment articles."));
            }

            var newArticleComment = new ArticleComment
            {
                Text = createArticleCommentRequest.Text,
                ArticleId = article.Id,
                UserId = user.Id
            };

            _context.ArticleComments.Add(newArticleComment);

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> UpdateArticleCommentAsync(UpdateArticleCommentRequest updateArticleCommentRequest, Guid authenticatedUserId)
        {
            if (string.IsNullOrWhiteSpace(updateArticleCommentRequest.Text))
            {
                return Result.Fail(new ValidationError("Comment text must be provided."));
            }

            var articleComment = await _context.ArticleComments
                .FirstOrDefaultAsync(comment => comment.Id == updateArticleCommentRequest.CommentId);

            if (articleComment == null)
            {
                return Result.Fail(new NotFoundError("Article comment not found."));
            }

            var user = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (user == null)
            {
                return Result.Fail(new UnauthorizedError("User not found."));
            }

            if (!user.Roles.Any(role => role.Id == UserRoleEnum.Commentator))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to manage article comments."));
            }

            if(user.Id != articleComment.UserId)
            {
                return Result.Fail(new ForbiddenError("User is not allowed to update this comment."));
            }

            articleComment.Text = updateArticleCommentRequest.Text;

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result> DeleteArticleCommentAsync(Guid commentId, Guid authenticatedUserId)
        {
            var articleComment = await _context.ArticleComments
                .FirstOrDefaultAsync(comment => comment.Id == commentId);

            if (articleComment == null)
            {
                return Result.Fail(new NotFoundError("Article comment not found."));
            }

            var user = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (user == null)
            {
                return Result.Fail(new UnauthorizedError("User not found."));
            }

            if (!user.Roles.Any(role => role.Id == UserRoleEnum.Commentator))
            {
                return Result.Fail(new ForbiddenError("User is not allowed to manage article comments."));
            }

            if (user.Id != articleComment.UserId)
            {
                return Result.Fail(new ForbiddenError("User is not allowed to delete this comment."));
            }

            _context.ArticleComments.Remove(articleComment);

            await _context.SaveChangesAsync();
            return Result.Ok();
        }

        public async Task<Result<Guid>> ReportArticleCommentAsync(ReportArticleCommentRequest reportArticleCommentRequest, Guid authenticatedUserId)
        {
            var articleComment = await _context.ArticleComments
                .FirstOrDefaultAsync(comment => comment.Id == reportArticleCommentRequest.CommentId);

            if (articleComment == null)
            {
                return Result.Fail(new NotFoundError("Article comment not found."));
            }

            var user = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (user == null)
            {
                return Result.Fail(new UnauthorizedError("User not found."));
            }

            var reportExists = await _context.ArticleCommentReports
                .AnyAsync(report => report.CommentId == reportArticleCommentRequest.CommentId && report.ReporterId == authenticatedUserId);

            if (reportExists)
            {
                return Result.Fail(new BadRequestError("A report for this comment by the current user has already been recorder."));
            }

            var report = new ArticleCommentReport
            {
                Reason = reportArticleCommentRequest.Reason,
                Description = reportArticleCommentRequest.Description,
                CommentId = reportArticleCommentRequest.CommentId,
                ReporterId = user.Id
            };

            _context.ArticleCommentReports.Add(report);
            await _context.SaveChangesAsync();

            return Result.Ok(report.Id);
        }

        public async Task<Result<IReadOnlyCollection<ArticleCommentDto>>> GetReportedCommentsAsync(Guid authenticatedUserId)
        {
            var authenticatedUser = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (authenticatedUser == null)
            {
                return Result.Fail(new UnauthorizedError("Authenticated user does not exist."));
            }

            if (!authenticatedUser.Roles.Any(role => role.Id == UserRoleEnum.Admin))
            {
                return Result.Fail(new ForbiddenError("User must be an administrator."));
            }

            var reportedComments = await _context.ArticleComments
                .AsNoTracking()
                .Include(comment => comment.Article)
                .Include(comment => comment.User)
                .Include(comment => comment.Reports)
                .Where(comment => comment.Reports.Any())
                .ToListAsync();

            return Result.Ok<IReadOnlyCollection<ArticleCommentDto>>(reportedComments.Select(comment => comment.ToDto()).ToList());
        }

        public async Task<Result> UpdateArticleCommentHiddenAsync(Guid commentId, bool isHidden, Guid authenticatedUserId)
        {
            var comment = await _context.ArticleComments
                .FirstOrDefaultAsync(comment => comment.Id == commentId);

            if(comment == null)
            {
                return Result.Fail(new NotFoundError("Article comment not found."));
            }

            var authenticatedUser = await _context.Users
                .Include(user => user.Roles)
                .FirstOrDefaultAsync(user => user.Id == authenticatedUserId);

            if (authenticatedUser == null)
            {
                return Result.Fail(new UnauthorizedError("Authenticated user does not exist."));
            }

            if (!authenticatedUser.Roles.Any(role => role.Id == UserRoleEnum.Admin))
            {
                return Result.Fail(new ForbiddenError("User must be an administrator."));
            }

            comment.IsHidden = isHidden;
            _context.Update(comment);
            await _context.SaveChangesAsync();

            return Result.Ok();
        }

        public async Task<Result<ArticleCommentDto>> GetArticleCommentByIdAsync(Guid commentId)
        {
            var comment = await _context.ArticleComments
                .AsNoTracking()
                .Include(comment => comment.Article)
                .Include(comment => comment.User)
                .Include(comment => comment.Reports)
                    .ThenInclude(report => report.Reporter)
                .FirstOrDefaultAsync(comment => comment.Id == commentId);

            if (comment == null)
            {
                return Result.Fail(new NotFoundError("Article comment not found."));
            }

            return Result.Ok(comment.ToDto());
        }

        private async Task<List<ValidationError>> ValidateCreateArticleRequestAsync(CreateArticleRequest createArticleRequest, Guid authenticatedUserId)
        {
            var validationErrors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(createArticleRequest.Title))
            {
                validationErrors.Add(
                    new ValidationError(TitleSpecifications.REQUIRED_ERROR_MESSAGE, nameof(createArticleRequest.Title))
                );
            }
            else
            {
                if (createArticleRequest.Title.Length > TitleSpecifications.MAX_LENGTH)
                {
                    validationErrors.Add(
                        new ValidationError(TitleSpecifications.MAX_LENGTH_ERROR_MESSAGE, nameof(createArticleRequest.Title))
                    );
                }
            }

            if (string.IsNullOrWhiteSpace(createArticleRequest.Text))
            {
                validationErrors.Add(
                    new ValidationError(TextSpecifications.REQUIRED_ERROR_MESSAGE, nameof(createArticleRequest.Text))
                );
            }

            if (createArticleRequest.ImageId != null)
            {
                var imageExists = await _context.ArticleImages
                    .AnyAsync(articleImage =>
                        articleImage.Id == createArticleRequest.ImageId &&
                        articleImage.UserId == authenticatedUserId &&
                        articleImage.ArticleId == null
                        );

                if (!imageExists)
                {
                    validationErrors.Add(new ValidationError("The selected image is invalid or unavailable. Please upload a new image.", nameof(createArticleRequest.ImageId)));
                }
            }

            return validationErrors;
        }

        private async Task<List<ValidationError>> ValidateUpdateArticleRequestAsync(UpdateArticleRequest updateArticleRequest, Guid authenticatedUserId)
        {
            var validationErrors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(updateArticleRequest.Title))
            {
                validationErrors.Add(
                    new ValidationError(TitleSpecifications.REQUIRED_ERROR_MESSAGE, nameof(updateArticleRequest.Title))
                );
            }
            else
            {
                if (updateArticleRequest.Title.Length > TitleSpecifications.MAX_LENGTH)
                {
                    validationErrors.Add(
                        new ValidationError(TitleSpecifications.MAX_LENGTH_ERROR_MESSAGE, nameof(updateArticleRequest.Title))
                    );
                }
            }

            if (string.IsNullOrWhiteSpace(updateArticleRequest.Text))
            {
                validationErrors.Add(
                    new ValidationError(TextSpecifications.REQUIRED_ERROR_MESSAGE, nameof(updateArticleRequest.Text))
                );
            }

            if (updateArticleRequest.ImageId != null)
            {
                var imageExists = await _context.ArticleImages
                    .AnyAsync(articleImage =>
                        articleImage.Id == updateArticleRequest.ImageId &&
                        articleImage.UserId == authenticatedUserId &&
                        (articleImage.ArticleId == null || articleImage.ArticleId == updateArticleRequest.Id)
                        );

                if (!imageExists)
                {
                    validationErrors.Add(new ValidationError("The selected image is invalid or unavailable. Please upload a new image.", nameof(updateArticleRequest.ImageId)));
                }
            }

            return validationErrors;
        }
    }
}
