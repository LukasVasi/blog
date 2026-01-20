using Blog.Domain.Enums;

namespace Blog.Application.Users.Requests
{
    public record UpdateUserRolesRequest
    {
        public required Guid Id { get; init; }
        
        public required IReadOnlyCollection<UserRoleEnum> UserRoles { get; init; }
    }
}
