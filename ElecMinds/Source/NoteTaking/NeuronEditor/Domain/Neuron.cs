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
#endregion

namespace NoteTaking.Domain
{
	
	public class NeuronData: INeuronDTO<NoteData, LinkData, ReferenceData, NoteSegment>
	{
		
		public TID? Id { get; set; }
		public ENeuronClass? NeuronClass { get; set; }
		public string? ImagePath { get; set; }

		#region Aggregate Members
		public NoteData? NoteData { get; set; }
		public Dictionary<TID, LinkData>? OutLinkData { get; set; }
		public Dictionary<TID, TID>? InLinkNeuronIdPairs { get; set; } 
			// dictionary of (linkId, neuronId) pairs;
			// neuron may be multiple linked, should not be key of dictionary
		public Dictionary<TID, ReferenceData>? OutReferenceData { get; set; }
		public Dictionary<TID, TID>? InReferenceNeuronIdPairs { get; set; } 
			// dictionary of (referenceId, NeuronId) pairs;
			// neuron may be multiple linked, should not be key of dictionary
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
			InLinkNeuronIdPairs = neuronData.InLinkNeuronIdPairs;
			OutReferenceData = neuronData.OutReferenceData;
			InReferenceNeuronIdPairs = neuronData.InReferenceNeuronIdPairs;
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
				neuronData.OutLinkData = new Dictionary<TID, LinkData>();
				foreach (var kvp in OutLinkData)
				{
					neuronData.OutLinkData[kvp.Key] = kvp.Value.DeepCopy();
				}
			}
			if (InLinkNeuronIdPairs == null)
			{
				neuronData.InLinkNeuronIdPairs = null;
			}
			else
			{
				neuronData.InLinkNeuronIdPairs = new Dictionary<TID, TID>(InLinkNeuronIdPairs);
			}
			if (OutReferenceData == null)
			{
				neuronData.OutReferenceData = null;
			}
			else
			{
				neuronData.OutReferenceData = new Dictionary<TID, ReferenceData>();
				foreach (var kvp in OutReferenceData)
				{
					neuronData.OutReferenceData[kvp.Key] = kvp.Value.DeepCopy();
				}
			}
			if (InReferenceNeuronIdPairs == null)
			{
				neuronData.InReferenceNeuronIdPairs = null;
			}
			else
			{
				neuronData.InReferenceNeuronIdPairs = new Dictionary<TID, TID>(InReferenceNeuronIdPairs);
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
				nodeData.OutLinkIDs = OutLinkData.Keys.ToList();
			}
			nodeData.InLinkIDs = new List<TID>();
			if (InLinkNeuronIdPairs != null)
			{
				nodeData.InLinkIDs = InLinkNeuronIdPairs.Keys.ToList();
			}
			nodeData.OutReferenceIDs = new List<TID>();
			if (OutReferenceData != null)
			{
				nodeData.OutReferenceIDs = OutReferenceData.Keys.ToList();
			}
			nodeData.InReferenceIDs = new List<TID>();
			if (InReferenceNeuronIdPairs != null)
			{
				nodeData.InReferenceIDs = InReferenceNeuronIdPairs.Keys.ToList();
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


		public Neuron(NeuronData neuronData):base(neuronData) 
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
