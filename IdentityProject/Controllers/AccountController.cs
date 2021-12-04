using IdentityProject.Models.Entities;
using IdentityProject.Models.ViewModels;
using IdentityProject.Services;
using Microsoft.AspNetCore.Authorization;
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

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var userInfo = new MyAccountinfoViewModel()
            {
                FullName = $"{user.FirstName}  {user.LastName}",
                UserName = user.UserName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled
            };
            return View(userInfo);
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
                    return RedirectToAction("TwoFactorLogin", new { viewModel.UserName, viewModel.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    //
                }
            }
            ModelState.AddModelError("", "Login Error");


            return View(viewModel);
        }

        public async Task<IActionResult> TwoFactorLogin(string userName, bool rememberMe)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return BadRequest();
            }
            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            TwoFactorLoginViewModel twoFactorLogin = new TwoFactorLoginViewModel();
            if (providers.Contains("Phone"))
            {
                var smsCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
                //SmsService.Send(user.PhoneNumber, smsCode);
                twoFactorLogin.Provider = "Phone";
                twoFactorLogin.IsPersistent = rememberMe;
            }
            else if (providers.Contains("Email"))
            {
                var emailCode = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                //EmailServices email = new EmailServices();
                //await email.Excute(user.Email, $"Tow Factor Code {emailCode}", "Tow Factor Login");
                twoFactorLogin.Provider = "Email";
                twoFactorLogin.IsPersistent = rememberMe;
            }
            return View(twoFactorLogin);
        }


        [HttpPost]
        public async Task<IActionResult> TwoFactorLogin(TwoFactorLoginViewModel viewModel)
        {

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return BadRequest();
            }
            else
            {
              var result = await _signInManager.TwoFactorSignInAsync(viewModel.Provider, viewModel.Code, viewModel.IsPersistent, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else if(result.IsLockedOut)
                {
                    ModelState.AddModelError("", "حساب کاربری شما قفل شده است");
                    return View();
                }
            }
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

        [HttpGet]
        [Authorize]
        public IActionResult SetPhoneNumber()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SetPhoneNumber(SetPhoneNumberViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var result = await _userManager.SetPhoneNumberAsync(user, viewModel.PhoneNumber);
                string code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, viewModel.PhoneNumber);

                // SMS
                //SmsService.Send(viewModel.PhoneNumber, code);
                TempData["PhoneNumber"] = viewModel.PhoneNumber;
                return RedirectToAction(nameof(VerifyPhoneNumber));
            }
            return View(viewModel);
        }

        [Authorize]
        public IActionResult VerifyPhoneNumber()
        {

            return View(new VerifyPhoneNumberViewModel
            {
                PhoneNumber = TempData["PhoneNumber"].ToString(),
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel verify)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            bool resultVerify = await _userManager.VerifyChangePhoneNumberTokenAsync(user, verify.Code, verify.PhoneNumber);
            if (!resultVerify)
            {
                ViewData["Message"] = $"کد وارد شده برای شماره {verify.PhoneNumber} اشتباه اشت";
                return View(verify);
            }
            else
            {
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("VerifySuccess");

        }


        public IActionResult VerifySuccess()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> TwoFactorEnabled()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var result = await _userManager.SetTwoFactorEnabledAsync(user, !user.TwoFactorEnabled);
            return RedirectToAction(nameof(Index));
        }
    }
}
