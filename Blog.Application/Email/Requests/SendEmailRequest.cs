namespace Blog.Application.Email.Requests
{
    public record SendEmailRequest
    {
        public required string From { get; set; }
        public required string To { get; set; }
        public required string Subject { get; set; }
        public required string PlainTextBody { get; set; }
        public string? HtmlBody { get; set; }
    }
}
