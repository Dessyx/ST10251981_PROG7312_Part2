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
            var announcements = _announcementService.GetAllAnnouncements();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                announcements = _announcementService.SearchAnnouncements(searchTerm);
            }

            if (!string.IsNullOrEmpty(category) && Enum.TryParse<CityPulse.Models.AnnouncementCategory>(category, out var categoryEnum))
            {
                announcements = announcements.Where(a => a.Category == categoryEnum).ToList();
            }

            if (dateFrom.HasValue && endDate.HasValue)
            {
                announcements = announcements.Where(a => a.Date >= dateFrom.Value && a.Date <= endDate.Value).ToList();
            }

            return Json(new { announcements = announcements.Take(20) });
        }
    }
}

