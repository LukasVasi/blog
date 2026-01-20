namespace Blog.Domain.Entities.Article
{
    public class ArticleRating
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required ArticleRatingValue Value { get; set; }

        public required Guid ArticleId { get; init; }
        public Article Article { get; init; } = null!;

        public required Guid UserId { get; init; }
        public User.User User { get; init; } = null!;
    }

    public enum ArticleRatingValue
    {
        Positive = 1,
        Negative = -1
    }
}
