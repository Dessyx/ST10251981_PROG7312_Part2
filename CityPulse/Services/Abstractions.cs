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

    public interface IAnnouncementService
    {
        void AddAnnouncement(Announcement announcement);
        List<Announcement> GetAllAnnouncements();
        List<Announcement> GetAnnouncementsByCategory(AnnouncementCategory category);
        List<Announcement> GetAnnouncementsByDateRange(DateTime startDate, DateTime endDate);
        List<Announcement> GetRecentAnnouncements(int count);
        List<Announcement> GetRecentlyCreatedAnnouncements(int count);
        List<Announcement> GetUpcomingAnnouncements(int count);
        List<Announcement> GetFeaturedAnnouncements();
        List<Announcement> SearchAnnouncements(string searchTerm);
        Announcement? GetAnnouncementById(Guid id);
        HashSet<string> GetUniqueCategories();
        HashSet<DateTime> GetUniqueDates();
        void SeedDefaultData();
    }

   
    public interface IAdminAuthenticationService
    {
        bool ValidateCredentials(string username, string password);
        bool UserExists(string username);
    }
}


