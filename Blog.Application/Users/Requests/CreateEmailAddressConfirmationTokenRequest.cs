namespace Blog.Application.Users.Requests
{
    public record CreateEmailAddressConfirmationTokenRequest
    {
        public required Guid UserId { get; init; }

        public required string EmailAddress { get; init; }
    }
}
