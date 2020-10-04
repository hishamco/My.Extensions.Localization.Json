using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using LocalizationSample.Mvc.ViewModels;

namespace LocalizationSample.Mvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly IStringLocalizer<AccountController> _localizer;

        public AccountController(IStringLocalizer<AccountController> localizer)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}