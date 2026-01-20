namespace Blog.Application.Users.Requests
{
    public record UpdateEmailAddressRequest
    {
        public required Guid UserId { get; init; }

        public required string NewEmailAddress { get; init; }
    }
}
