namespace Blog.Application.Articles.Requests
{
    public record UpdateArticleRequest
    {
        public required Guid Id { get; init; }

        public required string Title { get; init; }

        public required string Text { get; init; }

        public Guid? ImageId { get; init; }
    }
}
