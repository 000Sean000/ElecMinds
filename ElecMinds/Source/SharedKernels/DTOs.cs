
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#region Dependency

using Config;
using Enums;
using NoteTaking.Domain;
#endregion

// variable name should be the same for mapping
namespace DTOs
{
	#region Follow DTO Interfaces to ensure successful mapping between DTO and Domain Object instance
	public interface INodeDTO<TNoteDTO, TNoteSegmentDTO>
		where TNoteDTO : INoteDTO<TNoteSegmentDTO>
		where TNoteSegmentDTO : INoteSegmentDTO
	{

		public TID? Id { get; set; }
		public ENeuronClass? NeuronClass { get; set; }
		public string? ImagePath { get; set; }

		#region Aggregate Members
		public TNoteDTO? NoteData { get; protected set; }
		public List<TID>? OutLinkIDs { get; set; }
		public List<TID>? InLinkIDs { get; set; }
		public List<TID>? OutReferenceIDs { get; set; }
		public List<TID>? InReferenceIDs { get; set; }
		#endregion
	}

	public interface INeuronDTO<TNoteDTO, TLinkDTO, TReferenceDTO, TNoteSegmentDTO> 
		where TNoteDTO: INoteDTO<TNoteSegmentDTO> 
		where TLinkDTO : ILinkDTO
		where TReferenceDTO : IReferenceDTO
		where TNoteSegmentDTO : INoteSegmentDTO
	{

		public TID? Id { get; set; }
		public ENeuronClass? NeuronClass { get; set; }
		public string? ImagePath { get; set; }

		#region Aggregate Members
		public TNoteDTO? NoteData { get; protected set; }
		public List<TLinkDTO>? OutLinkData { get; set; }
		public List<TID>? InLinkIds { get; set; }
		public List<TReferenceDTO>? OutReferenceData { get; set; }
		public List<TID>? InReferenceIds { get; set; }
		#endregion
	}
	

	public interface INoteSegmentDTO
	{
		public string? Text { get; set; }
		public TID? ReferenceId { get; set; }
	}
	public interface INoteDTO<TNoteSegmentDTO> where TNoteSegmentDTO : INoteSegmentDTO
	{
		public ENoteComposition? Composition { get; set; }
		public ENoteImportance? Importance { get; set; }
		public List<TNoteSegmentDTO>? Segments { get; set; }
	}
	public interface ILinkDTO
	{
		public TID? Id { get; set; }
		public TID? SourceNeuronId { get; set; } // will be used by target neuron to check back
		public TID? TargetNeuronId { get; set; }
		public ELinkType? LinkType { get; set; }
		public Dictionary<ELinkInfoIndex, string>? LinkInfo { get; set; }
	}
	public interface IReferenceDTO
	{
		public TID? Id { get; set; }
		public TID? SourceNeuronId { get; set; } // will be used by target neuron to check back
		public TID? TargetNeuronId { get; set; }
		public EDereferencerType? DereferencerType { get; set; }

	}
	#endregion

	#region Implementations
	public class NeuronDTO: INeuronDTO<NoteDTO, LinkDTO, ReferenceDTO, NoteSegmentDTO> 
	{

		public TID? Id { get; set; }
		public ENeuronClass? NeuronClass { get; set; }
		public string? ImagePath { get; set; }

		#region Aggregate Members
		public NoteDTO? NoteData { get; set; }
		public List<LinkDTO>? OutLinkData { get; set; }
		public List<TID>? InLinkIds { get; set; }
		public List<ReferenceDTO>? OutReferenceData { get; set; }
		public List<TID>? InReferenceIds { get; set; }
		#endregion
	}
	public class NodeDTO : INodeDTO<NoteDTO, NoteSegmentDTO>
	{

		public TID? Id { get; set; }
		public ENeuronClass? NeuronClass { get; set; }
		public string? ImagePath { get; set; }

		#region Aggregate Members
		public NoteDTO? NoteData { get; set; }
		public List<TID>? OutLinkIDs { get; set; }
		public List<TID>? InLinkIDs { get; set; }
		public List<TID>? OutReferenceIDs { get; set; }
		public List<TID>? InReferenceIDs { get; set; }
		#endregion
	}
	public class NoteSegmentDTO: INoteSegmentDTO
	{
		public string? Text { get; set; }
		public TID? ReferenceId { get; set; }
	}
	public class NoteDTO:INoteDTO<NoteSegmentDTO> 
	{
		public ENoteComposition? Composition { get; set; }
		public ENoteImportance? Importance { get; set; }
		public List<NoteSegmentDTO>? Segments { get; set; }
	}
	public class LinkDTO: ILinkDTO
	{
		public TID? Id { get; set; }
		public TID? SourceNeuronId { get; set; } // will be used by target neuron to check back
		public TID? TargetNeuronId { get; set; }
		public ELinkType? LinkType { get; set; }
		public Dictionary<ELinkInfoIndex, string>? LinkInfo { get; set; }
	}
	public class ReferenceDTO:IReferenceDTO
	{
		public TID? Id { get; set; }
		public TID? SourceNeuronId { get; set; } // will be used by target neuron to check back
		public TID? TargetNeuronId { get; set; }
		public EDereferencerType? DereferencerType { get; set; }

	}
	#endregion
}



