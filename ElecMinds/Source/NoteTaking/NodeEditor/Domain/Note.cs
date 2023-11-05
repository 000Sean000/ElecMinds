using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;

#region Dependency
using Enums;
#endregion

namespace NoteTaking.Domain
{
	public class NoteSegment: INoteSegmentDTO
	{
		public string? Text { get; set; }
		public Guid? ReferenceId { get; set; }
		public NoteSegment()
		{

		}
		public NoteSegment(Guid referenceId)
		{
			ReferenceId = referenceId;
		}
		public NoteSegment(string text)
		{
			Text = text;
		}
		public NoteSegment(NoteSegment segment)
		{
			Text = segment.Text;
			ReferenceId = segment.ReferenceId;
		}

		public NoteSegment DeepCopy()
		{
			NoteSegment segment = new NoteSegment()
			{
				Text = this.Text,
				ReferenceId = this.ReferenceId
			};
			return segment;
		}

	}

	public class NoteData: INoteDTO<NoteSegment>
	{
		public ENoteComposition? Composition { get; set; }
		public ENoteImportance? Importance { get; set; }
		public List<NoteSegment>? Segments { get; set; }

		
		public NoteData() { } 
		public NoteData(NoteData? noteData)
		{
			if (noteData != null)
			{
				Overwrite(noteData);
				Segments = noteData.Segments;
			}
		}
		public void Write(NoteData noteData)
		{
			///noteData = noteData.DeepCopy(); 
			if (noteData.Composition != null)
			{
				Composition = noteData.Composition;
			}
			if (noteData.Importance != null)
			{
				Importance = noteData.Importance;
			}
			if (noteData.Segments != null)
			{
				Segments = noteData.Segments;
			}
		}
		protected void Overwrite(NoteData noteData)
		{
			///noteData = noteData.DeepCopy();
			Composition = noteData.Composition;
			Importance = noteData.Importance;
			Segments = noteData.Segments;
		}

		public NoteData Read()
		{
			return DeepCopy();
		}
		public NoteData DeepCopy()
		{
			NoteData noteData = new NoteData();

			noteData.Composition = Composition;
			noteData.Importance = Importance;
			if (Segments == null)
			{
				noteData.Segments = null;
			}
			else
			{
				noteData.Segments = new List<NoteSegment>();
				foreach (var segment in Segments)
				{
					noteData.Segments.Add(segment.DeepCopy());
				}
			}
			return noteData;
		}
		
	}

	public class Note: NoteData
	{
		public Note() : base()
		{
			EnsurePropertyNotNull();
		}
		public Note(NoteData? noteData):base(noteData)
		{
			EnsurePropertyNotNull();
		}
		public void EnsurePropertyNotNull()
		{
			if (Composition == null)
			{
				Composition = default(ENoteComposition);
			}
			if (Importance == null)
			{
				Importance = default(ENoteImportance);
			}
			if (Segments == null)
			{
				Segments = new List<NoteSegment>();
				Segments.Add(new NoteSegment(""));
			}
		}
		
		// Dereference need to be done in application service
		public void ExpireAllDereference()
		{
			foreach (var seg in Segments)
			{
				if (seg.ReferenceId != null)
				{
					seg.Text = null;
				}
			}
		}
		public void ExpireDereference(Guid referenceId)
		{
			foreach (var seg in Segments)
			{
				if (seg.ReferenceId == referenceId)
				{
					seg.Text = null;
					return;
				}
			}
		}

	}
}
