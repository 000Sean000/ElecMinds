using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs;
#region Dependency

using Config;
using Enums;
using Config;
using System.ComponentModel.DataAnnotations.Schema;
#endregion

namespace NoteTaking.Domain
{
	
	public class NeuronData: INeuronDTO<NoteData, LinkData, ReferenceData, NoteSegment>
	{
		
		public TID? Id { get; set; }
		public ENeuronClass? NeuronClass { get; set; }
		public string? ImagePath { get; set; }

		#region Aggregate Members
		public virtual NoteData? NoteData { get; set; }

		// virtual for be overrided to fetch data from fresh instances
		public virtual List<LinkData>? OutLinkData { get; set; }
		public virtual List<TID>? OutLinkIds
		{
			get
			{
				if (OutLinkData == null)
				{
					return null;
				}
				else
				{
					return OutLinkData.Select(link => (TID)link.Id).ToList();
				}
			}
		}
		public List<TID>? InLinkIds { get; set; } 
		public virtual List<ReferenceData>? OutReferenceData { get; set; }
		public List<TID>? OutReferenceIds
		{
			get
			{
				if (OutReferenceData == null)
				{
					return null;
				}
				else
				{
					return OutReferenceData.Select(reference => (TID)reference.Id).ToList();	
				}
			}
		}
		public List<TID>? InReferenceIds { get; set; } 
		#endregion

		public NeuronData() { }
		public NeuronData(NeuronData neuronData)
		{
			Overwrite(neuronData);
		}

		public void Write(NeuronData neuronData)
		{
			///neuronData = neuronData.DeepCopy();
			if (neuronData.Id != null)
			{
				Id = neuronData.Id;
			}
			if (neuronData.NeuronClass != null)
			{
				NeuronClass = neuronData.NeuronClass;
			}
			if (neuronData.ImagePath != null)
			{
				ImagePath = neuronData.ImagePath;
			}
			// don't write member object here
		}
		protected void Overwrite(NeuronData neuronData)
		{
			///neuronData = neuronData.DeepCopy();
			Id = neuronData.Id;
			NeuronClass = neuronData.NeuronClass;
			ImagePath = neuronData.ImagePath;

			NoteData = neuronData.NoteData;
			OutLinkData = neuronData.OutLinkData;
			InLinkIds = neuronData.InLinkIds;
			OutReferenceData = neuronData.OutReferenceData;
			InReferenceIds = neuronData.InReferenceIds;
		}
		public NeuronData Read()
		{
			return DeepCopy();
		}
		public NeuronData DeepCopy()
		{
			NeuronData neuronData = new NeuronData();

			neuronData.Id = Id;
			neuronData.NeuronClass = NeuronClass;
			neuronData.ImagePath = ImagePath;
			neuronData.NoteData = NoteData?.DeepCopy();
			if (OutLinkData ==  null)
			{
				neuronData.OutLinkData = null;
			}
			else
			{
				neuronData.OutLinkData = new List<LinkData>();
				foreach (var outLinkData in OutLinkData)
				{
					neuronData.OutLinkData.Add(outLinkData.DeepCopy());
				}
			}
			if (InLinkIds == null)
			{
				neuronData.InLinkIds = null;
			}
			else
			{
				neuronData.InLinkIds = new List<TID>(InLinkIds);
			}
			if (OutReferenceData == null)
			{
				neuronData.OutReferenceData = null;
			}
			else
			{
				neuronData.OutReferenceData = new List<ReferenceData>();
				foreach (var outReferenceData in OutReferenceData)
				{
					neuronData.OutReferenceData.Add(outReferenceData.DeepCopy());
				}
			}
			if (InReferenceIds == null)
			{
				neuronData.InReferenceIds = null;
			}
			else
			{
				neuronData.InReferenceIds = new List<TID>(InReferenceIds);
			}
			return neuronData;
		}
		public NodeData ToNodeData()
		{
			NodeData nodeData = new NodeData();

			nodeData.Id = Id;
			nodeData.NeuronClass = NeuronClass;
			nodeData.ImagePath = ImagePath;
			nodeData.NoteData = NoteData?.DeepCopy();
			nodeData.OutLinkIDs = new List<TID>();
			if (OutLinkData != null)
			{
				foreach (var outLinkData in OutLinkData) { nodeData.OutLinkIDs.Add((TID)outLinkData.Id); }
				
			}
			nodeData.InLinkIDs = new List<TID>();
			if (InLinkIds != null)
			{
				nodeData.InLinkIDs = InLinkIds;
			}
			nodeData.OutReferenceIDs = new List<TID>();
			if (OutReferenceData != null)
			{
				foreach(var outReferenceData in OutReferenceData) { nodeData.OutReferenceIDs.Add((TID)outReferenceData.Id); }
			}
			nodeData.InReferenceIDs = new List<TID>();
			if (InReferenceIds != null)
			{
				nodeData.InReferenceIDs = InReferenceIds;
			}
			return nodeData;
		}
	}
	
	public interface INeuronAggregate
	{
		#region Note
		public void WriteNote(NoteData data);
		public NoteData ReadNote();
		public void ExpireNoteDereference(TID referenceId); 
		#endregion

		#region Link
		public void WriteLink(TID linkId, LinkData linkData);
		public LinkData ReadLink(TID linkId);
		public void AddLink(TID linkId, LinkData linkData);
		public void RemoveLink(TID linkId);
		#endregion

		#region Reference
		public void WriteReference(TID referenceId, ReferenceData referenceData);
		public ReferenceData ReadReference(TID referenceId);
		public void AddReference(TID referenceId, ReferenceData referenceData);
		public void RemoveReference(TID referenceId);
		#endregion
	}
	public interface INeuronDomainService
	{
		#region Neuron
		public NeuronData CreateNewNeuron();
		public void DeleteNeuron(TID neuronId);
		#endregion
		#region Note 
		public void WriteNoteOfNeuron(TID neuronId, NoteData noteData, Dictionary<TID, ReferenceData>? newReferenceData);
		public NoteData ReadNoteOfNeuron(TID neuronId);
		///public void ExpireNoteDereferenceOfNeuron(TID neuronId, TID referenceNeuronId);
		public string GetNoteDereferenceOfNeuron(TID neuronId);
		public bool DoesReferencenRecurseInNeuron(TID neuronId, TID referenceNeuronId);
		#endregion

		#region Link
		
		public void WriteLinkOfNeuron(TID neuronId, TID linkId, LinkData linkData);
		public LinkData ReadLinkOfNeuron(TID neuronId, TID linkId);
		public void AddLinkToNeuron(TID neuronId, TID linkId, LinkData linkData); 
		public void RemoveLinkFromNeuron(TID neuronId, TID linkId);
		#endregion

		#region Reference (Reference is only required in Note operation)
		/*
		public void WriteReferenceOfNeuron(TID neuronId, TID referenceId, ReferenceData referenceData);
		public ReferenceData ReadReferenceOfNeuron(TID neuronId, TID referenceId);
		public void AddReferenceToNeuron(TID neuronId, TID referenceId, ReferenceData referenceData);
		public void RemoveReferenceToNeuron(TID neuronId, TID referenceId);
		*/
		#endregion
	}
	//implementation: public class Neuron:NeuronAggregate, INeuron {}
	public class Neuron : NeuronData, INeuronAggregate
	{

		#region Neuron Aggregate
		#region Holding References
		protected Note? Note { get; set; }
		protected Dictionary<TID, Link>? OutLinks { set; get; }
		protected Dictionary<TID, Reference>? OutReferences { set; get; }
		#endregion

		#region Override getter of LinkData & ReferenceData
		public bool IsLoadingDB = false;
		public override NoteData? NoteData
		{
			set
			{
				if (IsLoadingDB) Note = new Note(NoteData);
			}
			get
			{
				return Note;
			}
		} 
		public override List<LinkData>? OutLinkData
		{
			set
			{
				if (IsLoadingDB) OutLinks = value.ToDictionary(linkData => (TID)linkData.Id, linkData => new Link(linkData));
			}
			get
			{
				return OutLinks.Values.Select(link => (LinkData)link).ToList();
			}
		}
		
		public override List<ReferenceData>? OutReferenceData
		{
			set
			{
				if (IsLoadingDB) OutReferences = value.ToDictionary(referenceData => (TID)referenceData.Id, referenceData => new Reference(referenceData));	
			}
			get
			{
				return OutReferences.Values.Select(reference => (ReferenceData)reference).ToList();	
			}
		}
		
		#endregion 

		public Neuron(NeuronData neuronData) : base(neuronData)
		{
			EnsurePropertyNotNull();

		}
		public virtual void EnsurePropertyNotNull()
		{
			if (Id == null)
			{
				Id = default(TID);
			}
			if (NeuronClass == null)
			{
				NeuronClass = default(ENeuronClass);
			}
			if (ImagePath == null)
			{
				ImagePath = "";
			}
			// ... others later, there should not be error if repository work well
			InstantiateAggregateMembers();

		}
		public void InstantiateAggregateMembers()
		{
			if (Note == null)
			{
				Note = new Note(NoteData);
				Note.EnsurePropertyNotNull();
			}
			if (OutLinks == null)
			{
				OutLinks = new Dictionary<TID, Link>();
				foreach (var outLinkData in OutLinkData)
				{
					OutLinks[(TID)outLinkData.Id] = new Link(outLinkData);
					OutLinks[outLinkData.Key].EnsurePropertyNotNull();
				}
			}
			if (OutReferences == null)
			{
				OutReferences = new Dictionary<TID, Reference>();
				foreach (var outReferenceData in OutReferenceData)
				{
					OutReferences[(TID)outReferenceData.Id] = new Reference(outReferenceData);
					OutReferences[outReferenceData.Key].EnsurePropertyNotNull();
				}
			}
		}
		protected void InitializeProperties()
		{
			InLinkNeuronIdPairs = new Dictionary<TID, TID>();
			foreach (var id in InLinkIds)
			{
				InLinkNeuronIdPairs[(TID)id] = ;
			}
		}
		#region Note 
		public void ExpireNoteDereference(TID referenceId)
		{
			Note.ExpireDereference(referenceId);
		}

		public void WriteNote(NoteData data)
		{
			NoteData = data;
			Note.Write(data);
		}
		public NoteData ReadNote()
		{
			return Note.Read();
		}
		#endregion

		#region Link
		public void WriteLink(TID linkId, LinkData linkData)
		{
			OutLinks[linkId].Write(linkData);
			OutLinkData[linkId] = OutLinks[linkId].Read(); // get updated Link Data by .Read() due to partial write mechanism
		}
		public LinkData ReadLink(TID linkId)
		{
			return OutLinks[linkId].Read();
		}
		public void AddLink(TID linkId, LinkData linkData)
		{
			Link link = new Link(linkData);
			OutLinks[linkId] = link;
			OutLinkData[linkId] = linkData;
		}
		public void RemoveLink(TID linkId)
		{
			OutLinks.Remove(linkId);
			OutLinkData.Remove(linkId);
		}

		#endregion

		#region Reference
		public void WriteReference(TID referenceId, ReferenceData referenceData)
		{
			OutReferences[referenceId].Write(referenceData);
			OutReferenceData[referenceId] = OutReferences[referenceId].Read(); // get updated Reference Data by .Read() due to partial write mechanism
		}
		public ReferenceData ReadReference(TID referenceId)
		{
			return OutReferences[referenceId].Read();
		}
		public void AddReference(TID referenceId, ReferenceData referenceData)
		{
			Reference reference = new Reference(referenceData);
			OutReferences[referenceId] = reference;
			OutReferenceData[referenceId] = referenceData;
		}
		public void RemoveReference(TID referenceId)
		{
			OutReferences.Remove(referenceId);
			OutReferenceData.Remove(referenceId);
		}

		#endregion
		#endregion
	}
	public class Neuron0 : NeuronData, INeuronAggregate
	{

		#region Neuron Aggregate
		#region Holding References
		protected Note? Note { get; set; }
		protected Dictionary<TID, Link>? OutLinks { set; get; }
		protected Dictionary<TID, Reference>? OutReferences { set; get; }

		#endregion


		public Neuron(NeuronData neuronData) : base(neuronData)
		{
			EnsurePropertyNotNull();

		}
		public virtual void EnsurePropertyNotNull()
		{
			if (Id == null)
			{
				Id = default(TID);
			}
			if (NeuronClass == null)
			{
				NeuronClass = default(ENeuronClass);
			}
			if (ImagePath == null)
			{
				ImagePath = "";
			}
			// ... others later, there should not be error if repository work well
			InstantiateAggregateMembers();

		}
		public void InstantiateAggregateMembers()
		{
			if (Note == null)
			{
				Note = new Note(NoteData);
				Note.EnsurePropertyNotNull();
			}
			if (OutLinks == null)
			{
				OutLinks = new Dictionary<TID, Link>();
				foreach (var kvp in OutLinkData)
				{
					OutLinks[kvp.Key] = new Link(kvp.Value);
					OutLinks[kvp.Key].EnsurePropertyNotNull();
				}
			}
			if (OutReferences == null)
			{
				OutReferences = new Dictionary<TID, Reference>();
				foreach (var kvp in OutReferenceData)
				{
					OutReferences[kvp.Key] = new Reference(kvp.Value);
					OutReferences[kvp.Key].EnsurePropertyNotNull();
				}
			}



		}

		#region Note 
		public void ExpireNoteDereference(TID referenceId)
		{
			Note.ExpireDereference(referenceId);
		}

		public void WriteNote(NoteData data)
		{
			NoteData = data;
			Note.Write(data);
		}
		public NoteData ReadNote()
		{
			return Note.Read();
		}
		#endregion

		#region Link
		public void WriteLink(TID linkId, LinkData linkData)
		{
			OutLinks[linkId].Write(linkData);
			OutLinkData[linkId] = OutLinks[linkId].Read(); // get updated Link Data by .Read() due to partial write mechanism
		}
		public LinkData ReadLink(TID linkId)
		{
			return OutLinks[linkId].Read();
		}
		public void AddLink(TID linkId, LinkData linkData)
		{
			Link link = new Link(linkData);
			OutLinks[linkId] = link;
			OutLinkData[linkId] = linkData;
		}
		public void RemoveLink(TID linkId)
		{
			OutLinks.Remove(linkId);
			OutLinkData.Remove(linkId);
		}

		#endregion

		#region Reference
		public void WriteReference(TID referenceId, ReferenceData referenceData)
		{
			OutReferences[referenceId].Write(referenceData);
			OutReferenceData[referenceId] = OutReferences[referenceId].Read(); // get updated Reference Data by .Read() due to partial write mechanism
		}
		public ReferenceData ReadReference(TID referenceId)
		{
			return OutReferences[referenceId].Read();
		}
		public void AddReference(TID referenceId, ReferenceData referenceData)
		{
			Reference reference = new Reference(referenceData);
			OutReferences[referenceId] = reference;
			OutReferenceData[referenceId] = referenceData;
		}
		public void RemoveReference(TID referenceId)
		{
			OutReferences.Remove(referenceId);
			OutReferenceData.Remove(referenceId);
		}

		#endregion
		#endregion
	}
}
