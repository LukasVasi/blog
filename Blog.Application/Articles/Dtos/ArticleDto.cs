using Blog.Application.Users.Dtos;
using Blog.Domain.Entities.Article;

namespace Blog.Application.Articles.Dtos
{
    public record ArticleDto
    {
        public required Guid Id { get; init; }

        public required string Title { get; init; }

        public required string Text { get; init; }

        public required DateTime CreatedAt { get; init; }

        public required UserDto Author { get; init; }

        public ArticleImageDto? Image { get; init; }

        public required int Rating { get; init; }

        public ArticleRatingValue? AuthenticatedUserRating { get; init; }

        public required ICollection<ArticleCommentDto>? Comments { get; init; }
    }
}
