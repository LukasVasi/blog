using Blog.Domain.Validation.User;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Account
{
    public class ForgotPasswordViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = EmailAddressSpecifications.REQUIRED_ERROR_MESSAGE)]
        [EmailAddress(ErrorMessage = EmailAddressSpecifications.EMAIL_ADDRESS_ERROR_MESSAGE)]
        public string EmailAddress { get; set; } = string.Empty;
    }
}
