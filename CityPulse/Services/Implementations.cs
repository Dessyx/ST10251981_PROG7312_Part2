using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;           // Imports
using System.Threading.Tasks;					
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using CityPulse.Models;
using CityPulse.Services.Abstractions;
using System.Security.Cryptography;

// ---------------------------------------- Implementation class -------------------------------------------------
namespace CityPulse.Services
{

	//-----------------------------------------------------------------------------
	// Generate reference numbers for submitted reports
	public sealed class ReferenceNumberService : IReferenceNumberService
	{
		//-----------------------------------------------------------------------
		public string CreateReference()  // creates a unique reference number
		{
		
			var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			var randomPart = Random.Shared.Next(1000, 9999);
			return $"CP-{datePart}-{randomPart}";
		}
	}


    //-----------------------------------------------------------------------------
	// Storing images and documents
    public sealed class LocalStorageService : IStorageService
	{
		private readonly string _root;
		private static readonly long MaxFileSizeBytes = 5 * 1024 * 1024; 
		private static readonly Regex SafeName = new Regex("[^a-zA-Z0-9_.-]", RegexOptions.Compiled);

		//-----------------------------------------------------------------------
		public LocalStorageService(IWebHostEnvironment env)
		{
			_root = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
			Directory.CreateDirectory(_root);
		}

		//-----------------------------------------------------------------------
		public async Task<Attachment> SaveAsync(IFormFile file)
		{
			// Validate file
			if (file == null || file.Length == 0) throw new InvalidOperationException("Empty file.");
			if (file.Length > MaxFileSizeBytes) throw new InvalidOperationException("File exceeds 5 MB limit.");

			var safeFileName = SafeName.Replace(Path.GetFileName(file.FileName), "_");
			var uniqueName = $"{Guid.NewGuid():N}_{safeFileName}";
			var fullPath = Path.Combine(_root, uniqueName);

			await using (var stream = File.Create(fullPath))
			{
				await file.CopyToAsync(stream);
			}

			return new Attachment
			{
				FileName = file.FileName,
				ContentType = file.ContentType,
				StoredPath = $"/uploads/{uniqueName}",
				LengthBytes = file.Length
			};
		}
	}


    //-----------------------------------------------------------------------------
	// Storing submitted reports
    public sealed class IssueReportingService : IIssueReportingService
	{
		private readonly IReferenceNumberService _referenceNumberService;
		private readonly IStorageService _storageService;

		//-----------------------------------------------------------------------
		public IssueReportingService(IReferenceNumberService referenceNumberService, IStorageService storageService)
		{
			_referenceNumberService = referenceNumberService;
			_storageService = storageService;
		}

		//-----------------------------------------------------------------------
		public async Task<IssueReport> CreateAsync(IssueReportCreateRequest request)  // create new issue report
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			
			
			var report = new IssueReport
			{
				ReferenceNumber = _referenceNumberService.CreateReference(),  // generate reference number
				Location = request.Location,
				Category = request.Category,
				Description = request.Description
			};

		
			while (request.UploadQueue.TryDequeue(out var file))
			{
				var saved = await _storageService.SaveAsync(file);
				report.Attachments.AddLast(saved);
			}

			return report;
		}


        //-----------------------------------------------------------------------
        public CityPulse.Models.Queue<string> GetLocationSuggestions(string query)
		{
			var results = new CityPulse.Models.Queue<string>();  // queue for results
			if (string.IsNullOrWhiteSpace(query)) return results;

			// Seed location data 
			var seeded = new DoublyLinkedList<string>();
			seeded.AddLast("Cape Town CBD");
			seeded.AddLast("Johannesburg North");
			seeded.AddLast("Durban Central");
			seeded.AddLast("Pretoria East");
			seeded.AddLast("Gqeberha");
			seeded.AddLast("Bloemfontein");

		
			var node = seeded.Head;
			query = query.Trim();
			while (node != null)
			{
				if (node.Value.StartsWith(query, StringComparison.OrdinalIgnoreCase))
				{
					results.Enqueue(node.Value);
				}
				node = node.Next;
			}

			return results;
		}
	}

	//-----------------------------------------------------------------------------
	// Announcement Service with Advanced Data Structures
	public sealed class AnnouncementService : IAnnouncementService
	{
		// Primary storage: SortedDictionary for date-based organization
		private readonly SortedDictionary<DateTime, List<Announcement>> _announcementsByDate;
		
		// Secondary indexes for efficient lookups
		private readonly Dictionary<AnnouncementCategory, List<Announcement>> _announcementsByCategory;
		private readonly Dictionary<Guid, Announcement> _announcementsById;
		private readonly Dictionary<string, HashSet<Guid>> _searchIndex; // Inverted index for text search
		
		// Sets for unique values
		private readonly HashSet<string> _uniqueCategories;
		private readonly HashSet<DateTime> _uniqueDates;
		
		// Priority queue for featured/important announcements
		private readonly PriorityQueue<Announcement, int> _priorityQueue;
		
		// Stack for recently viewed announcements
		private readonly Stack<Announcement> _recentlyViewed;
		
		// Queue for pending announcements (admin workflow)
		private readonly System.Collections.Generic.Queue<Announcement> _pendingAnnouncements;

		//-----------------------------------------------------------------------
		public AnnouncementService()  // initialize announcement service
		{
			_announcementsByDate = new SortedDictionary<DateTime, List<Announcement>>();
			_announcementsByCategory = new Dictionary<AnnouncementCategory, List<Announcement>>();      // Initialise the data structures
			_announcementsById = new Dictionary<Guid, Announcement>();
			_searchIndex = new Dictionary<string, HashSet<Guid>>();
			_uniqueCategories = new HashSet<string>();
			_uniqueDates = new HashSet<DateTime>();
			_priorityQueue = new PriorityQueue<Announcement, int>();
			_recentlyViewed = new Stack<Announcement>();
			_pendingAnnouncements = new System.Collections.Generic.Queue<Announcement>();
			
		
			foreach (AnnouncementCategory category in Enum.GetValues<AnnouncementCategory>())
			{
				_announcementsByCategory[category] = new List<Announcement>();
				_uniqueCategories.Add(category.ToString());
			}
			
			SeedDefaultData();
		}

		//-----------------------------------------------------------------------
		public void AddAnnouncement(Announcement announcement)  // add announcement to all data structures
		{
			if (!_announcementsByDate.ContainsKey(announcement.Date.Date))
			{
				_announcementsByDate[announcement.Date.Date] = new List<Announcement>();
			}
			_announcementsByDate[announcement.Date.Date].Add(announcement);
			_announcementsByCategory[announcement.Category].Add(announcement);
			_announcementsById[announcement.Id] = announcement;
			AddToSearchIndex(announcement);
			_uniqueDates.Add(announcement.Date.Date);
			
			if (announcement.IsFeatured || announcement.Priority <= AnnouncementPriority.High)
			{
				_priorityQueue.Enqueue(announcement, (int)announcement.Priority);
			}
		}

		//-----------------------------------------------------------------------
		public List<Announcement> GetAllAnnouncements()  // retrieve all announcements sorted by date
		{
			var allAnnouncements = new List<Announcement>();
			foreach (var dateGroup in _announcementsByDate.Values)
			{
				allAnnouncements.AddRange(dateGroup);
			}
			return allAnnouncements.OrderByDescending(a => a.Date).ToList();
		}

		//-----------------------------------------------------------------------
		public List<Announcement> GetAnnouncementsByCategory(AnnouncementCategory category)  // get announcements by category
		{
			return _announcementsByCategory[category].OrderByDescending(a => a.Date).ToList();
		}

		//-----------------------------------------------------------------------
		public List<Announcement> GetAnnouncementsByDateRange(DateTime startDate, DateTime endDate)  // get announcements within date range
		{
			var results = new List<Announcement>();
			foreach (var kvp in _announcementsByDate)
			{
				if (kvp.Key >= startDate.Date && kvp.Key <= endDate.Date)
				{
					results.AddRange(kvp.Value);
				}
			}
			return results.OrderByDescending(a => a.Date).ToList();
		}

	//-----------------------------------------------------------------------
	public List<Announcement> GetRecentAnnouncements(int count)  // get most recent announcements
	{
		return GetAllAnnouncements().Take(count).ToList();
	}

	//-----------------------------------------------------------------------
	public List<Announcement> GetRecentlyCreatedAnnouncements(int count)  // get recently created announcements
	{
		return GetAllAnnouncements()
			.OrderByDescending(a => a.CreatedAt)  // sort by creation time
			.Take(count)
			.ToList();
	}

	//-----------------------------------------------------------------------
	public List<Announcement> GetUpcomingAnnouncements(int count)  // get upcoming future announcements
	{
		var today = DateTime.Now.Date;
		return GetAllAnnouncements()
			.Where(a => a.Date.Date >= today)
			.OrderBy(a => a.Date)
			.Take(count)
			.ToList();
	}

	//-----------------------------------------------------------------------
	public Announcement CreateAnnouncementFromViewModel(AnnouncementViewModel viewModel, string createdBy)
	{

		var announcement = new Announcement
		{
			Id = Guid.NewGuid(),
			Title = viewModel.Title,
			Description = viewModel.Description,
			Category = viewModel.Category,
			Date = viewModel.Date,
			Location = viewModel.Location,
			Duration = viewModel.Duration,
			AgeGroup = viewModel.AgeGroup,
			AffectedAreas = viewModel.AffectedAreas,
			ContactInfo = viewModel.ContactInfo,
			IsFeatured = viewModel.IsFeatured,
			Priority = viewModel.Priority,
			CreatedAt = DateTime.Now,
			CreatedBy = createdBy
		};

		AddAnnouncement(announcement);
		return announcement;
	}

	//-----------------------------------------------------------------------
	public List<Announcement> SearchWithFilters(string? searchTerm, string? category, DateTime? dateFrom, DateTime? dateTo, int maxResults)  // advanced search with filters
	{
		var announcements = GetAllAnnouncements();

		if (!string.IsNullOrEmpty(searchTerm))
		{
			announcements = SearchAnnouncements(searchTerm);
		}

		// category filter
		if (!string.IsNullOrEmpty(category) && Enum.TryParse<AnnouncementCategory>(category, out var categoryEnum))
		{
			announcements = announcements.Where(a => a.Category == categoryEnum).ToList();
		}

		// date range filter
		if (dateFrom.HasValue && dateTo.HasValue)
		{
			announcements = announcements.Where(a => a.Date >= dateFrom.Value && a.Date <= dateTo.Value).ToList();
		}

		return announcements.Take(maxResults).ToList();
	}

	//-----------------------------------------------------------------------
	public AdminDashboardViewModel GetDashboardViewModel()  // get announcements for dashboard
	{
		return new AdminDashboardViewModel
		{
			TotalAnnouncements = GetAllAnnouncements().Count,
			RecentAnnouncements = GetUpcomingAnnouncements(10)
		};
	}

		//-----------------------------------------------------------------------
		public List<Announcement> GetFeaturedAnnouncements()  // get featured announcements 
		{
			var featured = new List<Announcement>();
			var tempQueue = new PriorityQueue<Announcement, int>();
			
			while (_priorityQueue.Count > 0)
			{
				var announcement = _priorityQueue.Dequeue();
				featured.Add(announcement);
				tempQueue.Enqueue(announcement, (int)announcement.Priority);
			}
			
			while (tempQueue.Count > 0)
			{
				var announcement = tempQueue.Dequeue();
				_priorityQueue.Enqueue(announcement, (int)announcement.Priority);
			}
			
			return featured.OrderBy(a => a.Priority).ToList();
		}

        //-----------------------------------------------------------------------
        // Full-text search using inverted index to search announcements
		public List<Announcement> SearchAnnouncements(string searchTerm)
		{
			if (string.IsNullOrWhiteSpace(searchTerm))
				return GetAllAnnouncements();
			
			var searchWords = searchTerm.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
			var matchingIds = new HashSet<Guid>();
			
			foreach (var word in searchWords)
			{
				if (_searchIndex.ContainsKey(word))
				{
					if (matchingIds.Count == 0)
					{
						matchingIds = new HashSet<Guid>(_searchIndex[word]);
					}
					else
					{
						matchingIds.IntersectWith(_searchIndex[word]);
					}
				}
			}
			
			// Retrieve actual announcement objects
			var results = new List<Announcement>();
			foreach (var id in matchingIds)
			{
				if (_announcementsById.ContainsKey(id))
				{
					results.Add(_announcementsById[id]);
				}
			}
			
			return results.OrderByDescending(a => a.Date).ToList();
		}

		//-----------------------------------------------------------------------
		public Announcement? GetAnnouncementById(Guid id)  // get specific announcement by ID
		{
			return _announcementsById.ContainsKey(id) ? _announcementsById[id] : null;
		}

		//-----------------------------------------------------------------------
		public HashSet<string> GetUniqueCategories()  // get all unique category names
		{
			return new HashSet<string>(_uniqueCategories);
		}

		//-----------------------------------------------------------------------
		public HashSet<DateTime> GetUniqueDates()  // get all unique announcement dates
		{
			return new HashSet<DateTime>(_uniqueDates);
		}

		//-----------------------------------------------------------------------
		private void AddToSearchIndex(Announcement announcement)
		{
			var text = $"{announcement.Title} {announcement.Description}".ToLower();
			var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			
			foreach (var word in words)
			{
				var cleanWord = new string(word.Where(c => char.IsLetterOrDigit(c)).ToArray());  // remove punctuation
				if (!string.IsNullOrEmpty(cleanWord))
				{
					if (!_searchIndex.ContainsKey(cleanWord))
					{
						_searchIndex[cleanWord] = new HashSet<Guid>();
					}
					_searchIndex[cleanWord].Add(announcement.Id);  
				}
			}
		}

		//-----------------------------------------------------------------------
		public void SeedDefaultData()  // populate with sample announcements
		{
			var defaultAnnouncements = new List<Announcement>
			{
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Water Infrastructure Upgrade Project Begins Next Month",
					Description = "The municipality is pleased to announce a major water infrastructure upgrade project starting November 2025. This initiative aims to improve water quality and reduce service interruptions across the city. Residents in affected areas will be notified in advance of any temporary disruptions.",
					Category = AnnouncementCategory.Announcement,
					Date = new DateTime(2025, 10, 10),
					AffectedAreas = "Central Business District, Riverside, Green Valley",
					IsFeatured = true,
					Priority = AnnouncementPriority.High,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Annual City Clean-Up Day - October 28, 2025",
					Description = "Join us for the Annual City Clean-Up Day! Volunteers will gather at various locations throughout the city to help keep our community beautiful. Free refreshments and community service certificates will be provided to all participants.",
					Category = AnnouncementCategory.Event,
					Date = new DateTime(2025, 10, 28),
					Location = "Various Citywide Locations",
					Duration = "8:00 AM - 2:00 PM",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "New Municipal Mobile App Launched",
					Description = "Download the new CityPulse mobile app to report issues, track service requests, and receive real-time notifications about municipal services. Available now on iOS and Android platforms.",
					Category = AnnouncementCategory.ServiceUpdate,
					Date = new DateTime(2025, 10, 5),
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Scheduled Power Maintenance - October 20, 2025",
					Description = "Please be advised of scheduled power maintenance on October 20, 2025, from 9:00 AM to 3:00 PM. Affected areas include Hilltop Heights and Sunset Park. We apologize for any inconvenience.",
					Category = AnnouncementCategory.Notice,
					Date = new DateTime(2025, 10, 20),
					Duration = "Approximately 6 hours",
					AffectedAreas = "Hilltop Heights, Sunset Park",
					Priority = AnnouncementPriority.High,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Youth Development Program Registration Open",
					Description = "The municipality is now accepting applications for the Youth Development Program. This initiative offers free skills training, mentorship, and employment opportunities for youth aged 18-35. Limited spaces available.",
					Category = AnnouncementCategory.Program,
					Date = new DateTime(2025, 10, 1),
					AgeGroup = "18-35 years",
					Duration = "Deadline: October 31, 2025",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Community Music Festival",
					Description = "Enjoy an evening of live music, food vendors, and family activities at our annual Community Music Festival. Free entry for all residents!",
					Category = AnnouncementCategory.Event,
					Date = new DateTime(2025, 10, 15),
					Location = "City Central Park",
					Duration = "4:00 PM - 10:00 PM",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Road Resurfacing Project Completed Ahead of Schedule",
					Description = "The Main Street resurfacing project has been completed two weeks ahead of schedule. Thank you for your patience during the construction period.",
					Category = AnnouncementCategory.ServiceUpdate,
					Date = new DateTime(2025, 9, 28),
					Location = "Main Street",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "New Recycling Centers Opening Citywide",
					Description = "Five new recycling centers will open across the city to promote environmental sustainability. Drop-off facilities available for paper, plastic, glass, and electronics.",
					Category = AnnouncementCategory.Announcement,
					Date = new DateTime(2025, 9, 22),
					Location = "5 Centers Citywide",
					Duration = "Opening: November 1, 2025",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Weather Advisory",
					Description = "Heavy rainfall and strong winds expected over the next 48 hours. Residents are advised to stay informed and take necessary precautions.",
					Category = AnnouncementCategory.Emergency,
					Date = new DateTime(2025, 10, 13),
					Priority = AnnouncementPriority.Critical,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Free Swimming Lessons for Kids",
					Description = "The municipal swimming pool is offering free swimming lessons for children aged 6-12. Classes run every Saturday morning for 8 weeks. Registration is now open at the City Recreation Center.",
					Category = AnnouncementCategory.Event,
					Date = new DateTime(2025, 11, 2),
					Location = "Municipal Swimming Pool",
					Duration = "9:00 AM - 11:00 AM (Saturdays)",
					AgeGroup = "6-12 years",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "New Online Payment System for Municipal Bills",
					Description = "We've upgraded our online payment portal for water, electricity, and property taxes. The new system offers faster processing, multiple payment options, and automatic receipts. Visit our website to register today!",
					Category = AnnouncementCategory.ServiceUpdate,
					Date = new DateTime(2025, 10, 25),
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Small Business Development Workshop Series",
					Description = "Join our comprehensive 6-week workshop series designed to help local entrepreneurs grow their businesses. Topics include marketing, finance, and digital presence. Limited to 30 participants - register early!",
					Category = AnnouncementCategory.Program,
					Date = new DateTime(2025, 11, 5),
					Location = "City Business Hub",
					Duration = "Every Wednesday, 6:00 PM - 8:00 PM",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Halloween Community Block Party",
					Description = "Bring the whole family for a spooktacular evening! Enjoy trick-or-treating, costume contests, face painting, and live entertainment. Safe, family-friendly fun for all ages.",
					Category = AnnouncementCategory.Event,
					Date = new DateTime(2025, 10, 31),
					Location = "Market Square",
					Duration = "5:00 PM - 9:00 PM",
					AgeGroup = "All ages welcome",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Road Closure: Bridge Maintenance November 10-12",
					Description = "The Oak Street Bridge will be closed for routine maintenance and safety inspections. Alternative routes via Elm Avenue and Pine Street will be clearly marked. Expect minor delays during peak hours.",
					Category = AnnouncementCategory.Notice,
					Date = new DateTime(2025, 11, 10),
					Location = "Oak Street Bridge",
					Duration = "November 10-12, 2025 (3 days)",
					AffectedAreas = "Oak Street, Downtown Area",
					Priority = AnnouncementPriority.High,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				},
				new Announcement
				{
					Id = Guid.NewGuid(),
					Title = "Public WiFi Zones Expanded Throughout City",
					Description = "We're excited to announce the expansion of free public WiFi to 15 new locations including parks, libraries, and community centers. Access is free for all residents and requires simple registration.",
					Category = AnnouncementCategory.Announcement,
					Date = new DateTime(2025, 10, 18),
					Location = "15 locations citywide",
					Priority = AnnouncementPriority.Normal,
					CreatedAt = DateTime.Now,
					CreatedBy = "System"
				}
			};

			foreach (var announcement in defaultAnnouncements)
			{
				AddAnnouncement(announcement);
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Admin authentication service 
	public sealed class AdminAuthenticationService : IAdminAuthenticationService
	{
		private readonly Dictionary<string, string> _adminCredentials;

		//-----------------------------------------------------------------------
		public AdminAuthenticationService()
		{
			_adminCredentials = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string hashedPassword = HashPassword("Admin@123!");
			_adminCredentials.Add("admin", hashedPassword);
		}


		//-----------------------------------------------------------------------
		public bool ValidateCredentials(string username, string password)
		{
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				return false;
			}

			if (_adminCredentials.TryGetValue(username, out var storedHashedPassword))
			{
				return VerifyPassword(password, storedHashedPassword);
			}

			return false;
		}

		//-----------------------------------------------------------------------
		public bool UserExists(string username)  // check if admin user exists
		{
			return _adminCredentials.ContainsKey(username);
		}

		//-----------------------------------------------------------------------
		private static string HashPassword(string password)  // hash password using PBKDF2
		{
			byte[] salt = RandomNumberGenerator.GetBytes(16);

			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
			byte[] hash = pbkdf2.GetBytes(32);

			byte[] hashBytes = new byte[48];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 32);

			return Convert.ToBase64String(hashBytes);
		}

		//-----------------------------------------------------------------------
		private static bool VerifyPassword(string password, string storedHash)  // verify password against the stored hash
		{

			byte[] hashBytes = Convert.FromBase64String(storedHash);

			byte[] salt = new byte[16];
			Array.Copy(hashBytes, 0, salt, 0, 16);

			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
			byte[] hash = pbkdf2.GetBytes(32);

			for (int i = 0; i < 32; i++)
			{
				if (hashBytes[i + 16] != hash[i])
				{
					return false;
				}
			}

			return true;
		}
	}

	//-----------------------------------------------------------------------------
	// Recommendation service - analyzes user behavior and suggests relevant content
	public sealed class RecommendationService : IRecommendationService
	{
		private readonly IAnnouncementService _announcementService;		
		private readonly Dictionary<string, List<string>> _userSearchHistory;			
		private readonly Dictionary<string, Dictionary<AnnouncementCategory, int>> _userCategoryPreferences;		
		private readonly Dictionary<Guid, int> _announcementViewCounts;
		private readonly Dictionary<string, HashSet<Guid>> _userViewedAnnouncements;
		private readonly SortedDictionary<int, List<Guid>> _trendingAnnouncementsByViewCount;

		//-----------------------------------------------------------------------
		public RecommendationService(IAnnouncementService announcementService)
		{
			_announcementService = announcementService;
			_userSearchHistory = new Dictionary<string, List<string>>();
			_userCategoryPreferences = new Dictionary<string, Dictionary<AnnouncementCategory, int>>();
			_announcementViewCounts = new Dictionary<Guid, int>();
			_userViewedAnnouncements = new Dictionary<string, HashSet<Guid>>();
			_trendingAnnouncementsByViewCount = new SortedDictionary<int, List<Guid>>();
		}

		//-----------------------------------------------------------------------
		public void TrackSearch(string userId, string searchTerm, string? category)  // track user search activity
		{
			if (string.IsNullOrWhiteSpace(userId)) return;

			if (!string.IsNullOrWhiteSpace(searchTerm))
			{
				if (!_userSearchHistory.ContainsKey(userId))
				{
					_userSearchHistory[userId] = new List<string>();
				}
				_userSearchHistory[userId].Add(searchTerm.ToLower());
				

				if (_userSearchHistory[userId].Count > 20)
				{
					_userSearchHistory[userId].RemoveAt(0);
				}
			}

			// Track category preference
			if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<AnnouncementCategory>(category, out var categoryEnum))
			{
				if (!_userCategoryPreferences.ContainsKey(userId))
				{
					_userCategoryPreferences[userId] = new Dictionary<AnnouncementCategory, int>();
				}

				if (!_userCategoryPreferences[userId].ContainsKey(categoryEnum))
				{
					_userCategoryPreferences[userId][categoryEnum] = 0;
				}
				_userCategoryPreferences[userId][categoryEnum]++;
			}
		}

		//-----------------------------------------------------------------------
		public void TrackView(string userId, Announcement announcement)  // track announcement views
		{
			if (string.IsNullOrWhiteSpace(userId) || announcement == null) return;

			// Track view count for trending
			if (!_announcementViewCounts.ContainsKey(announcement.Id))
			{
				_announcementViewCounts[announcement.Id] = 0;
			}
			var oldCount = _announcementViewCounts[announcement.Id];
			_announcementViewCounts[announcement.Id]++;
			var newCount = _announcementViewCounts[announcement.Id];


			UpdateTrendingIndex(announcement.Id, oldCount, newCount);

			if (!_userViewedAnnouncements.ContainsKey(userId))
			{
				_userViewedAnnouncements[userId] = new HashSet<Guid>();
			}
			_userViewedAnnouncements[userId].Add(announcement.Id);

			if (!_userCategoryPreferences.ContainsKey(userId))
			{
				_userCategoryPreferences[userId] = new Dictionary<AnnouncementCategory, int>();
			}
			if (!_userCategoryPreferences[userId].ContainsKey(announcement.Category))
			{
				_userCategoryPreferences[userId][announcement.Category] = 0;
			}
			_userCategoryPreferences[userId][announcement.Category]++;
		}

		//-----------------------------------------------------------------------
		public List<Announcement> GetRecommendations(string userId, int count)  // get personalized recommendations
		{
			var allAnnouncements = _announcementService.GetAllAnnouncements();
			var scoredRecommendations = new List<(Announcement announcement, double score)>();

		
			if (_userCategoryPreferences.ContainsKey(userId) && _userCategoryPreferences[userId].Any())
			{
				var preferences = _userCategoryPreferences[userId];
				var maxPreference = preferences.Values.Max();
			
				foreach (var announcement in allAnnouncements)
				{
					double score = 0;
					
					if (preferences.ContainsKey(announcement.Category))
					{
					
						score += (preferences[announcement.Category] / (double)maxPreference) * 100;
					}

				
					if (announcement.IsFeatured)
					{
						score += 20;
					}
				
					score += (4 - (int)announcement.Priority) * 5;				
					score += GetViewCount(announcement.Id) * 2;
				
					if (announcement.Date >= DateTime.Now)
					{
						score += 10;
					}
				
					if (_userSearchHistory.ContainsKey(userId))
					{
						foreach (var searchTerm in _userSearchHistory[userId])
						{
							if (announcement.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
							    announcement.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
							{
								score += 15;
							}
						}
					}

					scoredRecommendations.Add((announcement, score));
				}

			
				return scoredRecommendations
					.OrderByDescending(r => r.score)
					.Select(r => r.announcement)
					.Take(count)
					.ToList();
			}
			else
			{
			
				var defaultRecs = new List<Announcement>();		
				defaultRecs.AddRange(GetTrendingAnnouncements(3));						
				defaultRecs.AddRange(_announcementService.GetUpcomingAnnouncements(3));
				
				return defaultRecs
					.Distinct()
					.OrderByDescending(a => a.IsFeatured)
					.ThenBy(a => a.Priority)
					.Take(count)
					.ToList();
			}
		}

		//-----------------------------------------------------------------------
		public List<Announcement> GetTrendingAnnouncements(int count)  // get trending announcements based on view counts
		{
			var trending = new List<Announcement>();

		
			foreach (var kvp in _trendingAnnouncementsByViewCount.Reverse())
			{
				foreach (var announcementId in kvp.Value)
				{
					var announcement = _announcementService.GetAnnouncementById(announcementId);
					if (announcement != null)
					{
						trending.Add(announcement);
						if (trending.Count >= count) return trending;
					}
				}
			}

		
			if (trending.Count < count)
			{
				var upcoming = _announcementService.GetUpcomingAnnouncements(count - trending.Count);
				trending.AddRange(upcoming.Where(u => !trending.Any(t => t.Id == u.Id)));
			}

			return trending.Take(count).ToList();
		}

		//-----------------------------------------------------------------------
		public List<Announcement> GetRelatedAnnouncements(Announcement announcement, int count)  // get similar announcements
		{
			if (announcement == null) return new List<Announcement>();

			var related = new HashSet<Announcement>();

		
			var sameCategory = _announcementService.GetAnnouncementsByCategory(announcement.Category)
				.Where(a => a.Id != announcement.Id)
				.Take(count);
			
			foreach (var ann in sameCategory)
			{
				related.Add(ann);
			}

		
			var startDate = announcement.Date.AddDays(-7);
			var endDate = announcement.Date.AddDays(7);
			var similarDate = _announcementService.GetAnnouncementsByDateRange(startDate, endDate)
				.Where(a => a.Id != announcement.Id)
				.Take(count / 2);
			
			foreach (var ann in similarDate)
			{
				related.Add(ann);
			}

			if (!string.IsNullOrWhiteSpace(announcement.Location))
			{
				var sameLocation = _announcementService.GetAllAnnouncements()
					.Where(a => a.Id != announcement.Id && 
					           !string.IsNullOrWhiteSpace(a.Location) &&
					           a.Location.Contains(announcement.Location, StringComparison.OrdinalIgnoreCase))
					.Take(count / 3);
				
				foreach (var ann in sameLocation)
				{
					related.Add(ann);
				}
			}

			var similarPriority = _announcementService.GetAllAnnouncements()
				.Where(a => a.Id != announcement.Id && 
				           (a.Priority == announcement.Priority || 
				            (a.IsFeatured && announcement.IsFeatured)))
				.Take(count / 3);
			
			foreach (var ann in similarPriority)
			{
				related.Add(ann);
			}

			return related
				.OrderByDescending(a => GetRelevanceScore(announcement, a))
				.Take(count)
				.ToList();
		}

		//-----------------------------------------------------------------------
		public Dictionary<AnnouncementCategory, int> GetUserPreferences(string userId)  // get user category preferences
		{
			if (_userCategoryPreferences.ContainsKey(userId))
			{
				return new Dictionary<AnnouncementCategory, int>(_userCategoryPreferences[userId]);
			}
			return new Dictionary<AnnouncementCategory, int>();
		}

		//-----------------------------------------------------------------------
		private void UpdateTrendingIndex(Guid announcementId, int oldCount, int newCount)  // update trending data structure
		{
	
			if (_trendingAnnouncementsByViewCount.ContainsKey(oldCount))
			{
				_trendingAnnouncementsByViewCount[oldCount].Remove(announcementId);
				if (_trendingAnnouncementsByViewCount[oldCount].Count == 0)
				{
					_trendingAnnouncementsByViewCount.Remove(oldCount);
				}
			}


			if (!_trendingAnnouncementsByViewCount.ContainsKey(newCount))
			{
				_trendingAnnouncementsByViewCount[newCount] = new List<Guid>();
			}
			_trendingAnnouncementsByViewCount[newCount].Add(announcementId);
		}

		//-----------------------------------------------------------------------
		private bool IsAlreadyViewed(string userId, Guid announcementId)  // check if user already viewed announcement
		{
			return _userViewedAnnouncements.ContainsKey(userId) && 
			       _userViewedAnnouncements[userId].Contains(announcementId);
		}

		//-----------------------------------------------------------------------
		private int GetViewCount(Guid announcementId)  // get view count for announcement
		{
			return _announcementViewCounts.ContainsKey(announcementId) ? _announcementViewCounts[announcementId] : 0;
		}

		//-----------------------------------------------------------------------
		private double GetRelevanceScore(Announcement source, Announcement target)  // calculate relevance score between announcements
		{
			double score = 0;

			// Same category = +10 points
			if (source.Category == target.Category) score += 10;

			// Same priority = +5 points
			if (source.Priority == target.Priority) score += 5;

			// Both featured = +5 points
			if (source.IsFeatured && target.IsFeatured) score += 5;

			// Date proximity (within 7 days) = +3 points
			var daysDiff = Math.Abs((source.Date - target.Date).TotalDays);
			if (daysDiff <= 7) score += 3;

			// Location match = +4 points
			if (!string.IsNullOrWhiteSpace(source.Location) && 
			    !string.IsNullOrWhiteSpace(target.Location) &&
			    source.Location.Equals(target.Location, StringComparison.OrdinalIgnoreCase))
			{
				score += 4;
			}

			
			score += GetViewCount(target.Id) * 0.1;

			return score;
		}
	}

	//-----------------------------------------------------------------------------
	// User service - manages user authentication and accounts
	public sealed class UserService : IUserService
	{
		private readonly Dictionary<Guid, User> _usersById;
		private readonly Dictionary<string, User> _usersByUsername;
		private readonly Dictionary<string, User> _usersByEmail;

		//-----------------------------------------------------------------------
		public UserService()
		{
			_usersById = new Dictionary<Guid, User>();
			_usersByUsername = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
			_usersByEmail = new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);
		}

		//-----------------------------------------------------------------------
		public async Task<User?> RegisterAsync(UserRegisterViewModel model)  // register new user
		{
			// Validate unique constraints
			if (UsernameExists(model.Username))
				return null;
			
			if (EmailExists(model.Email))
				return null;

			// Create new user
			var user = new User
			{
				Id = Guid.NewGuid(),
				Username = model.Username,
				Email = model.Email,
				PasswordHash = HashPassword(model.Password),
				FirstName = model.FirstName,
				LastName = model.LastName,
				CreatedAt = DateTime.Now,
				IsActive = true
			};

			// Add to storage
			_usersById[user.Id] = user;
			_usersByUsername[user.Username] = user;
			_usersByEmail[user.Email] = user;

			return await Task.FromResult(user);
		}

		//-----------------------------------------------------------------------
		public async Task<User?> LoginAsync(string usernameOrEmail, string password)  // authenticate user
		{
			if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
				return null;

			// Find user by username or email
			User? user = null;
			if (_usersByUsername.ContainsKey(usernameOrEmail))
			{
				user = _usersByUsername[usernameOrEmail];
			}
			else if (_usersByEmail.ContainsKey(usernameOrEmail))
			{
				user = _usersByEmail[usernameOrEmail];
			}

			// Verify password and active status
			if (user != null && user.IsActive && VerifyPassword(password, user.PasswordHash))
			{
				UpdateLastLogin(user.Id);
				return await Task.FromResult(user);
			}

			return null;
		}

		//-----------------------------------------------------------------------
		public User? GetUserById(Guid userId)  // get user by ID
		{
			return _usersById.ContainsKey(userId) ? _usersById[userId] : null;
		}

		//-----------------------------------------------------------------------
		public User? GetUserByUsername(string username)  // get user by username
		{
			return _usersByUsername.ContainsKey(username) ? _usersByUsername[username] : null;
		}

		//-----------------------------------------------------------------------
		public User? GetUserByEmail(string email)  // get user by email
		{
			return _usersByEmail.ContainsKey(email) ? _usersByEmail[email] : null;
		}

		//-----------------------------------------------------------------------
		public bool UsernameExists(string username)  // check if username exists
		{
			return _usersByUsername.ContainsKey(username);
		}

		//-----------------------------------------------------------------------
		public bool EmailExists(string email)  // check if email exists
		{
			return _usersByEmail.ContainsKey(email);
		}

		//-----------------------------------------------------------------------
		public void UpdateLastLogin(Guid userId)  // update last login timestamp
		{
			if (_usersById.ContainsKey(userId))
			{
				_usersById[userId].LastLoginAt = DateTime.Now;
			}
		}

		//-----------------------------------------------------------------------
		private static string HashPassword(string password)  // hash password using PBKDF2
		{
			byte[] salt = RandomNumberGenerator.GetBytes(16);
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
			byte[] hash = pbkdf2.GetBytes(32);
			byte[] hashBytes = new byte[48];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 32);
			return Convert.ToBase64String(hashBytes);
		}

		//-----------------------------------------------------------------------
		private static bool VerifyPassword(string password, string storedHash)  // verify password against stored hash
		{
			byte[] hashBytes = Convert.FromBase64String(storedHash);
			byte[] salt = new byte[16];
			Array.Copy(hashBytes, 0, salt, 0, 16);
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
			byte[] hash = pbkdf2.GetBytes(32);
			for (int i = 0; i < 32; i++)
			{
				if (hashBytes[i + 16] != hash[i])
					return false;
			}
			return true;
		}
	}
}



//-------------------------------------------<<< End of File >>>---------------------------------------------