using Blog.Domain.Enums;

namespace Blog.MVC.ViewModels.Admin
{
    public class UpdateUserRolesViewModel
    {
        public required Guid Id { get; init; }

        public required string Username { get; init; }

        public required List<UserRoleEnum> AssignedUserRoles { get; set; } = new();

        public required List<UserRoleEnum> SelectedUserRoles { get; set; } = new();
    }
}
