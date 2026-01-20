namespace Blog.Application.Users.Requests
{
    public record UpdateUsernameRequest
    {
        public required Guid UserId { get; init; }

        public required string NewUsername { get; init; }
    }
}
