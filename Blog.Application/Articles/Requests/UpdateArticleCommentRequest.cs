namespace Blog.Application.Articles.Requests
{
    public record UpdateArticleCommentRequest
    {
        public required Guid CommentId { get; init; }

        public required string Text { get; init; }
    }
}
