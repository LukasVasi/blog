using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class EmailAddressConfirmationTokenConfiguration : IEntityTypeConfiguration<EmailAddressConfirmationToken>
    {
        public void Configure(EntityTypeBuilder<EmailAddressConfirmationToken> builder)
        {
            builder.ToTable("EmailAddressConfirmationTokens");

            builder.HasKey(emailAddressConfirmationToken => emailAddressConfirmationToken.Id);

            builder.Property(emailAddressConfirmationToken => emailAddressConfirmationToken.EmailAddress)
                .HasMaxLength(128);

            builder.Property(emailAddressConfirmationToken => emailAddressConfirmationToken.TokenHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(emailAddressConfirmationToken => emailAddressConfirmationToken.CreatedAt)
                .IsRequired();

            builder.Property(emailAddressConfirmationToken => emailAddressConfirmationToken.ExpiresAt)
                .IsRequired();

            builder.Property(emailAddressConfirmationToken => emailAddressConfirmationToken.RevokedReason)
                .HasMaxLength(128);

            builder.HasOne<User>(emailAddressConfirmationToken => emailAddressConfirmationToken.User)
                .WithMany(user => user.EmailAddressConfirmationTokens)
                .HasForeignKey(emailAddressConfirmationToken => emailAddressConfirmationToken.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
