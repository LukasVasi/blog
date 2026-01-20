namespace Blog.Domain.Validation.User
{
    public class PasswordSpecifications
    {
        public const int MIN_LENGTH = 6;
        public const int MAX_LENGTH = 64;
        public const string COMPLEXITY_REGEX = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$";

        public const string REQUIRED_ERROR_MESSAGE = "Password must be provided.";
        public const string MIN_LENGTH_ERROR_MESSAGE = "Password must be at least 6 characters long.";
        public const string MAX_LENGTH_ERROR_MESSAGE = "Password cannot be longer than 64 characters.";
        public const string COMPLEXITY_ERROR_MESSAGE = "Password must contain at least one of each: uppercase letter, lowercase letter, digit, and special character.";
    }
}
