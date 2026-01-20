using Blog.Domain.Entities.Article;
using Blog.Domain.Entities.User;
using Blog.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Blog.Infrastructure.Persistence.Context
{
    public class BlogDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
        public DbSet<EmailAddressConfirmationToken> EmailAddressConfirmationTokens => Set<EmailAddressConfirmationToken>();

        public DbSet<Article> Articles => Set<Article>();
        public DbSet<ArticleImage> ArticleImages => Set<ArticleImage>();
        public DbSet<ArticleRating> ArticleRatings => Set<ArticleRating>();
        public DbSet<ArticleComment> ArticleComments => Set<ArticleComment>();
        public DbSet<ArticleCommentReport> ArticleCommentReports => Set<ArticleCommentReport>();

        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration<User>(new UserConfiguration());
            modelBuilder.ApplyConfiguration<UserRole>(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration<PasswordResetToken>(new PasswordResetTokenConfiguration());
            modelBuilder.ApplyConfiguration<EmailAddressConfirmationToken>(new EmailAddressConfirmationTokenConfiguration());
            modelBuilder.ApplyConfiguration<Article>(new ArticleConfiguration());
            modelBuilder.ApplyConfiguration<ArticleImage>(new ArticleImageConfiguration());
            modelBuilder.ApplyConfiguration<ArticleRating>(new ArticleRatingConfiguration());
            modelBuilder.ApplyConfiguration<ArticleComment>(new ArticleCommentConfiguration());
            modelBuilder.ApplyConfiguration<ArticleCommentReport>(new ArticleCommentReportConfiguration());
        }
    }
}
