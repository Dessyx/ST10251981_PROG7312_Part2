using Microsoft.AspNetCore.Mvc;

namespace CityPulse.Controllers
{
    public class NewsController : Controller
    {
        private readonly ILogger<NewsController> _logger;

        public NewsController(ILogger<NewsController> logger)
        {
            _logger = logger;
        }

        public IActionResult News()
        {
            return View();
        }
    }
}

