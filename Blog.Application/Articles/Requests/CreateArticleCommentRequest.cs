namespace Blog.Application.Articles.Requests
{
    public record CreateArticleCommentRequest
    {
        public required Guid ArticleId { get; init; }
        
        public required string Text { get; init; }
    }
}
