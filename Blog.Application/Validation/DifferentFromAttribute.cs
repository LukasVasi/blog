using System.ComponentModel.DataAnnotations;

namespace Blog.Application.Validation
{
    /// <summary>
    /// Specifies that a data field must have a different value than another one.
    /// </summary>
    public class DifferentFromAttribute : ValidationAttribute
    {
        /// <summary>
        /// The other property that the property with this
        /// attribute needs to be different from.
        /// </summary>
        public string OtherProperty { get; }

        /// <summary>
        /// Flag which detemines if the check is made with
        /// normalized proeprty values.
        /// </summary>
        public bool CheckNormalized { get; set; } = true;

        public DifferentFromAttribute(string otherProperty)
        {
            OtherProperty = otherProperty;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var otherProperty = validationContext.ObjectType.GetProperty(OtherProperty);
            if (otherProperty == null)
            {
                return new ValidationResult($"Property '{OtherProperty}' was not found on type '{validationContext.ObjectType.Name}'.");
            }

            var otherValue = otherProperty.GetValue(validationContext.ObjectInstance);


            if (value == null || otherValue == null) {
                return ValidationResult.Success;
            }

            if (value is string stringValue && otherValue is string otherStringValue)
            {
                var equal = CheckNormalized
                    ? string.Equals(stringValue.ToLowerInvariant(), otherStringValue.ToLowerInvariant())
                    : string.Equals(stringValue, otherStringValue);

                return equal
                    ? new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} must be different from {OtherProperty}.")
                    : ValidationResult.Success;
            }

            return Equals(value, otherValue)
                ? new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} must be different from {OtherProperty}.")
                : ValidationResult.Success;
        }
    }
}
