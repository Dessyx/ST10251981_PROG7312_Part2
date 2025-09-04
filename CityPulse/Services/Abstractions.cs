using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CityPulse.Models;

namespace CityPulse.Services.Abstractions
{
	public interface IReferenceNumberService
	{
		string CreateReference();
	}

	public interface IStorageService
	{
		Task<Attachment> SaveAsync(IFormFile file);
	}

	public interface IIssueReportingService
	{
		Task<IssueReport> CreateAsync(IssueReportCreateRequest request);

		CityPulse.Models.Queue<string> GetLocationSuggestions(string query);
	}
}


