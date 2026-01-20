using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(user => user.Id);

            builder.Property(user => user.EmailAddress)
                .HasMaxLength(128);

            builder.Property(user => user.NormalizedEmailAddress)
                .HasMaxLength(128);

            builder.Property(user => user.Username)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(user => user.NormalizedUsername)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(user => user.PasswordHash)
                .IsRequired()
                .HasMaxLength(256);

            builder.HasIndex(user => user.NormalizedUsername)
                .IsUnique();
        }
    }
}
