using IdentityProject.Areas.Admin.Models.ViewModels;
using IdentityProject.Models.Entities;
using IdentityProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly EmailServices _emailServices;

        public UsersController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            EmailServices emailServices)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailServices = emailServices;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.Select(u => new UsersListViewModel()
            {
                Id = u.Id,
                UserName = u.UserName,
                FirstName = u.FirstName,
                LastName = u.LastName,
                PhoneNumber = u.PhoneNumber,
                AccessFailedCount = u.AccessFailedCount,
                EmailConfirmed = u.EmailConfirmed
            }).ToList();

            return View(users);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateUser(AdminRegisterUserViewModel register)
        {
            if (ModelState.IsValid == false)
            {
                return View(register);
            }

            User newUser = new User()
            {
                FirstName = register.FirstName,
                LastName = register.LastName,
                Email = register.Email,
                UserName = register.Email,
            };

            var result = await _userManager.CreateAsync(newUser, register.Password);
            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                string callBackUrl = Url.Action("ConfirmEmail", "Account",
                    new { UserId = newUser.Id, token = token }, protocol: Request.Scheme);

                string body = $"جهت فعال سازی حساب کاربری <a href='{callBackUrl}'>کلیک نمایید</a>";
                await _emailServices.Excute(newUser.Email, body, "فعال سازی حساب کاربری");

                return RedirectToAction("DisplayEmail");
            }

            string message = "";
            foreach (var item in result.Errors.ToList())
            {
                message += item.Description + Environment.NewLine;
            }
            TempData["Message"] = message;
            return View(register);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            UserEditViewModel viewModel = new UserEditViewModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserName = user.UserName
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserEditViewModel viewModel)
        {
            var user = await _userManager.FindByIdAsync(viewModel.Id);
            user.FirstName = viewModel.FirstName;
            user.LastName = viewModel.LastName;
            user.Email = viewModel.Email;
            user.PhoneNumber = viewModel.PhoneNumber;



            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Users", new { area = "Admin" });
            }
            string message = "";
            foreach (var item in result.Errors.ToList())
            {
                message += item.Description + Environment.NewLine;
            }
            TempData["Message"] = message;
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["DeleteUser"] = user.FirstName + " " + user.LastName + "حذف شد";
            }
            return RedirectToAction("Index", "Users", new { area = "Admin" });
        }

        [HttpGet]
        public async Task<IActionResult> AddUserRole(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            var roles = _roleManager.Roles.Select(r => new SelectListItem()
            {
                Text = r.Name,
                Value = r.Name
            }).ToList();

            var result = new List<SelectListItem>(roles);

            return View(new AddUserRoleViewModel()
            {
                Id = Id,
                Roles = result,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddUserRole(AddUserRoleViewModel viewModel)
        {
            var user = await _userManager.FindByIdAsync(viewModel.Id);
            var result = await _userManager.AddToRoleAsync(user, viewModel.Role);
            if (result.Succeeded)
            {
                return RedirectToAction("UserRoles", "Users", new { Id = user.Id, area = "Admin" });
            }
            return View(viewModel);
        }

        public async Task<IActionResult> UserRoles(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.UserInfo = $"{user.FirstName} {user.LastName}";
            return View(userRoles);
        }
        [HttpGet]
        public IActionResult CreateClaims()
        { 
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateClaims(string claimType, string claimValue)
        {
            var user = await _userManager.GetUserAsync(User);

            Claim newClaim = new Claim(claimType, claimValue, ClaimValueTypes.String);
             
            var result = await _userManager.AddClaimAsync(user, newClaim);

            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }
            return View();
        }
    }
}
