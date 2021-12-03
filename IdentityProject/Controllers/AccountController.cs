using IdentityProject.Models.Entities;
using IdentityProject.Models.ViewModels;
using IdentityProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly EmailServices _emailServices;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, EmailServices emailServices)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailServices = emailServices;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
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


        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            return View(new LoginViewModel()
            {
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = await _userManager.FindByNameAsync(viewModel.UserName);
            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, viewModel.Password, viewModel.RememberMe, true);
                if (result.Succeeded)
                    return LocalRedirect(viewModel.ReturnUrl);
                if (result.RequiresTwoFactor)
                {
                    //
                }
                if (result.IsLockedOut)
                {
                    //
                }
            }
            ModelState.AddModelError("", "Login Error");


            return View(viewModel);
        }

        public IActionResult LogOut()
        {
            _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ConfirmEmail(string UserId, string Token)
        {
            if (UserId == null || Token == null)
            {
                return BadRequest();
            }
            var user = _userManager.FindByIdAsync(UserId).Result;
            if (user == null)
            {
                return View("Error");
            }

            var result = _userManager.ConfirmEmailAsync(user, Token).Result;
            if (result.Succeeded)
            {
                /// return 
            }
            else
            {

            }
            return RedirectToAction("login");

        }

        public IActionResult DisplayEmail()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordConfirmationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }
            var user = await _userManager.FindByEmailAsync(viewModel.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                ViewBag.message = "ممکن است ایمیل معتبر نباشد یا اینکه ایمیل خود را تایید نکرده باشید";
                return View("Error");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            string callBackUrl = Url.Action("ResetPassword", "Account",
                new { UserId = user.Id, token = token }, protocol: Request.Scheme);

            string body = $"برای تنظیم مجدد کلمه ی عبور کلیک کنید <a href='{callBackUrl}'>کلیک نمایید</a>";
            await _emailServices.Excute(user.Email, body, "فعال سازی حساب کاربری");
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string UserId, string token)
        {
            return View(new ResetPasswordViewModel()
            {
                UserId = UserId,
                Token = token
            });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);
            if (viewModel.Password != viewModel.ConfirmPassword)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(viewModel.UserId);
            if (user == null)
                return BadRequest();

            var result = await _userManager.ResetPasswordAsync(user, viewModel.Token, viewModel.Password);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
            else
            {
                ViewBag.Errors = result.Errors;
                return View(viewModel);
            }
        }

        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}
