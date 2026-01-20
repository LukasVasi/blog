namespace Blog.Application.Articles.Requests
{
    public record CreateArticleRequest
    {
        public required string Title { get; init; }

        public required string Text { get; init; }

        public Guid? ImageId { get; init; }
    }
}
