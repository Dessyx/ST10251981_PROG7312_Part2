using Microsoft.AspNetCore.Mvc;
using CityPulse.Services.Abstractions;    // imports

namespace CityPulse.Controllers
{
    // ----------------------------------------------------------------------------
    // News and announcements controller 
    public class NewsController : Controller
    {
        private readonly ILogger<NewsController> _logger;
        private readonly IAnnouncementService _announcementService;

        public NewsController(ILogger<NewsController> logger, IAnnouncementService announcementService)
        {
            _logger = logger;
            _announcementService = announcementService;
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult News()  // display news page with all announcements
        {
            var announcements = _announcementService.GetAllAnnouncements();  // retrieve all announcements
            return View(announcements);
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Search(string searchTerm, string category, DateTime? dateFrom, DateTime? endDate)  // search announcements with filters
        {
            var announcements = _announcementService.SearchWithFilters(searchTerm, category, dateFrom, endDate, 20);  // get the filtered results
            return Json(new { announcements });  
        }
    }
}

//----------------------------------------------- <<< End of File >>>--------------------------------

