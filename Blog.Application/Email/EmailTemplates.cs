namespace Blog.Application.Email
{
    public static class EmailTemplates
    {
        /// <summary>
        /// The relative path to the email address confirmation email plain text template.
        /// The path is relative to the EmailTemplates directory loaded into the host environment.
        /// </summary>
        public const string EMAIL_ADDRESS_CONFIRMATION_PLAIN_TEXT_TEMPLATE_PATH = "EmailAddressConfirmation/EmailAddressConfirmation.txt";

        /// <summary>
        /// The relative path to the email address confirmation email html template.
        /// The path is relative to the EmailTemplates directory loaded into the host environment.
        /// </summary>
        public const string EMAIL_ADDRESS_CONFIRMATION_HTML_TEMPLATE_PATH = "EmailAddressConfirmation/EmailAddressConfirmation.html";

        /// <summary>
        /// The relative path to the password reset email plain text template.
        /// The path is relative to the EmailTemplates directory loaded into the host environment.
        /// </summary>
        public const string PASSWORD_RESET_PLAIN_TEXT_TEMPLATE_PATH = "PasswordReset/PasswordReset.txt";

        /// <summary>
        /// The relative path to the password reset confirmation email html template.
        /// The path is relative to the EmailTemplates directory loaded into the host environment.
        /// </summary>
        public const string PASSWORD_RESET_HTML_TEMPLATE_PATH = "PasswordReset/PasswordReset.html";
    }
}
