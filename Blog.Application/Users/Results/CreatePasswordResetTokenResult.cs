namespace Blog.Application.Users.Results
{
    public record CreatePasswordResetTokenResult
    {
        public required string EmailAddress { get; init; }
        
        public required string PasswordResetToken { get; init; }
    }
}
