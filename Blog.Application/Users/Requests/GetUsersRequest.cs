using Blog.Domain.Enums;

namespace Blog.Application.Users.Requests
{
    public record GetUsersRequest
    {
        public string? Username { get; init; }

        public string? EmailAddress { get; init; }

        public bool? HasConfirmedEmailAddress { get; init; }

        public IReadOnlyCollection<UserRoleEnum>? UserRoleIds { get; init; }

        public required int Page { get; init; }

        public required int PageSize { get; init; }
    }
}
