using IdentityProject.Models.ViewModels.Blog;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityProject.Helpers
{
    public class BlogRequireMent : IAuthorizationRequirement
    {
    }

    public class BlogRequireMentHandler : AuthorizationHandler<BlogRequireMent, BlogViewModel>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, BlogRequireMent requirement, BlogViewModel resource)
        {
            if (context.User.Identity.Name == resource.UserName)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
