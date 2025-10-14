using Microsoft.AspNetCore.Mvc;
using CityPulse.Services.Abstractions;

namespace CityPulse.Controllers
{
    public class NewsController : Controller
    {
        private readonly ILogger<NewsController> _logger;
        private readonly IAnnouncementService _announcementService;

        public NewsController(ILogger<NewsController> logger, IAnnouncementService announcementService)
        {
            _logger = logger;
            _announcementService = announcementService;
        }

        public IActionResult News()
        {
            var announcements = _announcementService.GetAllAnnouncements();
            return View(announcements);
        }

        [HttpGet]
        public IActionResult Search(string searchTerm, string category, DateTime? dateFrom, DateTime? endDate)
        {
            var announcements = _announcementService.SearchWithFilters(searchTerm, category, dateFrom, endDate, 20);
            return Json(new { announcements });
        }
    }
}

