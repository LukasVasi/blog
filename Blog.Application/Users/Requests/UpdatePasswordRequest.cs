namespace Blog.Application.Users.Requests
{
    public record UpdatePasswordRequest
    {
        public required Guid UserId { get; init; }

        public required string NewPassword { get; init; }

        public required string ConfirmPassword { get; init; }
    }
}
