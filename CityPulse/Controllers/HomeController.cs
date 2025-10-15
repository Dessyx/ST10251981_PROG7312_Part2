using System.Diagnostics;
using CityPulse.Models;
using Microsoft.AspNetCore.Mvc;    // imports

namespace CityPulse.Controllers
{
    // ----------------------------------------------------------------------------
    // Home controller - handles main landing page and general site pages
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Index()  // display home page
        {
            return View();
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Privacy()  
        {
            return View();
        }

        //-----------------------------------------------------------------------
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()  
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

//----------------------------------------------- <<< End of File >>>--------------------------------
