using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder.ToTable("Articles");

            builder.HasKey(article => article.Id);

            builder.Property(article => article.Title)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(article => article.Text)
                .IsRequired();

            builder.Property(article => article.CreatedAt)
                .IsRequired();

            builder.HasOne<User>(article => article.Author)
                .WithMany(user => user.Articles)
                .HasForeignKey(article => article.AuthorId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(article => article.Title);

            builder.HasIndex(article => article.CreatedAt);
        }
    }
}
