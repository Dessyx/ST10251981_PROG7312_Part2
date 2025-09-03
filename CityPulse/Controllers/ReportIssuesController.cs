using System.Threading.Tasks;
using CityPulse.Models;
using CityPulse.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CityPulse.Controllers
{
	public class ReportIssuesController : Controller
	{
		private readonly IIssueReportingService _service;

		public ReportIssuesController(IIssueReportingService service)
		{
			_service = service;
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([FromForm] string location, [FromForm] IssueCategory category, [FromForm] string description)
		{
			var request = new IssueReportCreateRequest
			{
				Location = location,
				Category = category,
				Description = description
			};

			foreach (var file in Request.Form.Files)
			{
				request.UploadQueue.Enqueue(file);
			}

			if (!TryValidateModel(request))
			{
				ModelState.AddModelError("", "Please correct the errors and try again.");
				return View();
			}

			IssueReport report;
			try
			{
				report = await _service.CreateAsync(request);
			}
			catch (System.Exception ex)
			{
				ModelState.AddModelError("", ex.Message);
				return View();
			}

			TempData["ReferenceNumber"] = report.ReferenceNumber;
			return RedirectToAction("Success");
		}

		[HttpGet]
		public IActionResult Success()
		{
			ViewBag.ReferenceNumber = TempData["ReferenceNumber"]?.ToString();
			return View();
		}

		[HttpGet]
		public IActionResult LocationSuggest([FromQuery] string q)
		{
			var suggestions = _service.GetLocationSuggestions(q);
			return Json(new { suggestions = suggestions });
		}
	}
}


