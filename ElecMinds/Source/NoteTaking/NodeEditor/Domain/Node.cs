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
	
	public class NodeData: INodeDTO<NoteData, LinkData, ReferenceData, NoteSegment>
	{
		
		public Guid? Id { get; set; }
		public ENodeClass? NodeClass { get; set; }
		public string? ImagePath { get; set; }

		#region Aggregate Members
		public NoteData? NoteData { get; set; }
		public Dictionary<Guid, LinkData>? OutLinkData { get; set; }
		public Dictionary<Guid, Guid>? InLinkNodeIdPairs { get; set; } 
			// dictionary of (linkId, nodeId) pairs;
			// node may be multiple linked, should not be key of dictionary
		public Dictionary<Guid, ReferenceData>? OutReferenceData { get; set; }
		public Dictionary<Guid, Guid>? InReferenceNodeIdPairs { get; set; } 
			// dictionary of (referenceId, NodeId) pairs;
			// node may be multiple linked, should not be key of dictionary
		#endregion



		public NodeData() { }
		public NodeData(NodeData nodeData)
		{
			Overwrite(nodeData);
		}

		public void Write(NodeData nodeData)
		{
			///nodeData = nodeData.DeepCopy();
			if (nodeData.Id != null)
			{
				Id = nodeData.Id;
			}
			if (nodeData.NodeClass != null)
			{
				NodeClass = nodeData.NodeClass;
			}
			if (nodeData.ImagePath != null)
			{
				ImagePath = nodeData.ImagePath;
			}
			// don't write member object here
		}
		protected void Overwrite(NodeData nodeData)
		{
			///nodeData = nodeData.DeepCopy();
			Id = nodeData.Id;
			NodeClass = nodeData.NodeClass;
			ImagePath = nodeData.ImagePath;

			NoteData = nodeData.NoteData;
			OutLinkData = nodeData.OutLinkData;
			InLinkNodeIdPairs = nodeData.InLinkNodeIdPairs;
			OutReferenceData = nodeData.OutReferenceData;
			InReferenceNodeIdPairs = nodeData.InReferenceNodeIdPairs;
		}
		public NodeData Read()
		{
			return DeepCopy();
		}
		public NodeData DeepCopy()
		{
			NodeData nodeData = new NodeData();

			nodeData.Id = Id;
			nodeData.NodeClass = NodeClass;
			nodeData.ImagePath = ImagePath;
			nodeData.NoteData = NoteData?.DeepCopy();
			if (OutLinkData ==  null)
			{
				nodeData.OutLinkData = null;
			}
			else
			{
				nodeData.OutLinkData = new Dictionary<Guid, LinkData>();
				foreach (var kvp in OutLinkData)
				{
					nodeData.OutLinkData[kvp.Key] = kvp.Value.DeepCopy();
				}
			}
			if (InLinkNodeIdPairs == null)
			{
				nodeData.InLinkNodeIdPairs = null;
			}
			else
			{
				nodeData.InLinkNodeIdPairs = new Dictionary<Guid, Guid>(InLinkNodeIdPairs);
			}
			if (OutReferenceData == null)
			{
				nodeData.OutReferenceData = null;
			}
			else
			{
				nodeData.OutReferenceData = new Dictionary<Guid, ReferenceData>();
				foreach (var kvp in OutReferenceData)
				{
					nodeData.OutReferenceData[kvp.Key] = kvp.Value.DeepCopy();
				}
			}
			if (InReferenceNodeIdPairs == null)
			{
				nodeData.InReferenceNodeIdPairs = null;
			}
			else
			{
				nodeData.InReferenceNodeIdPairs = new Dictionary<Guid, Guid>(InReferenceNodeIdPairs);
			}
			return nodeData;
		}
	}
	public interface INodeAggregateRoot
	{
		#region Note
		public ENoteComposition NoteComposition { get; set; }
		public ENoteImportance NoteImportance { get; set; }
		public List<NoteSegment> NoteSegments { get; set; }

		public void ExpireNoteDereference(Guid referenceId);
		#endregion


		#region
		public void WriteLinkType(Guid linkId, ELinkType linkType);
		public ELinkType ReadLinkType(Guid linkId);
		public void WriteLinkInfo(Guid linkId, ELinkInfoIndex linkInfoIndex, string text);
		public string ReadLinkInfo(Guid linkId, ELinkInfoIndex linkInfoIndex);

		public Guid ReadLinkSourceId(Guid linkId);
		public Guid ReadLinkTargetId(Guid linkId);

		public void AddLink(Guid linkId, LinkData linkData);
		public void RemoveLink(Guid linkId);
		#endregion

		#region Reference
		public void WriteDereferencerType(Guid referenceId, EDereferencerType dereferencerType);
		public EDereferencerType ReadDereferencerType(Guid referencId);

		public Guid ReadReferenceSourceId(Guid referenceId);
		public Guid ReadReferenceTargetId(Guid referenceId);

		public void AddReference(Guid referenceId, ReferenceData referenceData);
		public void RemoveReference(Guid referenceId);
		#endregion

	}
	public interface INodeAggregate
	{

		#region
		#endregion

		#region Note
		public void WriteNote(NoteData data);
		public NoteData ReadNote();
		public void ExpireNoteDereference(Guid referenceId); 
		#endregion

		#region Link
		public void WriteLink(Guid linkId, LinkData linkData);
		public LinkData ReadLink(Guid linkId);
		public void AddLink(Guid linkId, LinkData linkData);
		public void RemoveLink(Guid linkId);
		#endregion

		#region Reference
		public void WriteReference(Guid referenceId, ReferenceData referenceData);
		public ReferenceData ReadReference(Guid referenceId);
		public void AddReference(Guid referenceId, ReferenceData referenceData);
		public void RemoveReference(Guid referenceId);
		#endregion

	}
	public interface INodeDomainService
	{
		#region Node
		public NodeData CreateNewNode();
		public void DeleteNode(Guid nodeId);
		#endregion
		#region Note 
		public void WriteNoteOfNode(Guid nodeId, NoteData noteData, Dictionary<Guid, ReferenceData>? newReferenceData);
		public NoteData ReadNoteOfNode(Guid nodeId);
		///public void ExpireNoteDereferenceOfNode(Guid nodeId, Guid referenceNodeId);
		public string GetNoteDereferenceOfNode(Guid nodeId);
		public bool DoesReferencenRecurseInNode(Guid nodeId, Guid referenceNodeId);
		#endregion

		#region Link
		public void WriteLinkOfNode(Guid nodeId, Guid linkId, LinkData linkData);
		public LinkData ReadLinkOfNode(Guid nodeId, Guid linkId);
		public void AddLinkToNode(Guid nodeId, Guid linkId, LinkData linkData);
		public void RemoveLinkFromNode(Guid nodeId, Guid linkId);
		#endregion

		#region Reference (Reference is only required in Note operation)
		/*
		public void WriteReferenceOfNode(Guid nodeId, Guid referenceId, ReferenceData referenceData);
		public ReferenceData ReadReferenceOfNode(Guid nodeId, Guid referenceId);
		public void AddReferenceToNode(Guid nodeId, Guid referenceId, ReferenceData referenceData);
		public void RemoveReferenceToNode(Guid nodeId, Guid referenceId);
		*/
		#endregion
	}
	//implementation: public class Node:NodeAggregate, INode {}
	public class Node : NodeData, INodeAggregate
	{

		#region Node Aggregate
		#region Holding References
		protected Note? Note { get; set; }
		protected Dictionary<Guid, Link>? OutLinks { set; get; }
		protected Dictionary<Guid, Reference>? OutReferences { set; get; }

		#endregion


		public Node(NodeData nodeData):base(nodeData) 
		{
			EnsurePropertyNotNull();
			
		}
		public virtual void EnsurePropertyNotNull()
		{
			if (Id == null)
			{
				Id = default(Guid);
			}
			if (NodeClass == null)
			{
				NodeClass = default(ENodeClass);
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
				OutLinks = new Dictionary<Guid, Link>();
				foreach (var kvp in OutLinkData)
				{
					OutLinks[kvp.Key] = new Link(kvp.Value);
					OutLinks[kvp.Key].EnsurePropertyNotNull();
				}
			}
			if (OutReferences == null)
			{
				OutReferences = new Dictionary<Guid, Reference>();
				foreach (var kvp in OutReferenceData)
				{
					OutReferences[kvp.Key] = new Reference(kvp.Value);
					OutReferences[kvp.Key].EnsurePropertyNotNull();
				}
			}



		}

		#region Note 
		public void ExpireNoteDereference(Guid referenceId)
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
		public void WriteLink(Guid linkId, LinkData linkData)
		{
			OutLinks[linkId].Write(linkData);
			OutLinkData[linkId] = OutLinks[linkId].Read(); // get updated Link Data by .Read() due to partial write mechanism
		}
		public LinkData ReadLink(Guid linkId)
		{
			return OutLinks[linkId].Read();
		}
		public void AddLink(Guid linkId, LinkData linkData)
		{
			Link link = new Link(linkData);
			OutLinks[linkId] = link;
			OutLinkData[linkId] = linkData;
		}
		public void RemoveLink(Guid linkId)
		{
			OutLinks.Remove(linkId);
			OutLinkData.Remove(linkId);
		}

		#endregion

		#region Reference
		public void WriteReference(Guid referenceId, ReferenceData referenceData)
		{
			OutReferences[referenceId].Write(referenceData);
			OutReferenceData[referenceId] = OutReferences[referenceId].Read(); // get updated Reference Data by .Read() due to partial write mechanism
		}
		public ReferenceData ReadReference(Guid referenceId)
		{
			return OutReferences[referenceId].Read();
		}
		public void AddReference(Guid referenceId, ReferenceData referenceData)
		{
			Reference reference = new Reference(referenceData);
			OutReferences[referenceId] = reference;
			OutReferenceData[referenceId] = referenceData;
		}
		public void RemoveReference(Guid referenceId)
		{
			OutReferences.Remove(referenceId);
			OutReferenceData.Remove(referenceId);
		}

		#endregion
		#endregion
	}
}
