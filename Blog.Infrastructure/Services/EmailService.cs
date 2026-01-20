using Blog.Application.Email;
using Blog.Application.Email.Requests;
using Blog.Application.Interfaces;
using Blog.Infrastructure.Options;
using FluentResults;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Blog.Infrastructure.Services
{
    internal class EmailService : IEmailService
    {
        private readonly SmtpOptions _smtpOptions;

        public EmailService(IOptions<SmtpOptions> smtpOptions)
        {
            _smtpOptions = smtpOptions.Value;
        }

        public async Task<Result> SendEmailAddressConfirmationAsync(SendEmailAddressConfirmationRequest sendEmailAddressConfirmationRequest)
        {
            var plainTextBody = await LoadEmailTemplateAsync(EmailTemplates.EMAIL_ADDRESS_CONFIRMATION_PLAIN_TEXT_TEMPLATE_PATH);
            plainTextBody = plainTextBody.Replace("{{EmailAddressConfirmationUrl}}", sendEmailAddressConfirmationRequest.ConfirmEmailAddressUrl);

            var htmlBody = await LoadEmailTemplateAsync(EmailTemplates.EMAIL_ADDRESS_CONFIRMATION_HTML_TEMPLATE_PATH);
            htmlBody = htmlBody.Replace("{{EmailAddressConfirmationUrl}}", sendEmailAddressConfirmationRequest.ConfirmEmailAddressUrl);

            var sendEmailRequest = new SendEmailRequest
            {
                From = "noreply@blog.com",
                To = sendEmailAddressConfirmationRequest.EmailAddress,
                Subject = "Blog email address confirmation",
                PlainTextBody = plainTextBody,
                HtmlBody = htmlBody
            };

            return await SendEmailAsync(sendEmailRequest);
        }

        public async Task<Result> SendPasswordResetAsync(SendPasswordResetRequest sendPasswordResetRequest)
        {
            var plainTextBody = await LoadEmailTemplateAsync(EmailTemplates.PASSWORD_RESET_PLAIN_TEXT_TEMPLATE_PATH);
            plainTextBody = plainTextBody.Replace("{{PasswordResetUrl}}", sendPasswordResetRequest.ResetPasswordUrl);

            var htmlBody = await LoadEmailTemplateAsync(EmailTemplates.PASSWORD_RESET_HTML_TEMPLATE_PATH);
            htmlBody = htmlBody.Replace("{{PasswordResetUrl}}", sendPasswordResetRequest.ResetPasswordUrl);

            var sendEmailRequest = new SendEmailRequest
            {
                From = "noreply@blog.com",
                To = sendPasswordResetRequest.EmailAddress,
                Subject = "Blog password reset",
                PlainTextBody = plainTextBody,
                HtmlBody = htmlBody
            };

            return await SendEmailAsync(sendEmailRequest);
        }

        public async Task<Result> SendEmailAsync(SendEmailRequest sendEmailRequest)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Blog", sendEmailRequest.From));
            message.To.Add(new MailboxAddress(sendEmailRequest.To, sendEmailRequest.To));
            message.Subject = sendEmailRequest.Subject;

            var builder = new BodyBuilder();

            builder.TextBody = sendEmailRequest.PlainTextBody;
            builder.HtmlBody = sendEmailRequest.HtmlBody;

            message.Body = builder.ToMessageBody();

            try
            {
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port, SecureSocketOptions.None);
                    await client.AuthenticateAsync(_smtpOptions.User, _smtpOptions.Password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
                return Result.Ok();
            } 
            catch
            {
                return Result.Fail("Failed to send email. Please try again later.");
            }
        }

        private async Task<string> LoadEmailTemplateAsync(string templatePath)
        {
            var fullPath = Path.Combine(
                AppContext.BaseDirectory,
                "Email",
                "Templates",
                templatePath.Replace('/', Path.DirectorySeparatorChar)
                );

            if (File.Exists(fullPath))
            {
                return await File.ReadAllTextAsync(fullPath).ConfigureAwait(false);
            }
            else
            {
                throw new FileNotFoundException($"Email template not found: {fullPath}");
            }
        }
    }
}
