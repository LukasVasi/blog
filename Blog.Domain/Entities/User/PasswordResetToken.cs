namespace Blog.Domain.Entities.User
{
    public class PasswordResetToken
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// The hash of the token sent to the user.
        /// </summary>
        public string TokenHash { get; init; }

        public DateTime CreatedAt { get; init; }
        public DateTime ExpiresAt { get; init; }

        public string? RevokedReason { get; set; }
        public DateTime? RevokedAt { get; set; }

        public required Guid UserId { get; init; }
        public User User { get; set; } = null!;

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        public bool IsRevoked => RevokedAt != null;

        public bool IsValid => !IsExpired && !IsRevoked;
    }
}
