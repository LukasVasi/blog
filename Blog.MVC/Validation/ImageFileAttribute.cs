using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Validation
{
    public class ImageFileAttribute : ValidationAttribute
    {
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".png",
                ".jpg",
                ".jpeg",
                ".gif",
                ".webp",
                ".bmp"
            };

        private static readonly HashSet<string> AllowedMimeTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "image/png",
                "image/jpeg",
                "image/gif",
                "image/webp",
                "image/bmp"
            };

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }

            if (value is not IFormFile file || file.Length == 0)
            {
                return ValidationResult.Success;
            }

            var extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedExtensions.Contains(extension))
            {
                return new ValidationResult(
                    ErrorMessage ??
                    "The provided file must be a valid .png, .jpg, .jpeg, .gif, .webp or .bmp image file.");
            }

            if (string.IsNullOrWhiteSpace(file.ContentType) ||
                !AllowedMimeTypes.Contains(file.ContentType))
            {
                return new ValidationResult(
                    ErrorMessage ??
                    "The provided file must be a valid .png, .jpg, .jpeg, .gif, .webp or .bmp image file.");
            }

            return ValidationResult.Success;
        }
    }
}
