using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotnetCoreIdentityAuthDemo.Models;
using DotnetCoreIdentityAuthDemo.Models.ExtendUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotnetCoreIdentityAuthDemo.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<CustomUser> userManager;
        private readonly SignInManager<CustomUser> signInManager;

        public AccountController(UserManager<CustomUser> userManager, SignInManager<CustomUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        #region Register 
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new CustomUser
                {
                    UserName = registerViewModel.Email,
                    Email = registerViewModel.Email,
                    City = registerViewModel.City,
                    LastName = registerViewModel.LastName,
                    FirstName = registerViewModel.FirstName
                };
                var result = await userManager.CreateAsync(user, registerViewModel.Password);
                if (result.Succeeded)
                {
                    if (signInManager.IsSignedIn(User) && User.IsInRole("admin"))// In case of admin role logged in .... just redirect to List user page.
                    {
                        return RedirectToAction("ListUsers", "Users");
                    }
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "home");
                }
                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(registerViewModel);
        }

        #endregion


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        #region Login

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(string returnUrl)
        {
            LoginViewModel loginViewModel = new LoginViewModel();
            loginViewModel.ReturnUrl = returnUrl;
            loginViewModel.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return View(loginViewModel);
        }

        [HttpPost]
        [AllowAnonymous] // returnUrl is defined by dotnet core using Modelbinding. So built in method in url define this value at time of login.
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            loginViewModel.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                #region check email confirmation has been done for the user

                //var user = await userManager.FindByEmailAsync(loginViewModel.Email);

                //if (user != null && !user.EmailConfirmed && (await userManager.CheckPasswordAsync(user, loginViewModel.Password)))
                //{
                //    ModelState.AddModelError(string.Empty, "Email Not confirmed yer!!");
                //    return View(loginViewModel);
                //}
                #endregion

                var result = await signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.RememberMe, false);
                if (result.Succeeded)
                {
                    // to prevent Open redirect vulnerability example
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                }
                // If there are any errors, add them to the ModelState object
                // which will be displayed by the validation summary tag helper
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }
            return View(loginViewModel);
        }

        #endregion

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailInuser(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"Email {email} is already in use");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }


        [AllowAnonymous]
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                                new { ReturnUrl = returnUrl });
            var properties = signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }


        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", loginViewModel);
            }

            // Get the login information about the user from the external login provider
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information.");
                return View("Login", loginViewModel);
            }

            // If the user already has a login (i.e if there is a record in AspNetUserLogins
            // table) then sign-in the user with this external login provider
            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            // If there is no record in AspNetUserLogins table, the user may not have
            // a local account
            else
            {
                // Get the email claim value
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                if (email != null)
                {
                    // Create a new user without password if we do not have a user already
                    var user = await userManager.FindByEmailAsync(email);

                    if (user == null)
                    {
                        user = new CustomUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };

                        await userManager.CreateAsync(user);
                    }

                    // Add a login (i.e insert a row for the user in AspNetUserLogins table)
                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, isPersistent: false);

                    return LocalRedirect(returnUrl);
                }

                // If we cannot find the user email we cannot continue
                ViewBag.ErrorTitle = $"Email claim not received from: {info.LoginProvider}";
                ViewBag.ErrorMessage = "Please contact support on support@email.com";

                return View("Error");
            }
        }

    }
}