using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using LocalizationSample.Mvc.Models;

namespace LocalizationSample.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStringLocalizer _localizer;
        private readonly IStringLocalizer<HomeController> _localizerOfT;

        public HomeController(IStringLocalizer localizer, IStringLocalizer<HomeController> localizerOfT)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            _localizerOfT = localizerOfT ?? throw new ArgumentNullException(nameof(localizerOfT));
        }

        public IActionResult Index()
        {
            ViewData["Title"] = _localizer["Home Page"];

            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Title"] = _localizer["Privacy Policy"];
            ViewData["Message"] = _localizerOfT["Use this page to detail your site's privacy policy."];

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}