using Blog.Domain.Validation.User;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Account.Profile
{
    public class UpdatePasswordViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = PasswordSpecifications.REQUIRED_ERROR_MESSAGE)]
        [MinLength(PasswordSpecifications.MIN_LENGTH, ErrorMessage = PasswordSpecifications.MIN_LENGTH_ERROR_MESSAGE)]
        [MaxLength(PasswordSpecifications.MAX_LENGTH, ErrorMessage = PasswordSpecifications.MAX_LENGTH_ERROR_MESSAGE)]
        [RegularExpression(
            PasswordSpecifications.COMPLEXITY_REGEX,
            ErrorMessage = PasswordSpecifications.COMPLEXITY_ERROR_MESSAGE
        )]
        public string NewPassword { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = ConfirmPasswordSpecifications.REQUIRED_ERROR_MESSAGE)]
        [Compare(nameof(NewPassword), ErrorMessage = ConfirmPasswordSpecifications.COMPARISON_ERROR_MESSAGE)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
