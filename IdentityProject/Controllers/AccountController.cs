using Identity.Bugeto.Models.Entities;
using IdentityProject.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;

        public AccountController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);
            var user = new User()
            {
                FirstName = viewModel.FirstName,
                LastName = viewModel.LastName,
                Email = viewModel.Email,
                UserName = viewModel.Email
            };
            var result = _userManager.CreateAsync(user, viewModel.Password).Result;
            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            string message = "";
            foreach (var item in result.Errors)
            {
                message += item.Description;
            }
            TempData["Message"] = message;
            return View(viewModel);
        }
    }
}
