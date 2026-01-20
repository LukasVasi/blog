namespace Blog.Domain.Validation.User
{
    public class EmailAddressSpecifications
    {
        public const int MAX_LENGTH = 128;

        public const string REQUIRED_ERROR_MESSAGE = "Email address must be provided.";
        public const string EMAIL_ADDRESS_ERROR_MESSAGE = "Email address must be a valid email address.";
        public const string MAX_LENGTH_ERROR_MESSAGE = "Email address cannot be longer than 128 characters.";
        public const string AVAILABILITY_ERROR_MESSAGE = "Email address is already in use.";
        public const string NEW_DIFFERENT_FROM_CURRENT_ERROR_MESSAGE = "The new email address must be different to the current one.";
    }
}
