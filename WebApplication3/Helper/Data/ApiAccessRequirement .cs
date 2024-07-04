using Microsoft.AspNetCore.Authorization;

namespace WebApplication3.Helper.Data
{
    public class ApiAccessRequirement : IAuthorizationRequirement
    {
        public string ApiName { get; }

        public ApiAccessRequirement(string apiName)
        {
            ApiName = apiName;
        }
    }
}
