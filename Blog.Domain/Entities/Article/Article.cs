namespace Blog.Domain.Entities.Article
{
    public class Article
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required string Title { get; set; }

        public required string Text { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.Now;

        public required Guid AuthorId { get; init; }

        public User.User Author { get; set; } = null!;

        public ArticleImage? Image { get; set; }

        /// <summary>
        /// The ratings of this article.
        /// </summary>
        public ICollection<ArticleRating> Ratings { get; } = new List<ArticleRating>();

        /// <summary>
        /// The comments of this article.
        /// </summary>
        public ICollection<ArticleComment> Comments { get; } = new List<ArticleComment>();
    }
}
