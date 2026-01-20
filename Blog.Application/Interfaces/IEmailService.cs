using Blog.Application.Email.Requests;
using FluentResults;

namespace Blog.Application.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Generic method for sending emails.
        /// </summary>
        /// <param name="sendEmailRequest">
        /// The request to send an email.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful.
        /// </returns>
        public Task<Result> SendEmailAsync(SendEmailRequest sendEmailRequest);

        /// <summary>
        /// Sends an email address confirmation email.
        /// </summary>
        /// <param name="sendEmailAddressConfirmationRequest">
        /// The request to send an email containing an email address confirmation.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful.
        /// </returns>
        public Task<Result> SendEmailAddressConfirmationAsync(SendEmailAddressConfirmationRequest sendEmailAddressConfirmationRequest);

        /// <summary>
        /// Sends a password reset email.
        /// </summary>
        /// <param name="sendPasswordResetRequest">
        /// The request to send an email containing the password reset.
        /// </param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the operation was successful.
        /// </returns>
        public Task<Result> SendPasswordResetAsync(SendPasswordResetRequest sendPasswordResetRequest);
    }
}
