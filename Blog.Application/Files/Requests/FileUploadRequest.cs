namespace Blog.Application.Files.Dtos
{
    public record FileUploadRequest
    {
        public required string FileName { get; init; }
        public required string ContentType { get; init; }
        public required byte[] Content { get; init; }
    }
}
