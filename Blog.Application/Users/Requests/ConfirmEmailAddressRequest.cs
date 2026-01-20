namespace Blog.Application.Users.Requests
{
    public record ConfirmEmailAddressRequest
    {
        public required Guid UserId { get; init; }

        public required string Token { get; init; }
    }
}
