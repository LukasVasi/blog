namespace Blog.Domain.Validation.User
{
    public class ConfirmPasswordSpecifications
    {
        public const string REQUIRED_ERROR_MESSAGE = "Password confirmation must be provided.";
        public const string COMPARISON_ERROR_MESSAGE = "Password confirmation must match password.";
    }
}
