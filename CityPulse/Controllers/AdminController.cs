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
        public IActionResult Login(AdminLoginViewModel model)  // process admin login
        {
            if (ModelState.IsValid)
            {
                // Use secure authentication service to validate credentials
                if (_authService.ValidateCredentials(model.Username, model.Password))
                {
                    HttpContext.Session.SetString("IsAdmin", "true");  // set admin session
                    HttpContext.Session.SetString("AdminUsername", model.Username);  // store username
                    return RedirectToAction("Dashboard");  // redirect to dashboard
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password");  // show error
                }
            }
            return View(model);
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Dashboard()  // display admin dashboard
        {
            if (!IsAdminLoggedIn())  // check authentication
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
            if (!IsAdminLoggedIn())  // check authentication
            {
                return RedirectToAction("Login");
            }

            var model = new AnnouncementViewModel();  
            return View(model);
        }

        //-----------------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAnnouncement(AnnouncementViewModel model)  // process new announcement
        {
            if (!IsAdminLoggedIn())  
            {
                return RedirectToAction("Login");
            }

            if (ModelState.IsValid)
            {
                var username = HttpContext.Session.GetString("AdminUsername") ?? "Admin";  // get admin username
                _announcementService.CreateAnnouncementFromViewModel(model, username);  // create announcement
                return RedirectToAction("Dashboard");  // redirect to dashboard
            }

            return View(model);  
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Logout()  // log out admin user
        {
            HttpContext.Session.Remove("IsAdmin");  // clear admin session
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
