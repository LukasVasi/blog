using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class ArticleCommentConfiguration : IEntityTypeConfiguration<ArticleComment>
    {
        public void Configure(EntityTypeBuilder<ArticleComment> builder)
        {
            builder.ToTable("ArticleComments");

            builder.HasKey(articleComment => articleComment.Id);

            builder.Property(articleComment => articleComment.Text)
                .IsRequired();

            builder.Property(articleComment => articleComment.CreatedAt)
                .IsRequired();

            builder.Property(articleComment => articleComment.IsHidden)
                .IsRequired();

            // Cannot cascade as this would create a cascade cycle with article
            builder.HasOne<User>(articleComment => articleComment.User)
                .WithMany()
                .HasForeignKey(articleComment => articleComment.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Article>(articleComment => articleComment.Article)
                .WithMany(article => article.Comments)
                .HasForeignKey(articleComment => articleComment.ArticleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
