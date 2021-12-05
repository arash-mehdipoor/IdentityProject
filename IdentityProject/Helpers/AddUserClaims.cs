using IdentityProject.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityProject.Helpers
{
    public class AddUserClaims : UserClaimsPrincipalFactory<User>
    {
        public AddUserClaims(UserManager<User> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
        {
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("FullName", $"{user.FirstName} {user.LastName}", ClaimValueTypes.String));
            identity.AddClaim(new Claim("Cradit","11000", ClaimValueTypes.String));
            return identity;
        }
    }
}
