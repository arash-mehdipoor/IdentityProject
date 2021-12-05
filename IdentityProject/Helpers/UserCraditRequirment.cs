using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityProject.Helpers
{
    public class UserCraditRequirment : IAuthorizationRequirement
    {
        public int Cradit { get; set; }
        public UserCraditRequirment(int cradit)
        {
            Cradit = cradit;
        }
    }

    public class UserCraditRequirmentHander : AuthorizationHandler<UserCraditRequirment>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserCraditRequirment requirement)
        {
            var claim = context.User.FindFirst("Cradit");
            if (claim != null)
            {
                var cradit = int.Parse(claim?.Value);
                if (cradit >= requirement.Cradit)
                {
                    context.Succeed(requirement);
                }
            }
            return Task.CompletedTask;
        }
    }

}
