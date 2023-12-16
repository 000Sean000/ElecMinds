using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

#region Dependency

using Config;
using Enums;
using DTOs;
using Config;
using PKG;
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
		protected List<LinkData> _OutLinkDatas;
		protected List<ReferenceData> _OutReferenceDatas;
		public virtual List<LinkData>? OutLinkDatas { get; set; }
		public List<TID>? InLinkIds { get; set; } 
		public virtual List<ReferenceData>? OutReferenceDatas { get; set; }
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
			OutLinkDatas = neuronData.OutLinkDatas;
			InLinkIds = neuronData.InLinkIds;
			OutReferenceDatas = neuronData.OutReferenceDatas;
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
			if (OutLinkDatas ==  null)
			{
				neuronData.OutLinkDatas = null;
			}
			else
			{
				neuronData.OutLinkDatas = new List<LinkData>();
				foreach (var outLinkData in OutLinkDatas)
				{
					neuronData.OutLinkDatas.Add(outLinkData.DeepCopy());
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
			if (OutReferenceDatas == null)
			{
				neuronData.OutReferenceDatas = null;
			}
			else
			{
				neuronData.OutReferenceDatas = new List<ReferenceData>();
				foreach (var outReferenceData in OutReferenceDatas)
				{
					neuronData.OutReferenceDatas.Add(outReferenceData.DeepCopy());
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
			if (OutLinkDatas != null)
			{
				foreach (var outLinkData in OutLinkDatas) { nodeData.OutLinkIDs.Add((TID)outLinkData.Id); }
				
			}
			nodeData.InLinkIDs = new List<TID>();
			if (InLinkIds != null)
			{
				nodeData.InLinkIDs = InLinkIds;
			}
			nodeData.OutReferenceIDs = new List<TID>();
			if (OutReferenceDatas != null)
			{
				foreach(var outReferenceData in OutReferenceDatas) { nodeData.OutReferenceIDs.Add((TID)outReferenceData.Id); }
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
		public TID CreateNewNeuron();
		public void DeleteNeuron(TID neuronId);
		#endregion
		#region Note 
		public void WriteNoteOfNeuron(TID neuronId, NoteData noteData, List<ReferenceData>? newReferenceData);
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

		#region Override accessors of LinkData & ReferenceData
		/*
		 *  Getter will be called automatically when property methods(e.g. Add(...)) are invoked, but Setter won't.
		 */
		public bool IsLoadingDB = false;
		protected NoteData? _noteData;
		public override NoteData? NoteData
		{
			set
			{
				if ((IsLoadingDB || NoteData == null) && value != null)
				{
					_noteData = value;
					Note = new Note(_noteData);
				}
			}
			get
			{
				_noteData.Write(Note);
				return _noteData;
			}
		}
		protected List<LinkData>? _outLinkDatas;
		public override List<LinkData>? OutLinkDatas
		{
			set
			{
				if ((IsLoadingDB || _outLinkDatas == null) && value != null)
				{
					_outLinkDatas = value;
					OutLinks = _outLinkDatas.ToDictionary(linkData => (TID)linkData.Id, linkData => new Link(linkData));

				}
			}
			get
			{
				// due to DB entity tracking, entities with same ID should have same reference(memory address)
				ListPKG.UpdateEntityList<LinkData, Link>(_outLinkDatas, OutLinks.Values.ToList(),
					(linkData, link) => linkData.Id == link.Id,
					(linkData, link) => linkData.Write(link.Read()),
					(link) => new LinkData(link.Read()));
				return _outLinkDatas;

			}
		}

		protected List<ReferenceData>? _outReferenceDatas;
		public override List<ReferenceData>? OutReferenceDatas
		{
			set
			{
				if ((IsLoadingDB || OutReferenceDatas == null) && value != null)
				{
					_outReferenceDatas = value;
					OutReferences = _outReferenceDatas.ToDictionary(referenceData => (TID)referenceData.Id, referenceData => new Reference(referenceData));
				}
			}
			get
			{
				// due to DB entity tracking, entities with same ID should have same reference(memory address)
				ListPKG.UpdateEntityList<ReferenceData, Reference>(_outReferenceDatas, OutReferences.Values.ToList(),
					(referenceData, reference) => referenceData.Id == reference.Id,
					(referenceData, reference) => referenceData.Write(reference.Read()),
					(reference) => new ReferenceData(reference.Read()));
				return _outReferenceDatas;
			}
		}

		#endregion
		public Neuron() : base()
		{
			EnsurePropertyNotNull();
		}
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
		}
		public LinkData ReadLink(TID linkId)
		{
			return OutLinks[linkId].Read();
		}
		public void AddLinkFromDB(TID linkId, LinkData linkData) // for recovering of deletion
		{
			if (_outLinkDatas == null) return; // initialization should be done before
			
			// update both DB entities and domain instances
			_outLinkDatas.Add(linkData);
			OutLinks[(TID)linkData.Id] = new Link(linkData);
		}
		public void AddLinkSimple(TID linkId, LinkData linkData)
		{
			OutLinks[(TID)linkData.Id] = new Link(linkData);
		}
		public void AddLink(TID linkId, LinkData linkData) // for recovering of deletion
		{
			if (_outLinkDatas == null) return; // initialization should be done before

			// update both DB entities and domain instances
			_outLinkDatas.Add(linkData);
			OutLinks[(TID)linkData.Id] = new Link(linkData);
		}
		public void RemoveLink(TID linkId)
		{
			OutLinks.Remove(linkId);
		}

		#endregion

		#region Reference
		public void WriteReference(TID referenceId, ReferenceData referenceData)
		{
			OutReferences[referenceId].Write(referenceData);
		}
		public ReferenceData ReadReference(TID referenceId)
		{
			return OutReferences[referenceId].Read();
		}
		public void AddReferenceFromDB(TID referenceId, ReferenceData referenceData) // for recovering of deletion
		{
			if (_outReferenceDatas == null) return;// initialization should be done before

			// update both DB entities and domain instances
			_outReferenceDatas.Add(referenceData);
			OutReferences[(TID)referenceData.Id] = new Reference(referenceData);
		}
		public void AddReferenceSimple(TID referenceId, ReferenceData referenceData)
		{
			OutReferences[(TID)referenceData.Id] = new Reference(referenceData);
		}
		public void AddReference(TID referenceId, ReferenceData referenceData) // for recovering of deletion
		{
			if (_outReferenceDatas == null) return;// initialization should be done before

			// update both DB entities and domain instances
			_outReferenceDatas.Add(referenceData);
			OutReferences[(TID)referenceData.Id] = new Reference(referenceData);
		}
		public void RemoveReference(TID referenceId)
		{
			OutReferences.Remove(referenceId);
		}

		#endregion
		#endregion
	}
}
