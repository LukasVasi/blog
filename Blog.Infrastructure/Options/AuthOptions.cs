namespace Blog.Infrastructure.Options
{
    public class AuthOptions
    {
        /// <summary>
        /// Determines if email confirmation is required for certain actions.
        /// </summary>
        public bool EmailAddressConfirmationRequired { get; set; } = false;

        /// <summary>
        /// The number of days before an email address confirmation token expires.
        /// </summary>
        public int EmailAddressConfirmationTokenTTLDays { get; set; } = 3;

        /// <summary>
        /// The number of days before a password reset token expires.
        /// </summary>
        public int PasswordResetTokenTTLDays { get; set; } = 1;
    }
}
