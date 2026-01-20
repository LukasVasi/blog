using System.Security.Claims;

namespace Blog.Infrastructure.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetAuthenticatedUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new InvalidOperationException("Authenticated user does not contain a user identifier claim.");
            }

            if (!Guid.TryParse(userIdClaim.Value, out var userId))
            {
                throw new InvalidOperationException("User identifier claim is not a valid GUID.");
            }

            return userId;
        }
    }
}
