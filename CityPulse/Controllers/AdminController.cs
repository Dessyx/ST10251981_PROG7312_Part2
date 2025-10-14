using Microsoft.AspNetCore.Mvc;
using CityPulse.Models;
using CityPulse.Services.Abstractions;

namespace CityPulse.Controllers
{
    public class AdminController : Controller   // TODO : Remove the business logic from this controller 
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

            var viewModel = new AdminDashboardViewModel
            {
                TotalAnnouncements = _announcementService.GetAllAnnouncements().Count,
                RecentAnnouncements = _announcementService.GetUpcomingAnnouncements(10)
            };

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
                var announcement = new Announcement
                {
                    Id = Guid.NewGuid(),
                    Title = model.Title,
                    Description = model.Description,
                    Category = model.Category,
                    Date = model.Date,
                    Location = model.Location,
                    Duration = model.Duration,
                    AgeGroup = model.AgeGroup,
                    AffectedAreas = model.AffectedAreas,
                    ContactInfo = model.ContactInfo,
                    IsFeatured = model.IsFeatured,
                    Priority = model.Priority,
                    CreatedAt = DateTime.Now,
                    CreatedBy = "Admin"
                };

                _announcementService.AddAnnouncement(announcement);
                TempData["SuccessMessage"] = "Announcement added successfully!";
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
