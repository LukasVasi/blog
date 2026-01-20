namespace Blog.Application.Users.Requests
{
    public record SignUpRequest
    {
        public required string EmailAddress { get; init; }

        public required string Username { get; init; }

        public required string Password { get; init; }

        public required string ConfirmPassword { get; init; }
    }
}
