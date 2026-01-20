namespace Blog.Application.Users.Requests
{
    public record SignInRequest
    {
        public required string Username { get; init; }

        public required string Password { get; init; }
    }
}
