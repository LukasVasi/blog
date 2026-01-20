namespace Blog.Domain.Entities.User
{
    public class EmailAddressConfirmationToken
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// The email address the token is confirming.
        /// </summary>
        public required string EmailAddress { get; init; }

        /// <summary>
        /// The hash of the token sent to the user.
        /// </summary>
        public required string TokenHash { get; init; }

        public required DateTime CreatedAt { get; init; }
        public required DateTime ExpiresAt { get; init; }

        public string? RevokedReason { get; set; }
        public DateTime? RevokedAt { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public required Guid UserId { get; init; }
        public User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsRevoked => RevokedAt != null;

        public bool IsConfirmed => ConfirmedAt != null;

        public bool IsValid => !IsExpired && !IsRevoked && !IsConfirmed;
    }
}
