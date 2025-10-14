using Microsoft.AspNetCore.Mvc;
using CityPulse.Models;
using CityPulse.Services.Abstractions;

namespace CityPulse.Controllers
{
    public class AdminController : Controller 
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IAnnouncementService _announcementService;
        private readonly IAdminAuthenticationService _authService;

        public AdminController(ILogger<AdminController> logger, IAnnouncementService announcementService, IAdminAuthenticationService authService)
        {
            _logger = logger;
            _announcementService = announcementService;
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Use secure authentication service to validate credentials
                if (_authService.ValidateCredentials(model.Username, model.Password))
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                    HttpContext.Session.SetString("AdminUsername", model.Username);
                    return RedirectToAction("Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password");
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var viewModel = _announcementService.GetDashboardViewModel();
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult AddAnnouncement()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            var model = new AnnouncementViewModel();
            return View(model);
        }

        [HttpPost]
        public IActionResult AddAnnouncement(AnnouncementViewModel model)
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var username = HttpContext.Session.GetString("AdminUsername") ?? "Admin";
                _announcementService.CreateAnnouncementFromViewModel(model, username);
                return RedirectToAction("Dashboard");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsAdmin");
            return RedirectToAction("Login");
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }
    }
}
