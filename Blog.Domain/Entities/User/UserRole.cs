using Blog.Domain.Enums;

namespace Blog.Domain.Entities.User
{
    public class UserRole
    {
        /// <summary>
        /// The user role Id that is expressed via a string value 
        /// that corresponds to a <see cref="UserRoleEnum"/> value.
        /// </summary>
        public required UserRoleEnum Id { get; init; }

        /// <summary>
        /// The users that belong to the specific role.
        /// </summary>
        public ICollection<User> Users { get; } = new List<User>();
    }
}
