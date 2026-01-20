namespace Blog.Application.Email.Requests
{
    public record SendEmailAddressConfirmationRequest
    {
        public required string EmailAddress { get; set; }

        public required string ConfirmEmailAddressUrl { get; set; }
    }
}
