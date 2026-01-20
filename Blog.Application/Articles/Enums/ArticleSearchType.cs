namespace Blog.Application.Articles.Enums
{
    public enum ArticleSearchType
    {
        All,    // Search in all fields
        Title,  // Search only in article title
        Text,   // Search only in article text
        Author  // Search only in the author username
    }
}
