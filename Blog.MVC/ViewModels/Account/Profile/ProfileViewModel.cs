using Blog.Domain.Enums;

namespace Blog.MVC.ViewModels.Account.Profile
{
    public class ProfileViewModel
    {
        public required Guid Id { get; init; }

        public required string Username { get; init; }

        public required string? EmailAddress { get; init; }

        public required IReadOnlyCollection<UserRoleEnum> UserRoles { get; init; }
    }
}
