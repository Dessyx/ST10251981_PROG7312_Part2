using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using CityPulse.Models;    // imports

namespace CityPulse.Services.Abstractions
{
	// ----------------------------------------------------------------------------
	// Reference number generation service interface
	public interface IReferenceNumberService
	{
		string CreateReference();  // generate unique reference number for issue reports
	}

	// ----------------------------------------------------------------------------
	// File storage service interface
	public interface IStorageService
	{
		Task<Attachment> SaveAsync(IFormFile file);  // save uploaded file and return attachment metadata
	}

	// ----------------------------------------------------------------------------
	// Issue reporting service interface
	public interface IIssueReportingService
	{
		Task<IssueReport> CreateAsync(IssueReportCreateRequest request);  // create new issue report with attachments

		CityPulse.Models.Queue<string> GetLocationSuggestions(string query);  // get location autocomplete suggestions
	}

	// ----------------------------------------------------------------------------
	// Announcement management service interface
	public interface IAnnouncementService
	{
		void AddAnnouncement(Announcement announcement);  // add new announcement to storage
		Announcement CreateAnnouncementFromViewModel(AnnouncementViewModel viewModel, string createdBy);  // create announcement from view model
		List<Announcement> GetAllAnnouncements();  // retrieve all announcements
		List<Announcement> GetAnnouncementsByCategory(AnnouncementCategory category);  // filter announcements by category
		List<Announcement> GetAnnouncementsByDateRange(DateTime startDate, DateTime endDate);  // filter announcements by date range
		List<Announcement> GetRecentAnnouncements(int count);  // get most recent announcements
		List<Announcement> GetRecentlyCreatedAnnouncements(int count);  // get recently created announcements
		List<Announcement> GetUpcomingAnnouncements(int count);  // get upcoming future announcements
		List<Announcement> GetFeaturedAnnouncements();  // get featured/priority announcements
		List<Announcement> SearchAnnouncements(string searchTerm);  // search announcements by text
		List<Announcement> SearchWithFilters(string? searchTerm, string? category, DateTime? dateFrom, DateTime? dateTo, int maxResults);  // advanced search with multiple filters
		Announcement? GetAnnouncementById(Guid id);  // retrieve specific announcement by ID
		AdminDashboardViewModel GetDashboardViewModel();  // get dashboard statistics and data
		HashSet<string> GetUniqueCategories();  // get all unique category values
		HashSet<DateTime> GetUniqueDates();  // get all unique date values
		void SeedDefaultData();  // populate initial sample announcements
	}

	// ----------------------------------------------------------------------------
	// Admin authentication service interface
	public interface IAdminAuthenticationService
	{
		bool ValidateCredentials(string username, string password);  // verifies admin login credentials
		bool UserExists(string username);  // checks if admin user exists
	}

	// ----------------------------------------------------------------------------
	// Recommendation service interface - analyzes user patterns and suggests relevant announcements
	public interface IRecommendationService
	{
		void TrackSearch(string userId, string searchTerm, string? category);  // track user search activity
		void TrackView(string userId, Announcement announcement);  // track announcement views
		List<Announcement> GetRecommendations(string userId, int count);  // get personalized recommendations
		List<Announcement> GetTrendingAnnouncements(int count);  // get trending announcements
		List<Announcement> GetRelatedAnnouncements(Announcement announcement, int count);  // get similar announcements
		Dictionary<AnnouncementCategory, int> GetUserPreferences(string userId);  // get user category preferences
	}

	// ----------------------------------------------------------------------------
	// User authentication service interface - manages user accounts
	public interface IUserService
	{
		Task<User?> RegisterAsync(UserRegisterViewModel model);  // register new user
		Task<User?> LoginAsync(string usernameOrEmail, string password);  // authenticate user
		User? GetUserById(Guid userId);  // get user by ID
		User? GetUserByUsername(string username);  // get user by username
		User? GetUserByEmail(string email);  // get user by email
		bool UsernameExists(string username);  // check if username exists
		bool EmailExists(string email);  // check if email exists
		void UpdateLastLogin(Guid userId);  // update last login timestamp
	}
}

//----------------------------------------------- <<< End of File >>>-------------------------------- 
