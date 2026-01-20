namespace Blog.Application.Articles.Requests
{
    public record CreateArticleImageRequest
    {
        public required string FileName { get; init; }
        public required string OriginalFileName { get; init; }
    }
}
