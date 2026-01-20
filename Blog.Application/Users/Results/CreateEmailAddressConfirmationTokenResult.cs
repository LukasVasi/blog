namespace Blog.Application.Users.Results
{
    public record CreateEmailAddressConfirmationTokenResult
    {
        public required Guid UserId { get; init; }

        public required string EmailAddress { get; init; }
        
        public required string EmailAddressConfirmationToken { get; init; }
    }
}
