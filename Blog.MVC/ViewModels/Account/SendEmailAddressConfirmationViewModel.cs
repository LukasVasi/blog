using Blog.Domain.Validation.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Account
{
    public class SendEmailAddressConfirmationViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = EmailAddressSpecifications.REQUIRED_ERROR_MESSAGE)]
        [EmailAddress(ErrorMessage = EmailAddressSpecifications.EMAIL_ADDRESS_ERROR_MESSAGE)]
        [MaxLength(EmailAddressSpecifications.MAX_LENGTH, ErrorMessage = EmailAddressSpecifications.MAX_LENGTH_ERROR_MESSAGE)]
        [Remote("CheckEmailAddressAvailability", "Account", ErrorMessage = EmailAddressSpecifications.AVAILABILITY_ERROR_MESSAGE)]
        public string EmailAddress { get; set; } = string.Empty;
    }
}
