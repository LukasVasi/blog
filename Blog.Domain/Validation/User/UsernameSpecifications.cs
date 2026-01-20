namespace Blog.Domain.Validation.User
{
    public class UsernameSpecifications
    {
        public const int MIN_LENGTH = 3;
        public const int MAX_LENGTH = 32;
        public const string COMPLEXITY_REGEX = @"^[a-zA-Z0-9_]+$";

        public const string REQUIRED_ERROR_MESSAGE = "Username must be provided.";
        public const string MIN_LENGTH_ERROR_MESSAGE = "Username must be at least 3 characters long.";
        public const string MAX_LENGTH_ERROR_MESSAGE = "Username cannot be longer than 32 characters.";
        public const string COMPLEXITY_ERROR_MESSAGE = "Username must only contain letters, numbers and underscores (_).";
        public const string AVAILABILITY_ERROR_MESSAGE = "Username is already in use.";
        public const string NEW_DIFFERENT_FROM_CURRENT_ERROR_MESSAGE = "The new username must be different to the current one.";
    }
}
