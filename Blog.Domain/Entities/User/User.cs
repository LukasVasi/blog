namespace Blog.Domain.Entities.User
{
    public class User
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// The current confirmed email address of the user.
        /// NULL if user has no confirmed email address.
        /// </summary>
        public string? EmailAddress { get; set; }

        /// <summary>
        /// The normalized (lower invariant) version of the email address.
        /// NULL if user has no confirmed email address.
        /// </summary>
        public string? NormalizedEmailAddress { get; set; }

        /// <summary>
        /// The username of the user.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// The normalized (lower invariant) version of the username.
        /// </summary>
        public required string NormalizedUsername { get; set; }

        /// <summary>
        /// The hash of the user's password.
        /// </summary>
        public required string PasswordHash { get; set; }

        /// <summary>
        /// The collection of the roles the user is assigned to.
        /// </summary>
        public ICollection<UserRole> Roles { get; } = new List<UserRole>();

        /// <summary>
        /// The collection of the password reset tokens issued for the user.
        /// </summary>
        public ICollection<PasswordResetToken> PasswordResetTokens { get; } = new List<PasswordResetToken>();

        /// <summary>
        /// The collection of the email address confirmation tokens issued for the user.
        /// </summary>
        public ICollection<EmailAddressConfirmationToken> EmailAddressConfirmationTokens { get; } = new List<EmailAddressConfirmationToken>();

        /// <summary>
        /// The collection of the articles the user has written.
        /// </summary>
        public ICollection<Article.Article> Articles { get; } = new List<Article.Article>();
    }
}
