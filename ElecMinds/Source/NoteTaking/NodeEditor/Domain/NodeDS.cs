/*
 * Maintain consistency of aggregate, encapsulate it to Domain Service
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

#region Dependency
using Enums;
#endregion

namespace NoteTaking.Domain
{
	public interface INodeRepository
	{
		public Node CreateNode();
		public Node FetchNode(Guid nodeId);
		public void DeleteNode(Guid nodeId);
		public void RecoverNode(NodeData nodeData); // Undo DeleteNode()
	}

	// don't return Aggregate instance to outside, just return data instance
	public class NodeDomainService: INodeDomainService
	{
		protected INodeRepository _nodeRepo { get; set; }

		public NodeDomainService(IServiceProvider serviceProvider)
		{
			_nodeRepo = serviceProvider.GetRequiredService<INodeRepository>();
		}
		#region Node
		public NodeData CreateNewNode()
		{
			Node node = _nodeRepo.CreateNode();
			return node.Read();
		}
		public void DeleteNode(Guid nodeId)
		{
			_nodeRepo.DeleteNode(nodeId);
		}
		public void RecoverNode(NodeData nodeData)
		{
			_nodeRepo.RecoverNode(nodeData);
		}
		public NodeData ReadNode(Guid nodeId)
		{
			Node node = _nodeRepo.FetchNode(nodeId);
			return node.Read();
		}
		public void WriteNode(Guid nodeId, NodeData nodeData)
		{
			Node node = _nodeRepo.FetchNode(nodeId);
			node.Write(nodeData);
		}
		#endregion

		#region Note
		public void WriteNoteOfNode(Guid nodeId, NoteData noteData, Dictionary<Guid, ReferenceData> newReferenceDataPairs) 
		{
			//// check whether reference recurses-> do this job in application service
			// suppose that references do not cause reursion
			Node node = _nodeRepo.FetchNode(nodeId);

			// write node's note
			node.WriteNote(noteData);
			
			// update node's outgoing references
			Dictionary<Guid, ReferenceData> oldOutReferenceData = node.OutReferenceData;
			Dictionary<Guid, Guid> removedOutReferenceNodeIds = new Dictionary<Guid, Guid>();
			foreach(var oldReferenceId in node.OutReferenceData.Keys) // remvoe all old references
			{
				if (!newReferenceDataPairs.ContainsKey(oldReferenceId))
				{
					removedOutReferenceNodeIds[oldReferenceId] = (Guid)oldOutReferenceData[oldReferenceId].TargetNodeId;
				}
				node.RemoveReference(oldReferenceId);
			}
			foreach(var kvp in newReferenceDataPairs) // add new reference
			{
				node.AddReference(kvp.Key, kvp.Value);				
			}

			// update outgoing reference node's InReferenceNodeIdPairs

			foreach (var kvp in removedOutReferenceNodeIds) // remove old reference
			{
				Node removedNode = _nodeRepo.FetchNode(kvp.Value);
				removedNode.RemoveReference(kvp.Key);
			}
			foreach (var kvp in newReferenceDataPairs) // add new reference
			{
				Node outNode = _nodeRepo.FetchNode((Guid)kvp.Value.TargetNodeId);
				outNode.AddReference(kvp.Key, kvp.Value);
			}

			// expire incoming reference node's dereference
			foreach (var kvp in node.InReferenceNodeIdPairs)
			{
				Node inNode = _nodeRepo.FetchNode(kvp.Value);
				inNode.ExpireNoteDereference(kvp.Key);
			}
		
		}
		public NoteData ReadNoteOfNode(Guid nodeId)
		{
			return _nodeRepo.FetchNode(nodeId).ReadNote();
		}
		/*
		public void ExpireNoteDereferenceOfNode(Guid nodeId, Guid referenceId)
		{
			Node node = NodeRepo.FetchNode(nodeId);
			node.ExpireNoteDereference(referenceId);
		}
		*/
		public string GetNoteDereferenceOfNode(Guid nodeId)
		{
			return GetNoteDereferenceWithUpdate(new List<Guid> { nodeId });
		}
		protected string GetNoteDereferenceWithUpdate(List<Guid> branchVisitedNodeIds)
		{
			string dereference = string.Empty;

			Guid nodeId = branchVisitedNodeIds.Last();
			Node node = _nodeRepo.FetchNode(nodeId);
			
			List<NoteSegment> segments = node.ReadNote().Segments;
			foreach (var seg in segments)
			{
				if (seg.ReferenceId != null) // this is a reference
				{
					Guid referenceId = (Guid)seg.ReferenceId;
					Guid referenceTargetNodeId = (Guid)node.ReadReference(referenceId).TargetNodeId;
					
					if (seg.Text == null) // dereference had expired
					{
						if (branchVisitedNodeIds.Contains((Guid)referenceTargetNodeId))
						{
							throw new Exception("[Recursive Reference!]");
						}
						else
						{
							List<Guid> nextBranchVisitedNodeIds = new List<Guid>(branchVisitedNodeIds);
							nextBranchVisitedNodeIds.Add(referenceTargetNodeId);
							string text = GetNoteDereferenceWithUpdate(nextBranchVisitedNodeIds);
							EDereferencerType dereferencerType = (EDereferencerType)node.ReadReference(referenceId).DereferencerType;
							seg.Text = RefConfig.Dereferencer[dereferencerType](text);
						}
					}
				}
				dereference += seg.Text;
			}
			node.WriteNote(new NoteData() { Segments = segments });
			return dereference;
		}
		public bool DoesReferencenRecurseInNode(Guid nodeId, Guid referenceNodeId)
		{
			List<Guid> nodeIds = new List<Guid>() { referenceNodeId, nodeId };
			return IsReferenceRecursion(nodeIds);
		}
		protected bool IsReferenceRecursion(List<Guid> branchVisitedNodeIds)
		{
			
			Guid nodeId = branchVisitedNodeIds.Last();
			Node node = _nodeRepo.FetchNode(nodeId);

			List<NoteSegment> segments = node.ReadNote().Segments;
			foreach (var seg in segments)
			{
				if (seg.ReferenceId != null) // this is a reference
				{
					Guid referenceId = (Guid)seg.ReferenceId;
					Guid referenceTargetNodeId = (Guid)node.ReadReference(referenceId).TargetNodeId;

					if (branchVisitedNodeIds.Contains((Guid)referenceTargetNodeId))
					{
						return true;
					}
					else
					{
						List<Guid> nextBranchVisitedNodeIds = new List<Guid>(branchVisitedNodeIds);
						nextBranchVisitedNodeIds.Add(referenceTargetNodeId);
						if (IsReferenceRecursion(nextBranchVisitedNodeIds))
						{
							return true;
						}
					}
					
				}
			}
			return false;
		}
		#endregion

		#region Link
		public void WriteLinkOfNode(Guid nodeId, Guid linkId, LinkData linkData)
		{
			Node node = _nodeRepo.FetchNode(nodeId);
			node.WriteLink(linkId, linkData);
				
		}
		public LinkData ReadLinkOfNode(Guid nodeId, Guid linkId)
		{
			return _nodeRepo.FetchNode(nodeId).ReadLink(linkId);
		}
		public void AddLinkToNode(Guid nodeId, Guid linkId, LinkData linkData)
		{
			Node node = _nodeRepo.FetchNode(nodeId);

			node.AddLink(linkId, linkData);

			// update target node's incoming links
			Node targetNode = _nodeRepo.FetchNode((Guid)linkData.TargetNodeId);
			targetNode.InLinkNodeIdPairs[linkId] = nodeId;
		}
		public void RemoveLinkFromNode(Guid nodeId, Guid linkId)
		{
			Node node = _nodeRepo.FetchNode(nodeId);

			// update target node's incoming links
			Guid targetNodeId = node.InLinkNodeIdPairs[linkId];
			Node targetNode = _nodeRepo.FetchNode(targetNodeId);
			targetNode.InLinkNodeIdPairs.Remove(linkId);

			node.RemoveLink(linkId);
		}
		#endregion

		#region Reference (Reference is only required in Note operation)
		/*
		public void WriteReferenceOfNode(Guid nodeId, Guid referenceId, ReferenceData referenceData)
		{

		}
		public ReferenceData ReadReferenceOfNode(Guid nodeId, Guid referenceId)
		{
			return NodeRepo.FetchNode(nodeId).ReadReference(referenceId);
		}
		public void AddReferenceToNode(Guid nodeId, Guid referenceId, ReferenceData referenceData)
		{

		}
		public void RemoveReferenceToNode(Guid nodeId, Guid referenceId)
		{

		}
		*/
		#endregion

	}
}
