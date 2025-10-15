using Microsoft.AspNetCore.Mvc;
using CityPulse.Models;
using CityPulse.Services.Abstractions;    // imports

namespace CityPulse.Controllers
{
    // ----------------------------------------------------------------------------
    // Admin controller - handles authentication and announcement management
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

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Login()  // display admin login page
        {
            return View();
        }

        //-----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AdminLoginViewModel model)  
        {
            if (ModelState.IsValid)
            {
       
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

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Dashboard()  // display admin dashboard
        {
            if (!IsAdminLoggedIn())  
            {
                return RedirectToAction("Login");
            }

            var viewModel = _announcementService.GetDashboardViewModel();  
            return View(viewModel);
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult AddAnnouncement()  // display add announcement form
        {
            if (!IsAdminLoggedIn())  
            {
                return RedirectToAction("Login");
            }

            var model = new AnnouncementViewModel();  
            return View(model);
        }

        //-----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Logout()  // log out admin user
        {
            HttpContext.Session.Remove("IsAdmin");  
            return RedirectToAction("Login");
        }

        //-----------------------------------------------------------------------
        private bool IsAdminLoggedIn()  // helper method to check admin authentication
        {
            return HttpContext.Session.GetString("IsAdmin") == "true";
        }
    }
}

//----------------------------------------------- <<< End of File >>>--------------------------------
