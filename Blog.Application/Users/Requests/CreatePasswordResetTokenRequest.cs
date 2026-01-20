namespace Blog.Application.Users.Requests
{
    public record CreatePasswordResetTokenRequest
    {
        public required string EmailAddress { get; init; }
    }
}
