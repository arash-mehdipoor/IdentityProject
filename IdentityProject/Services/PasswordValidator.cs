using IdentityProject.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityProject.Services
{
    public class PasswordValidator : IPasswordValidator<User>
    {
        List<string> _blackListPassword = new List<string>()
        {
            "123456",
            "abcdef",
            "123457"
        };
        public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user, string password)
        {
            if (_blackListPassword.Contains(password))
            {
                var result = IdentityResult.Failed(new IdentityError()
                {
                    Code = "blackListPassword",
                    Description = "لطفا کلمه عبور قوی تری انتخاب نمایید"
                });
                return Task.FromResult(result);
            }
            return Task.FromResult(IdentityResult.Success);
        }
    }
}
