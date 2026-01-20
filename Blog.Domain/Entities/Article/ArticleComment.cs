namespace Blog.Domain.Entities.Article
{
    public class ArticleComment
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required string Text { get; set; }

        public DateTime CreatedAt { get; init; } = DateTime.Now;

        public bool IsHidden { get; set; } = false;

        public required Guid ArticleId { get; init; }
        public Article Article { get; init; } = null!;

        public required Guid UserId { get; init; }
        public User.User User { get; init; } = null!;

        public ICollection<ArticleCommentReport> Reports { get; } = new List<ArticleCommentReport>();
    }
}
