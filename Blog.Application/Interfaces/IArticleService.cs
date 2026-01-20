using Blog.Application.Admin.Results;
using Blog.Application.Articles.Dtos;
using Blog.Application.Articles.Requests;
using Blog.Application.Articles.Results;
using Blog.Domain.Entities.Article;
using FluentResults;

namespace Blog.Application.Interfaces
{
    public interface IArticleService
    {
        /// <summary>
        /// Retrieves the data needed for the administration dashboard view.
        /// </summary>
        /// <param name="authenticatedUserId">
        /// The Id of the authenticated user.
        /// </param>
        /// <returns>The admin dashboard page data transfer object wrapped in <see cref="Result"/></returns>
        public Task<Result<GetAdminDashboardDataResult>> GetAdminDashboardDataAsync(Guid authenticatedUserId);

        /// <summary>
        /// Retrieves the data needed for the home page view.
        /// This includes: the top 5 latest articles, the top 3 highest ranked articles,
        /// the top 3 articles with the newest comments.
        /// </summary>
        /// <param name="authenticatedUserId">
        /// The Id of the authenticated user.
        /// </param>
        /// <returns>The home page data transfer object wrapped in <see cref="Result"/></returns>
        public Task<Result<GetHomePageDataResult>> GetHomePageDataAsync(Guid? authenticatedUserId);

        /// <summary>
        /// Retrieves a paged list of articles that match the filter criteria
        /// and are sorted by the order provided in the request.
        /// If the Id of the authenticated user is provided, it is used to retrieve
        /// the user's rating of each article.
        /// </summary>
        /// <param name="getArticlesRequest">
        /// Request containing page and filter information.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// The result object that contains the paged article list and additional 
        /// result context wrapped in a <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result<GetArticlesResult>> GetArticlesAsync(GetArticlesRequest getArticlesRequest, Guid? authenticatedUserId);

        /// <summary>
        /// Retrieves a specific article with the provided Id.
        /// If the Id of the authenticated user is provided, it is used to retrieve
        /// the user's rating of the article.
        /// </summary>
        /// <param name="id">
        /// The Id of the article.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// The article data transfer object wrapped in a <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result<ArticleDto>> GetArticleByIdAsync(Guid id, Guid? authenticatedUserId);

        /// <summary>
        /// Creates a new article.
        /// </summary>
        /// <param name="createArticleRequest">
        /// The request containing the article information.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// The Id of the created article wrapped in a <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result<Guid>> CreateArticleAsync(CreateArticleRequest createArticleRequest, Guid authenticatedUserId);

        /// <summary>
        /// Updates an article with new information.
        /// </summary>
        /// <param name="updateArticleRequest">
        /// The request containing the update information.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// The article data transfer object with the updated information wrapped in a <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result<ArticleDto>> UpdateArticleAsync(UpdateArticleRequest updateArticleRequest, Guid authenticatedUserId);

        /// <summary>
        /// Deletes an article.
        /// </summary>
        /// <param name="id">
        /// The Id of the article that is to be deleted.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// A <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result> DeleteArticleAsync(Guid id, Guid authenticatedUserId);

        /// <summary>
        /// Creates an article image.
        /// </summary>
        /// <param name="createArticleImageRequest">
        /// The request containing the creation information including the
        /// information of the associated image file upload.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// An article image data transfer object wrapped in a <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result<ArticleImageDto>> CreateArticleImageAsync(CreateArticleImageRequest createArticleImageRequest, Guid authenticatedUserId);

        /// <summary>
        /// Creates or updates the user's rating of an article.
        /// </summary>
        /// <param name="articleId">
        /// The Id of the article that the rating belongs to.
        /// </param>
        /// <param name="rating">
        /// The <see cref="ArticleRatingValue"/> of the rating.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// A <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result> RateArticleAsync(Guid articleId, ArticleRatingValue rating, Guid authenticatedUserId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="articleId">
        /// The Id of the article that the rating belongs to.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// A <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result> DeleteRatingAsync(Guid articleId, Guid authenticatedUserId);

        /// <summary>
        /// Creates an article comment provided.
        /// </summary>
        /// <param name="createArticleCommentRequest">
        /// The request containing the creation information.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// A <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result> CreateArticleCommentAsync(CreateArticleCommentRequest createArticleCommentRequest, Guid authenticatedUserId);

        /// <summary>
        /// Updates an existing article comment.
        /// </summary>
        /// <param name="updateArticleCommentRequest">
        /// The request containing the update information.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// A <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result> UpdateArticleCommentAsync(UpdateArticleCommentRequest updateArticleCommentRequest, Guid authenticatedUserId);

        /// <summary>
        /// Deletes an existing article comment.
        /// </summary>
        /// <param name="commentId">
        /// The Id of the comment that should be deleted.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// A <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result> DeleteArticleCommentAsync(Guid commentId, Guid authenticatedUserId);

        /// <summary>
        /// Creates a report for an article comment.
        /// </summary>
        /// <param name="reportArticleCommentRequest">
        /// The request containing the report creation information.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// The Id of the created report wrapped in a <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result<Guid>> ReportArticleCommentAsync(ReportArticleCommentRequest reportArticleCommentRequest, Guid authenticatedUserId);

        /// <summary>
        /// Retrieves the reported comments.
        /// </summary>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// The collection of reported comments wrapped in a <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result<IReadOnlyCollection<ArticleCommentDto>>> GetReportedCommentsAsync(Guid authenticatedUserId);

        /// <summary>
        /// Updates an article comment's hidden status.
        /// </summary>
        /// <param name="commentId">
        /// The Id of the article comment.
        /// </param>
        /// <param name="isHidden">
        /// The new hidden status value.
        /// </param>
        /// <param name="authenticatedUserId">
        /// The Id of the currently authenticated user making the request.
        /// </param>
        /// <returns>
        /// A <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result> UpdateArticleCommentHiddenAsync(Guid commentId, bool isHidden, Guid authenticatedUserId);

        /// <summary>
        /// Retrieves an article comment by its Id.
        /// </summary>
        /// <param name="commentId">
        /// The article comment Id.
        /// </param>
        /// <returns>
        /// An article comment dto wrapped in a <see cref="FluentResults.Result"/>.
        /// <para>
        /// If an error is encountered returns a failed result with an error 
        /// or multiple validation errors in case of validation failure.
        /// </para>
        /// </returns>
        public Task<Result<ArticleCommentDto>> GetArticleCommentByIdAsync(Guid commentId);
    }
}
