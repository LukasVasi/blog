using Blog.Domain.Entities.User;

namespace Blog.Domain.Entities.Article
{
    public class ArticleImage
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public required string FileName { get; init; }
        public required string OriginalFileName { get; init; }
        public DateTime CreatedAt { get; init; } = DateTime.Now;

        /// <summary>
        /// The Id of the user that has uploaded this article image.
        /// </summary>
        public required Guid UserId { get; init; }
        public User.User User { get; init; } = null!;

        /// <summary>
        /// The Id of the article this image belongs to.
        /// Not defined (article image is orphaned) until the
        /// article with this image specified is created.
        /// </summary>
        public Guid? ArticleId { get; set; }
        public Article? Article { get; set; }
    }
}
