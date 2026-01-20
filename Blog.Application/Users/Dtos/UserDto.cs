using Blog.Domain.Enums;

namespace Blog.Application.Users.Dtos
{
    public record UserDto
    {
        public required Guid Id { get; init; }

        public required string? EmailAddress { get; init; }

        public required string Username { get; init; }

        public required IReadOnlyCollection<UserRoleEnum> Roles { get; init; }
    }
}
