namespace Blog.Application.Email.Requests
{
    public record SendPasswordResetRequest
    {
        public required string EmailAddress { get; init; }

        public required string ResetPasswordUrl { get; init; }
    }
}
