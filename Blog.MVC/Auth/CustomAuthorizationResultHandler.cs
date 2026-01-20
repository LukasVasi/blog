using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Blog.MVC.Auth
{
    public class CustomAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (!authorizeResult.Succeeded)
            {
                if (authorizeResult.Forbidden &&
                    policy.Requirements.Any(r => r is ConfirmedEmailAddressRequirement))
                {
                    context.Response.Redirect("/account/send-email-address-confirmation");
                    return;
                }
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);

        }
    }
}
