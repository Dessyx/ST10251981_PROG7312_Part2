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

// ---------------------------------------- Implementation -------------------------------------------------
namespace CityPulse.Services
{

	//-----------------------------------------------------------------------------
	// Generate reference numbers for submitted reports
	public sealed class ReferenceNumberService : IReferenceNumberService
	{
		public string CreateReference()
		{
		
			var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			var randomPart = Random.Shared.Next(1000, 9999);
			return $"CP-{datePart}-{randomPart}";
		}
	}


    //-----------------------------------------------------------------------------
	// Storing images an ddocuments
    public sealed class LocalStorageService : IStorageService
	{
		private readonly string _root;
		private static readonly long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
		private static readonly Regex SafeName = new Regex("[^a-zA-Z0-9_.-]", RegexOptions.Compiled);

		public LocalStorageService(IWebHostEnvironment env)
		{
			_root = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
			Directory.CreateDirectory(_root);
		}

		public async Task<Attachment> SaveAsync(IFormFile file)
		{
			if (file == null || file.Length == 0) throw new InvalidOperationException("Empty file."); // Conditions
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

		public IssueReportingService(IReferenceNumberService referenceNumberService, IStorageService storageService)
		{
			_referenceNumberService = referenceNumberService;
			_storageService = storageService;
		}

		public async Task<IssueReport> CreateAsync(IssueReportCreateRequest request)
		{
			if (request == null) throw new ArgumentNullException(nameof(request));
			var report = new IssueReport
			{
				ReferenceNumber = _referenceNumberService.CreateReference(),
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


        //-----------------------------------------------------------------------------
		// Preset posible locations 
        public CityPulse.Models.Queue<string> GetLocationSuggestions(string query)
		{
			var results = new CityPulse.Models.Queue<string>();
			if (string.IsNullOrWhiteSpace(query)) return results;

			
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

		public AnnouncementService()
		{
			_announcementsByDate = new SortedDictionary<DateTime, List<Announcement>>();
			_announcementsByCategory = new Dictionary<AnnouncementCategory, List<Announcement>>();
			_announcementsById = new Dictionary<Guid, Announcement>();
			_searchIndex = new Dictionary<string, HashSet<Guid>>();
			_uniqueCategories = new HashSet<string>();
			_uniqueDates = new HashSet<DateTime>();
			_priorityQueue = new PriorityQueue<Announcement, int>();
			_recentlyViewed = new Stack<Announcement>();
			_pendingAnnouncements = new System.Collections.Generic.Queue<Announcement>();
			
			// Initialize category lists
			foreach (AnnouncementCategory category in Enum.GetValues<AnnouncementCategory>())
			{
				_announcementsByCategory[category] = new List<Announcement>();
				_uniqueCategories.Add(category.ToString());
			}
			
			SeedDefaultData();
		}

		public void AddAnnouncement(Announcement announcement)
		{
			// Add to primary storage
			if (!_announcementsByDate.ContainsKey(announcement.Date.Date))
			{
				_announcementsByDate[announcement.Date.Date] = new List<Announcement>();
			}
			_announcementsByDate[announcement.Date.Date].Add(announcement);
			
			// Add to category index
			_announcementsByCategory[announcement.Category].Add(announcement);
			
			// Add to ID index
			_announcementsById[announcement.Id] = announcement;
			
			// Add to search index
			AddToSearchIndex(announcement);
			
			// Add to sets
			_uniqueDates.Add(announcement.Date.Date);
			
			// Add to priority queue if featured or high priority
			if (announcement.IsFeatured || announcement.Priority <= AnnouncementPriority.High)
			{
				_priorityQueue.Enqueue(announcement, (int)announcement.Priority);
			}
		}

		public List<Announcement> GetAllAnnouncements()
		{
			var allAnnouncements = new List<Announcement>();
			foreach (var dateGroup in _announcementsByDate.Values)
			{
				allAnnouncements.AddRange(dateGroup);
			}
			return allAnnouncements.OrderByDescending(a => a.Date).ToList();
		}

		public List<Announcement> GetAnnouncementsByCategory(AnnouncementCategory category)
		{
			return _announcementsByCategory[category].OrderByDescending(a => a.Date).ToList();
		}

		public List<Announcement> GetAnnouncementsByDateRange(DateTime startDate, DateTime endDate)
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

		public List<Announcement> GetRecentAnnouncements(int count)
		{
			return GetAllAnnouncements().Take(count).ToList();
		}

		public List<Announcement> GetFeaturedAnnouncements()
		{
			var featured = new List<Announcement>();
			var tempQueue = new PriorityQueue<Announcement, int>();
			
			// Copy priority queue to avoid modifying original
			while (_priorityQueue.Count > 0)
			{
				var announcement = _priorityQueue.Dequeue();
				featured.Add(announcement);
				tempQueue.Enqueue(announcement, (int)announcement.Priority);
			}
			
			// Restore priority queue
			while (tempQueue.Count > 0)
			{
				var announcement = tempQueue.Dequeue();
				_priorityQueue.Enqueue(announcement, (int)announcement.Priority);
			}
			
			return featured.OrderBy(a => a.Priority).ToList();
		}

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

		public Announcement? GetAnnouncementById(Guid id)
		{
			return _announcementsById.ContainsKey(id) ? _announcementsById[id] : null;
		}

		public HashSet<string> GetUniqueCategories()
		{
			return new HashSet<string>(_uniqueCategories);
		}

		public HashSet<DateTime> GetUniqueDates()
		{
			return new HashSet<DateTime>(_uniqueDates);
		}

		private void AddToSearchIndex(Announcement announcement)
		{
			var text = $"{announcement.Title} {announcement.Description}".ToLower();
			var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			
			foreach (var word in words)
			{
				var cleanWord = new string(word.Where(c => char.IsLetterOrDigit(c)).ToArray());
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

		public void SeedDefaultData()
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
				}
			};

			foreach (var announcement in defaultAnnouncements)
			{
				AddAnnouncement(announcement);
			}
		}
	}


	public sealed class AdminAuthenticationService : IAdminAuthenticationService
	{
		
		private readonly Dictionary<string, string> _adminCredentials;

		public AdminAuthenticationService()
		{
			_adminCredentials = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			string hashedPassword = HashPassword("Admin@123!");
			_adminCredentials.Add("admin", hashedPassword);
		}


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

		public bool UserExists(string username)
		{
			return _adminCredentials.ContainsKey(username);
		}

	
		private static string HashPassword(string password)
		{
			// Generate a random salt
			byte[] salt = RandomNumberGenerator.GetBytes(16);

			// Hash the password with PBKDF2
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
			byte[] hash = pbkdf2.GetBytes(32);

			// Combine salt and hash
			byte[] hashBytes = new byte[48];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 32);

			// Convert to base64 for storage
			return Convert.ToBase64String(hashBytes);
		}

		
		private static bool VerifyPassword(string password, string storedHash)
		{
			// Extract the bytes
			byte[] hashBytes = Convert.FromBase64String(storedHash);

			// Get the salt
			byte[] salt = new byte[16];
			Array.Copy(hashBytes, 0, salt, 0, 16);

			// Hash the input password with the same salt
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
			byte[] hash = pbkdf2.GetBytes(32);

			// Compare the results
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
}



//-------------------------------------------<<< End of File >>>---------------------------------------------