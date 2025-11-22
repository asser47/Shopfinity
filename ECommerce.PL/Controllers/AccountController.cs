using ECommerce.BLL.Interfaces;
using ECommerce.DAL.Models;
using ECommerce.PL.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ICartService _cartService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ICartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _cartService = cartService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user;

                if (model.UserType == "Buyer")
                {
                    user = new Buyer
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName,
                        CompanyName = model.CompanyName,
                        TaxNumber = model.TaxNumber,
                    };
                }
                else
                {
                    user = new Customer
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FullName = model.FullName,
                        Address = model.Address,
                        PhoneNumber = model.PhoneNumber,
                    };
                }

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await EnsureRolesExist();
                    await _userManager.AddToRoleAsync(user, model.UserType);
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    await MigrateSessionCart(user.Id);

                    TempData["SuccessMessage"] = "Registration successful! Welcome!";
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);

                    await MigrateSessionCart(user.Id);

                    TempData["SuccessMessage"] = "Login successful!";

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task EnsureRolesExist()
        {
            string[] roles = { "Customer", "Buyer", "Admin" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task MigrateSessionCart(string userId)
        {
            var anonymousUserId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(anonymousUserId) && anonymousUserId != userId)
            {
                await _cartService.MergeCartsAsync(anonymousUserId, userId);
                HttpContext.Session.Remove("UserId");
            }
        }
    }
}
