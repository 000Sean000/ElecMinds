/*
 * Wrap Domain Service to Undo-able Application Service
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;



#region Dependency
using NoteTaking.Domain;
using EvntObj;
using InteractionDirecting.API;

#endregion

namespace NoteTaking.Application
{
	public class NodeApplicationService
	{
		protected readonly IServiceProvider _serviceProvider;
		protected INodeRepository _nodeRepository;
		protected NodeDomainService _nodeDS;
		protected InteractionDirecting.API.IInteractionAPI _interactionAPI;
		
		public NodeApplicationService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_nodeDS = serviceProvider.GetRequiredService<NodeDomainService>();

			_interactionAPI = serviceProvider.GetService<InteractionDirecting.API.IInteractionAPI>();
		}

		#region Node		
		public Guid CreateNewNode()
		{
			CreateNewNode cmd = new CreateNewNode(_nodeDS);
			_interactionAPI.Execute(cmd);
			return (Guid)cmd.NodeData.Id;
		}
		public void DeleteNode(Guid nodeId)
		{
			DeleteNode cmd = new DeleteNode(_nodeDS, nodeId);
			_interactionAPI.Execute(cmd);
		}
		public NodeData ReadNode(Guid nodeId)
		{
			return _nodeDS.ReadNode(nodeId);
		}
		public void WriteNode(Guid nodeId, NodeData nodeData)
		{
			WriteNode cmd = new WriteNode(_nodeDS, nodeId, nodeData);
			_interactionAPI.Execute(cmd);

		}
		#endregion

		#region Note
		public NoteData ReadNoteOfNode(Guid nodeId)
		{
			return _nodeDS.ReadNoteOfNode(nodeId);
		}
		public void WriteNoteOfNode(Guid nodeId, NoteData noteData, Dictionary<Guid, ReferenceData> newReferenceDataPairs)
		{
			WriteNoteOfNode cmd = new WriteNoteOfNode(_nodeDS, nodeId, noteData, newReferenceDataPairs);
			_interactionAPI.Execute(cmd);

		}
		public bool DoesReferencenRecurseInNode(Guid nodeId, Guid referenceNodeId)
		{
			return _nodeDS.DoesReferencenRecurseInNode(nodeId, referenceNodeId);
		}

		#endregion

		#region Link 
		public LinkData ReadLinkOfNode(Guid nodeId, Guid linkId)
		{
			return _nodeDS.ReadLinkOfNode(nodeId, linkId);
		}
		public void WriteLinkOfNode(Guid nodeId, Guid linkId, LinkData linkData)
		{
			WriteLinkOfNode cmd = new WriteLinkOfNode(_nodeDS, nodeId, linkId, linkData);
			_interactionAPI.Execute(cmd);
		}
		public void AddLinkToNode(Guid nodeId, Guid linkId, LinkData linkData)
		{			
			AddLinkToNode cmd = new AddLinkToNode(_nodeDS, nodeId, linkId, linkData);
			_interactionAPI.Execute(cmd);
		}
		public void RemoveLinkFromNode(Guid nodeId, Guid linkId)
		{
			RemoveLinkFromNode cmd = new RemoveLinkFromNode(_nodeDS, nodeId, linkId); 
			_interactionAPI.Execute(cmd);
		}
		#endregion
	}
	#region 
	#endregion
	#region
	#endregion
	#region Command with Undo

	public class CreateNewNode : ICommandWithUndo
	{
		protected NodeDomainService _nodeDS;
		public NodeData? NodeData { get; set; } 
		public CreateNewNode(NodeDomainService nodeDS)
		{
			_nodeDS = nodeDS;
		}
		public void Execute()
		{
			if (NodeData == null)
			{
				NodeData = _nodeDS.CreateNewNode();
			}
			else
			{
				_nodeDS.RecoverNode(NodeData);
			}
		}
		public void Undo()
		{
			_nodeDS.DeleteNode((Guid)NodeData.Id);
		}
	}
	public class DeleteNode : ICommandWithUndo
	{
		protected NodeDomainService _nodeDS;
		public NodeData NodeData { get; set; }
		public DeleteNode(NodeDomainService nodeDS, Guid nodeId)
		{
			_nodeDS = nodeDS;
			NodeData = _nodeDS.ReadNode(nodeId);
		}
		public void Execute()
		{
			_nodeDS.DeleteNode((Guid)NodeData.Id);
		}
		public void Undo()
		{
			_nodeDS.RecoverNode(NodeData);
		}
	}
	public class WriteNode : ICommandWithUndo
	{
		protected NodeDomainService _nodeDS;
		public Guid NodeId { get; set; }
		public NodeData OldNodeData { get; set; }
		public NodeData NewNodeData { get; set; }
		public WriteNode(NodeDomainService nodeDS, Guid nodeId, NodeData nodeData)
		{
			_nodeDS = nodeDS;
			NodeId = nodeId;
			OldNodeData = _nodeDS.ReadNode(nodeId);
			NewNodeData = nodeData;

		}
		public void Execute()
		{
			_nodeDS.WriteNode(NodeId, NewNodeData);
		}
		public void Undo()
		{
			_nodeDS.WriteNode(NodeId, OldNodeData);
		}
	}
	public class WriteNoteOfNode : ICommandWithUndo
	{
		protected NodeDomainService _nodeDS;
		public Guid NodeId { get; set; }
		public NoteData OldNoteData { get; set; }
		public NoteData NewNoteData { get; set; }
		Dictionary<Guid, ReferenceData> OldReferenceDataPairs { get; set; }
		Dictionary<Guid, ReferenceData> NewReferenceDataPairs { get; set; }

		public WriteNoteOfNode(NodeDomainService nodeDS, Guid nodeId, NoteData noteData, Dictionary<Guid, ReferenceData> newReferenceDataPairs)
		{
			_nodeDS = nodeDS;
			OldNoteData = _nodeDS.ReadNoteOfNode(nodeId);
			NewNoteData = noteData;
			OldReferenceDataPairs = _nodeDS.ReadNode(nodeId).OutReferenceData;
			NewReferenceDataPairs = newReferenceDataPairs;
		}
		public void Execute()
		{
			_nodeDS.WriteNoteOfNode(NodeId, NewNoteData, NewReferenceDataPairs);
		}
		public void Undo()
		{
			_nodeDS.WriteNoteOfNode(NodeId, OldNoteData, OldReferenceDataPairs);
		}
	}
	public class WriteLinkOfNode : ICommandWithUndo
	{
		protected NodeDomainService _nodeDS;
		public Guid NodeId { get; set; }
		public Guid LinkId { get; set; }
		public LinkData OldLinkData { get; set; }
		public LinkData NewLinkData { get; set; }
		public WriteLinkOfNode(NodeDomainService nodeDS, Guid nodeId, Guid linkId, LinkData linkData)
		{
			_nodeDS = nodeDS;
			NodeId = nodeId;
			LinkId = linkId;
			OldLinkData = _nodeDS.ReadLinkOfNode(nodeId, linkId);
			NewLinkData = linkData;
		}
		public void Execute()
		{
			_nodeDS.WriteLinkOfNode(NodeId, LinkId, NewLinkData);
		}
		public void Undo()
		{
			_nodeDS.WriteLinkOfNode(NodeId, LinkId, OldLinkData);
		}
	}
	public class AddLinkToNode : ICommandWithUndo
	{
		protected NodeDomainService _nodeDS;
		public Guid NodeId { get; set; }
		public Guid LinkId { get; set; }
		public LinkData LinkData { get; set; }
		public AddLinkToNode(NodeDomainService nodeDS, Guid nodeId, Guid linkId, LinkData linkData)
		{
			_nodeDS = nodeDS;
			NodeId = nodeId;
			LinkId = linkId;
			LinkData = linkData;
		}
		public void Execute()
		{
			_nodeDS.AddLinkToNode(NodeId, LinkId, LinkData);
		}
		public void Undo()
		{
			_nodeDS.RemoveLinkFromNode(NodeId, LinkId);
		}
	}
	public class RemoveLinkFromNode : ICommandWithUndo
	{
		protected NodeDomainService _nodeDS;
		public Guid NodeId { get; set; }
		public Guid LinkId { get; set; }
		public LinkData LinkData { get; set; }
		public RemoveLinkFromNode(NodeDomainService nodeDS, Guid nodeId, Guid linkId)
		{
			_nodeDS = nodeDS;
			NodeId = nodeId;	
			LinkId = linkId;
			LinkData = _nodeDS.ReadLinkOfNode(nodeId, linkId);
		}
		public void Execute()
		{
			_nodeDS.RemoveLinkFromNode(NodeId, LinkId) ;
		}
		public void Undo()
		{
			_nodeDS.AddLinkToNode(NodeId, LinkId, LinkData) ;
		}
	}




	#endregion
	public class Op : ICommandWithUndo
	{
		protected NodeDomainService _nodeDS;

		public Op(NodeDomainService nodeDS)
		{
			_nodeDS = nodeDS;
		}
		public void Execute()
		{

		}
		public void Undo()
		{

		}
	}

}
