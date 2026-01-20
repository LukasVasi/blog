namespace Blog.Application.Articles.Dtos
{
    public record ArticleImageDto
    {
        public required Guid Id { get; init; }
        public required string FileName { get; init; }
        public required string OriginalFileName { get; init; }
        public required DateTime CreatedAt { get; init; }

        public string Url => "/" + FileName;
    }
}
