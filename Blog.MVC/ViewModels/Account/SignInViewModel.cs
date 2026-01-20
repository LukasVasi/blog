using Blog.Domain.Validation.User;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Account
{
    public class SignInViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = UsernameSpecifications.REQUIRED_ERROR_MESSAGE)]
        public string Username { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = PasswordSpecifications.REQUIRED_ERROR_MESSAGE)]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
