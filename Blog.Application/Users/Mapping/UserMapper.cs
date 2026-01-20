using Blog.Application.Users.Dtos;
using Blog.Domain.Entities.User;

namespace Blog.Application.Users.Mapping
{
    public static class UserMapper
    {
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                EmailAddress = user.EmailAddress,
                Username = user.Username,
                Roles = user.Roles.Select(role => role.Id).ToList()
            };
        }
    }
}
