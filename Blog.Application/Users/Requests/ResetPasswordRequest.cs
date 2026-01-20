namespace Blog.Application.Users.Requests
{
    public record ResetPasswordRequest
    {
        public required string NewPassword { get; init; }

        public required string ConfirmPassword { get; init; }

        public required string Token { get; init; }
    }
}
