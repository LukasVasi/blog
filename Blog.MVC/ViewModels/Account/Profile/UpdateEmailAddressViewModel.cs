using Blog.Application.Validation;
using Blog.Domain.Validation.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Account.Profile
{
    public class UpdateEmailAddressViewModel
    {
        public required string CurrentEmailAddress { get; init; }

        [Required(AllowEmptyStrings = false, ErrorMessage = EmailAddressSpecifications.REQUIRED_ERROR_MESSAGE)]
        [EmailAddress(ErrorMessage = EmailAddressSpecifications.EMAIL_ADDRESS_ERROR_MESSAGE)]
        [MaxLength(EmailAddressSpecifications.MAX_LENGTH, ErrorMessage = EmailAddressSpecifications.MAX_LENGTH_ERROR_MESSAGE)]
        [DifferentFrom(nameof(CurrentEmailAddress), ErrorMessage = EmailAddressSpecifications.NEW_DIFFERENT_FROM_CURRENT_ERROR_MESSAGE)]
        [Remote("CheckEmailAddressAvailability", "Account", ErrorMessage = EmailAddressSpecifications.AVAILABILITY_ERROR_MESSAGE)]
        public string NewEmailAddress { get; set; } = string.Empty;
    }
}
