using Blog.Application.Users.Dtos;
using Blog.Domain.Enums;

namespace Blog.MVC.ViewModels.Admin
{
    public class AdminUserListViewModel
    {
        public string? Username { get; set; }
        public string? EmailAddress { get; set; }

        public bool? HasConfirmedEmailAddress { get; set; }

        public List<UserRoleEnum> SelectedUserRoles { get; set; } = new();

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public required IReadOnlyCollection<UserDto> Users { get; set; }
    }
}
