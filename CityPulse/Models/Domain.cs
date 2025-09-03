using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CityPulse.Models
{
	public enum IssueCategory
	{
		Sanitation,
		Roads,
		Utilities,
		Water,
		Electricity,
		Other
	}

	public sealed class IssueReport
	{
		[Required]
		public string ReferenceNumber { get; set; } = string.Empty;

		[Required]
		[StringLength(256)]
		public string Location { get; set; } = string.Empty;

		[Required]
		public IssueCategory Category { get; set; }

		[Required]
		[StringLength(4000)]
		public string Description { get; set; } = string.Empty;

		public DoublyLinkedList<Attachment> Attachments { get; } = new DoublyLinkedList<Attachment>();

		public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
	}

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

		// Queue to stage uploads before persisting
		public CityPulse.Models.Queue<IFormFile> UploadQueue { get; } = new CityPulse.Models.Queue<IFormFile>();
	}

	// Simple node for doubly linked list
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

	// Minimal doubly linked list implementation (no arrays/lists)
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

	// Simple queue implemented via linked nodes
	public sealed class QueueNode<T>
	{
		public T Value { get; }
		public QueueNode<T>? Next { get; internal set; }
		public QueueNode(T value) { Value = value; }
	}

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
}


