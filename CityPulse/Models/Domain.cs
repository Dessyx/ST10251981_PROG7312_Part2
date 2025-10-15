using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CityPulse.Models   
{
	//-----------------------------------------------------------------------
	// Different types of issues citizens can report
	public enum IssueCategory
	{
		Sanitation,
		Roads,
		Utilities,
		Water,
		Electricity,
		Other
	}

    //-----------------------------------------------------------------------
	// A single issue report submitted by a citizen
	public sealed class IssueReport
	{
		[Required]
		public string ReferenceNumber { get; set; } = string.Empty; // So users can track their progress later

		[Required]
		[StringLength(256)]
		public string Location { get; set; } = string.Empty;  // location of user

		[Required]
		public IssueCategory Category { get; set; }  // Categeory of issue

		[Required]
		[StringLength(4000)]
		public string Description { get; set; } = string.Empty; // optional information about issue

		public DoublyLinkedList<Attachment> Attachments { get; } = new DoublyLinkedList<Attachment>(); // images or files attached

		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow; // time created
	}

	// Info about files attached to reports (images, documents, etc.)
	public sealed class Attachment
	{
		[Required]
		public string FileName { get; set; } = string.Empty;
		[Required]
		public string ContentType { get; set; } = string.Empty;
		[Required]
		public string StoredPath { get; set; } = string.Empty;
		public long LengthBytes { get; set; }
	}

	// What I need from the user when they submit a new report
	public sealed class IssueReportCreateRequest
	{
		[Required]
		[StringLength(256)]
		public string Location { get; set; } = string.Empty;

		[Required]
		public IssueCategory Category { get; set; }

		[Required]
		[StringLength(4000)]
		public string Description { get; set; } = string.Empty;


		public CityPulse.Models.Queue<IFormFile> UploadQueue { get; } = new CityPulse.Models.Queue<IFormFile>();
	}


	//-----------------------------------------------------------------------
	// Single node in the doubly linked list
	public sealed class DoublyLinkedListNode<T>
	{
		public T Value { get; }
		public DoublyLinkedListNode<T>? Previous { get; internal set; }
		public DoublyLinkedListNode<T>? Next { get; internal set; }

		public DoublyLinkedListNode(T value)
		{
			Value = value;
		}
	}

	// My own doubly linked list (lets us go forward and backward through items)
	public sealed class DoublyLinkedList<T>
	{
		public DoublyLinkedListNode<T>? Head { get; private set; }
		public DoublyLinkedListNode<T>? Tail { get; private set; }
		public int Count { get; private set; }

		public DoublyLinkedListNode<T> AddLast(T value)
		{
			var node = new DoublyLinkedListNode<T>(value);
			if (Tail == null)
			{
				Head = Tail = node;
			}
			else
			{
				node.Previous = Tail;
				Tail.Next = node;
				Tail = node;
			}
			Count++;
			return node;
		}

		public bool Remove(DoublyLinkedListNode<T> node)
		{
			if (node == null) return false;
			if (node.Previous != null) node.Previous.Next = node.Next; else Head = node.Next;
			if (node.Next != null) node.Next.Previous = node.Previous; else Tail = node.Previous;
			Count--;
			return true;
		}
	}


	public sealed class QueueNode<T>
	{
		public T Value { get; }
		public QueueNode<T>? Next { get; internal set; }
		public QueueNode(T value) { Value = value; }
	}

	// queue (first in, first out)
	public sealed class Queue<T>
	{
		private QueueNode<T>? _head;
		private QueueNode<T>? _tail;
		public int Count { get; private set; }

		public void Enqueue(T item)
		{
			var node = new QueueNode<T>(item);
			if (_tail == null)
			{
				_head = _tail = node;
			}
			else
			{
				_tail.Next = node;
				_tail = node;
			}
			Count++;
		}

		public bool TryDequeue(out T? item)
		{
			if (_head == null) { item = default; return false; }
			item = _head.Value;
			_head = _head.Next;
			if (_head == null) _tail = null;
			Count--;
			return true;
		}
	}


	//-----------------------------------------------------------------------	
	// Types of announcements that can be posted
	public enum AnnouncementCategory
	{
		Announcement,
		Event,
		ServiceUpdate,
		Notice,
		Program,
		Emergency
	}

	// How important an announcement is
	public enum AnnouncementPriority
	{
		Low = 3,
		Normal = 2,
		High = 1,
		Critical = 0
	}

	// A single announcement or event posted by admins
	public sealed class Announcement
	{
		public Guid Id { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public AnnouncementCategory Category { get; set; }
		public DateTime Date { get; set; }
		public string? Location { get; set; }
		public string? Duration { get; set; }
		public string? AgeGroup { get; set; }
		public string? AffectedAreas { get; set; }
		public string? ContactInfo { get; set; }
		public bool IsFeatured { get; set; }
		public AnnouncementPriority Priority { get; set; }
		public DateTime CreatedAt { get; set; }
		public string CreatedBy { get; set; } = string.Empty;
	}

	//-----------------------------------------------------------------------	
	// Form for admins to log in
	public sealed class AdminLoginViewModel
	{
		[Required]
		[Display(Name = "Username")]
		public string Username { get; set; } = string.Empty;

		[Required]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = string.Empty;
	}

	// Admin dashboard 
	public sealed class AdminDashboardViewModel
	{
		public int TotalAnnouncements { get; set; }
		public List<Announcement> RecentAnnouncements { get; set; } = new();
	}

	// Form for admins to create new announcements
	public sealed class AnnouncementViewModel
	{
		[Required]
		[StringLength(200)]
		[Display(Name = "Title")]
		public string Title { get; set; } = string.Empty;

		[Required]
		[StringLength(2000)]
		[Display(Name = "Description")]
		public string Description { get; set; } = string.Empty;

		[Required]
		[Display(Name = "Category")]
		public AnnouncementCategory Category { get; set; }

		[Required]
		[Display(Name = "Date")]
		public DateTime Date { get; set; } = DateTime.Now;

		[StringLength(200)]
		[Display(Name = "Location")]
		public string? Location { get; set; }

		[StringLength(100)]
		[Display(Name = "Duration")]
		public string? Duration { get; set; }

		[StringLength(100)]
		[Display(Name = "Age Group")]
		public string? AgeGroup { get; set; }

		[StringLength(500)]
		[Display(Name = "Affected Areas")]
		public string? AffectedAreas { get; set; }

		[StringLength(200)]
		[Display(Name = "Contact Information")]
		public string? ContactInfo { get; set; }

		[Display(Name = "Featured")]
		public bool IsFeatured { get; set; }

		[Display(Name = "Priority")]
		public AnnouncementPriority Priority { get; set; } = AnnouncementPriority.Normal;
	}

	//-----------------------------------------------------------------------
	// A registered user account
	public sealed class User
	{
		public Guid Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLoginAt { get; set; }
		public bool IsActive { get; set; } = true;
	}

	// Form for users to log in
	public sealed class UserLoginViewModel
	{
		[Required(ErrorMessage = "Username or email is required")]
		[Display(Name = "Username or Email")]
		public string UsernameOrEmail { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required")]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = string.Empty;

		[Display(Name = "Remember me")]
		public bool RememberMe { get; set; }
	}

	// Form for users to create an account
	public sealed class UserRegisterViewModel
	{
		[Required(ErrorMessage = "Username is required")]
		[StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
		[Display(Name = "Username")]
		public string Username { get; set; } = string.Empty;

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email address")]
		[Display(Name = "Email")]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "Password is required")]
		[StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; } = string.Empty;

		[Required(ErrorMessage = "Please confirm your password")]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm Password")]
		[Compare("Password", ErrorMessage = "Passwords do not match")]
		public string ConfirmPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "First name is required")]
		[StringLength(50)]
		[Display(Name = "First Name")]
		public string FirstName { get; set; } = string.Empty;

		[Required(ErrorMessage = "Last name is required")]
		[StringLength(50)]
		[Display(Name = "Last Name")]
		public string LastName { get; set; } = string.Empty;
	}

	// Helps track when users view announcements (for recommendations)
	public sealed class TrackViewModel
	{
		public Guid AnnouncementId { get; set; }
	}
}

//---------------------------------------------------------- <<< End of File >>>--------------------------------
