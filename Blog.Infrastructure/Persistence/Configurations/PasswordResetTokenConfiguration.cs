using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
    {
        public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
        {
            builder.ToTable("PasswordResetTokens");

            builder.HasKey(passwordResetToken => passwordResetToken.Id);

            builder.Property(passwordResetToken => passwordResetToken.TokenHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(passwordResetToken => passwordResetToken.CreatedAt)
                .IsRequired();

            builder.Property(passwordResetToken => passwordResetToken.ExpiresAt)
                .IsRequired();

            builder.Property(passwordResetToken => passwordResetToken.RevokedReason)
                .HasMaxLength(128);

            builder.HasOne<User>(passwordResetToken => passwordResetToken.User)
                .WithMany(user => user.PasswordResetTokens)
                .HasForeignKey(passwordResetToken => passwordResetToken.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
