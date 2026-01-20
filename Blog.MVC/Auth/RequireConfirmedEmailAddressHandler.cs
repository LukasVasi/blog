using Blog.Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Blog.MVC.Auth
{
    public class RequireConfirmedEmailAddressHandler : AuthorizationHandler<ConfirmedEmailAddressRequirement>
    {
        private readonly AuthOptions _authOptions;

        public RequireConfirmedEmailAddressHandler(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ConfirmedEmailAddressRequirement requirement)
        {
            if (!_authOptions.EmailAddressConfirmationRequired)
            {
                context.Succeed(requirement);
            }
            else
            {
                var hasEmailAddressClaim = context.User.HasClaim(c =>
                c.Type == ClaimTypes.Email &&
                !string.IsNullOrEmpty(c.Value)
                );

                if (hasEmailAddressClaim)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
