using Blog.Domain.Validation.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Account
{
    public class SignUpViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = EmailAddressSpecifications.REQUIRED_ERROR_MESSAGE)]
        [EmailAddress(ErrorMessage = EmailAddressSpecifications.EMAIL_ADDRESS_ERROR_MESSAGE)]
        [MaxLength(EmailAddressSpecifications.MAX_LENGTH, ErrorMessage = EmailAddressSpecifications.MAX_LENGTH_ERROR_MESSAGE)]
        [Remote("CheckEmailAddressAvailability", "Account", ErrorMessage = EmailAddressSpecifications.AVAILABILITY_ERROR_MESSAGE)]
        public string EmailAddress { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = UsernameSpecifications.REQUIRED_ERROR_MESSAGE)]
        [MaxLength(UsernameSpecifications.MAX_LENGTH, ErrorMessage = UsernameSpecifications.MAX_LENGTH_ERROR_MESSAGE)]
        [RegularExpression(
            UsernameSpecifications.COMPLEXITY_REGEX, 
            ErrorMessage = UsernameSpecifications.COMPLEXITY_ERROR_MESSAGE
        )]
        [Remote("CheckUsernameAvailability", "Account", ErrorMessage = UsernameSpecifications.AVAILABILITY_ERROR_MESSAGE)]
        public string Username { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = PasswordSpecifications.REQUIRED_ERROR_MESSAGE)]
        [MinLength(PasswordSpecifications.MIN_LENGTH, ErrorMessage = PasswordSpecifications.MIN_LENGTH_ERROR_MESSAGE)]
        [MaxLength(PasswordSpecifications.MAX_LENGTH, ErrorMessage = PasswordSpecifications.MAX_LENGTH_ERROR_MESSAGE)]
        [RegularExpression(
            PasswordSpecifications.COMPLEXITY_REGEX,
            ErrorMessage = PasswordSpecifications.COMPLEXITY_ERROR_MESSAGE
        )]
        public string Password { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = ConfirmPasswordSpecifications.REQUIRED_ERROR_MESSAGE)]
        [Compare(nameof(Password), ErrorMessage = ConfirmPasswordSpecifications.COMPARISON_ERROR_MESSAGE)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
