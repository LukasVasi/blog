using Blog.Domain.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Blog.Infrastructure.Persistence.Configurations
{
    internal class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(role => role.Id);

            builder.Property(role => role.Id)
                .HasConversion<int>();

            builder.HasMany<User>(role => role.Users)
            .WithMany(user => user.Roles)
            .UsingEntity(ura => ura.ToTable("UserRoleAssignments"));
        }
    }
}
