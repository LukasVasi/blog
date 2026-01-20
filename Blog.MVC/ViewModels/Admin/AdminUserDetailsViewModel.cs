using Blog.Domain.Enums;

namespace Blog.MVC.ViewModels.Admin
{
    public class AdminUserDetailsViewModel
    {
        public required Guid Id { get; init; }

        public required string Username { get; init; }

        public required string? EmailAddress { get; init; }

        public required IReadOnlyCollection<UserRoleEnum> UserRoles { get; init; }
    }
}