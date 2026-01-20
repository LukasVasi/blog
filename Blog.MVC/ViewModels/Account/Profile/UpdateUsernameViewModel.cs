using Blog.Application.Validation;
using Blog.Domain.Validation.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Blog.MVC.ViewModels.Account.Profile
{
    public class UpdateUsernameViewModel
    {
        public required string CurrentUsername { get; init; }

        [Required(AllowEmptyStrings = false, ErrorMessage = UsernameSpecifications.REQUIRED_ERROR_MESSAGE)]
        [MaxLength(UsernameSpecifications.MAX_LENGTH, ErrorMessage = UsernameSpecifications.MAX_LENGTH_ERROR_MESSAGE)]
        [RegularExpression(
            UsernameSpecifications.COMPLEXITY_REGEX,
            ErrorMessage = UsernameSpecifications.COMPLEXITY_ERROR_MESSAGE
        )]
        [DifferentFrom(nameof(CurrentUsername), ErrorMessage = UsernameSpecifications.NEW_DIFFERENT_FROM_CURRENT_ERROR_MESSAGE)]
        [Remote("CheckUsernameAvailability", "Account", ErrorMessage = UsernameSpecifications.AVAILABILITY_ERROR_MESSAGE)]
        public string NewUsername { get; set; } = string.Empty;
    }
}
