using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using CityPulse.Models;
using CityPulse.Services.Abstractions;

namespace CityPulse.Services
{
	public sealed class ReferenceNumberService : IReferenceNumberService
	{
		public string CreateReference()
		{
			
			var datePart = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
			var randomPart = Random.Shared.Next(1000, 9999);
			return $"CP-{datePart}-{randomPart}";
		}
	}

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

		public CityPulse.Models.Queue<string> GetLocationSuggestions(string query)
		{
			var results = new CityPulse.Models.Queue<string>();
			if (string.IsNullOrWhiteSpace(query)) return results;

			// For now
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
}


