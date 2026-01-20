using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class ArticleCommentReportConfiguration : IEntityTypeConfiguration<ArticleCommentReport>
    {
        public void Configure(EntityTypeBuilder<ArticleCommentReport> builder)
        {
            builder.ToTable("ArticleCommentReports");

            builder.HasKey(report => report.Id);

            builder.Property(role => role.Reason)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(role => role.Description)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(report => report.CreatedAt)
                .IsRequired();

            // Cannot cascade as this would create a cascade cycle with comment
            builder.HasOne<User>(report => report.Reporter)
                .WithMany()
                .HasForeignKey(report => report.ReporterId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<ArticleComment>(report => report.Comment)
                .WithMany(comment => comment.Reports)
                .HasForeignKey(report => report.CommentId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // A user can report a comment only once
            builder.HasIndex(report => new { report.CommentId, report.ReporterId })
                .IsUnique();
        }
    }
}
