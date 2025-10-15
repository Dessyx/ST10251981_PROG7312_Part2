using Microsoft.AspNetCore.Mvc;
using CityPulse.Models;
using CityPulse.Services.Abstractions;    // imports

namespace CityPulse.Controllers
{
    // ----------------------------------------------------------------------------
    // News and announcements controller - handles public news feed and search with recommendations
    public class NewsController : Controller
    {
        private readonly ILogger<NewsController> _logger;
        private readonly IAnnouncementService _announcementService;
        private readonly IRecommendationService _recommendationService;

        public NewsController(ILogger<NewsController> logger, IAnnouncementService announcementService, IRecommendationService recommendationService)
        {
            _logger = logger;
            _announcementService = announcementService;
            _recommendationService = recommendationService;
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult News()  // display news page with all announcements
        {
            var announcements = _announcementService.GetAllAnnouncements();  // retrieve all announcements
            
            // Get user ID (from logged-in user or session)
            var userId = GetUserIdentifier();
            
            // Get personalized recommendations
            var recommendations = _recommendationService.GetRecommendations(userId, 6);
            ViewBag.Recommendations = recommendations;
            
            // Get trending announcements
            var trending = _recommendationService.GetTrendingAnnouncements(5);
            ViewBag.Trending = trending;
            
            // Check if user is logged in
            ViewBag.IsLoggedIn = HttpContext.Session.GetString("UserId") != null;
            ViewBag.Username = HttpContext.Session.GetString("Username");
            
            return View(announcements);
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult Search(string searchTerm, string category, DateTime? dateFrom, DateTime? endDate)  // search announcements with filters
        {
            var userId = GetUserIdentifier();  // get user identifier
            
            // Track search for recommendations
            _recommendationService.TrackSearch(userId, searchTerm, category);
            
            var announcements = _announcementService.SearchWithFilters(searchTerm, category, dateFrom, endDate, 20);  // get the filtered results
            return Json(new { announcements });  
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult GetRecommendations(int count = 6)  // get personalized recommendations
        {
            var userId = GetUserIdentifier();
            
            // Get user preferences for debugging
            var preferences = _recommendationService.GetUserPreferences(userId);
            
            _logger.LogInformation("GetRecommendations - UserId: {UserId}, Preferences: {Preferences}", 
                userId, string.Join(", ", preferences.Select(p => $"{p.Key}={p.Value}")));
            
            var recommendations = _recommendationService.GetRecommendations(userId, count);
            
            return Json(new { 
                recommendations = recommendations,
                preferences = preferences,
                userId = userId
            });
        }

        //-----------------------------------------------------------------------
        [HttpGet]
        public IActionResult GetTrending(int count = 5)  // get trending announcements
        {
            var trending = _recommendationService.GetTrendingAnnouncements(count);
            return Json(new { trending });
        }

        //-----------------------------------------------------------------------
        [HttpPost]
        public IActionResult TrackView([FromBody] TrackViewModel model)  // track announcement view for recommendations
        {
            try
            {
                var userId = GetUserIdentifier();
                
                _logger.LogInformation("TrackView called - UserId: {UserId}, AnnouncementId: {AnnouncementId}", userId, model.AnnouncementId);
                
                var announcement = _announcementService.GetAnnouncementById(model.AnnouncementId);
                
                if (announcement != null)
                {
                    _recommendationService.TrackView(userId, announcement);
                    
                    var isLoggedIn = HttpContext.Session.GetString("UserId") != null;
                    _logger.LogInformation("View tracked successfully - IsLoggedIn: {IsLoggedIn}, Category: {Category}", isLoggedIn, announcement.Category);
                    
                    return Json(new { success = true, isLoggedIn = isLoggedIn, category = announcement.Category.ToString() });
                }
                
                _logger.LogError("Announcement not found - AnnouncementId: {AnnouncementId}", model.AnnouncementId);
                return Json(new { success = false, message = $"Announcement not found: {model.AnnouncementId}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TrackView");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        //-----------------------------------------------------------------------
        private string GetUserIdentifier()  // get unique user identifier (logged-in user ID or session ID)
        {
            // Try to get logged-in user ID first
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("User identified as logged-in user: {UserId}", userId);
                return userId;
            }
            
            // Fallback to session ID for guest users
            var guestId = "guest_" + HttpContext.Session.Id;
            _logger.LogInformation("User identified as guest: {GuestId}", guestId);
            return guestId;
        }
    }
}

//----------------------------------------------- <<< End of File >>>--------------------------------

