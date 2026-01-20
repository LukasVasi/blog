using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class ArticleRatingConfiguration : IEntityTypeConfiguration<ArticleRating>
    {
        public void Configure(EntityTypeBuilder<ArticleRating> builder)
        {
            builder.ToTable("ArticleRatings");

            builder.HasKey(articleRating => articleRating.Id);

            builder.Property(articleRating => articleRating.Value)
                .IsRequired();

            // Cannot cascade as this would create a cascade cycle with article
            builder.HasOne<User>(articleRating => articleRating.User)
                .WithMany()
                .HasForeignKey(articleRating => articleRating.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Article>(articleRating => articleRating.Article)
                .WithMany(article => article.Ratings)
                .HasForeignKey(articleRating => articleRating.ArticleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // Ensures that there can be only one rating for each user-article pair 
            builder.HasIndex(articleRating => new { articleRating.ArticleId, articleRating.UserId })
                .IsUnique();
        }
    }
}
