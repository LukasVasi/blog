using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class ArticleImageConfiguration : IEntityTypeConfiguration<ArticleImage>
    {
        public void Configure(EntityTypeBuilder<ArticleImage> builder)
        {
            builder.ToTable("ArticleImages");

            builder.HasKey(articleImage => articleImage.Id);

            builder.Property(articleImage => articleImage.FileName)
                .IsRequired();

            builder.Property(articleImage => articleImage.OriginalFileName)
                .IsRequired();

            builder.Property(articleImage => articleImage.CreatedAt)
                .IsRequired();

            builder.HasOne<User>(articleImage => articleImage.User)
                .WithMany()
                .HasForeignKey(articleImage => articleImage.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Article>(articleImage => articleImage.Article)
                .WithOne(article => article.Image)
                .HasForeignKey<ArticleImage>(articleImage => articleImage.ArticleId)
                .IsRequired(false);
        }
    }
}
