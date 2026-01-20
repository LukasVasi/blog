namespace Blog.Domain.Validation.Article
{
    public static class TitleSpecifications
    {
        public const int MAX_LENGTH = 128;

        public const string REQUIRED_ERROR_MESSAGE = "Title must be provided.";
        public const string MAX_LENGTH_ERROR_MESSAGE = "Title cannot be longer than 128 characters.";
    }
}
