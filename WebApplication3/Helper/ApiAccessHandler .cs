using Microsoft.AspNetCore.Authorization;
using WebApplication3.Helper.Data;

namespace WebApplication3.Helper
{
    public class ApiAccessHandler : AuthorizationHandler<ApiAccessRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiAccessHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiAccessRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Fail(); // User is not authenticated, fail the requirement
                return Task.CompletedTask;
            }

            // Check if the user has the necessary claim (or other authorization logic)
            if (!context.User.HasClaim(c => c.Type == "ApiAccess" && c.Value == requirement.ApiName))
            {
                context.Fail(); // User does not have the required claim, fail the requirement
                return Task.CompletedTask;
            }

            // User meets the requirement
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
