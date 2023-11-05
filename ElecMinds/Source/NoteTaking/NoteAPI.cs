
/*
 * Wrap Application Service with event publish
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;


#region Dependency
using DTOs;
using EvntObj;
using InteractionDirecting.API;
using NoteTaking.Application;
using NoteTaking.Domain;

#endregion

namespace NoteTaking.API
{
	public class NodeProfile : Profile
	{
		public NodeProfile()
		{
			CreateMap<NodeData, NodeDTO>().ReverseMap();
			CreateMap<NoteSegment, NoteSegmentDTO>().ReverseMap();
			CreateMap<NoteData, NoteDTO>().ReverseMap();
			CreateMap<LinkData, LinkDTO>().ReverseMap();
			CreateMap<ReferenceData, ReferenceDTO>().ReverseMap();
		}
	}
	public interface INodeEditorAPI
	{
		public Guid CreateNewNode();
		public void DeleteNode(Guid nodeId);
		public NodeDTO ReadNode(Guid nodeId);
		public void WriteNode(Guid nodeId, NodeDTO nodeDTO);
		public NoteDTO ReadNoteOfNode(Guid nodeId);
		public void WriteNoteOfNode(Guid nodeId, NoteDTO noteDTO, Dictionary<Guid, ReferenceDTO> newReferenceDTOPairs);
		public bool DoesReferencenRecurseInNode(Guid nodeId, Guid referenceNodeId);
		public LinkDTO ReadLinkOfNode(Guid nodeId, Guid linkId);
		public void WriteLinkOfNode(Guid nodeId, Guid linkId, LinkDTO linkDTO);
		public void AddLinkToNode(Guid nodeId, Guid linkId, LinkDTO linkDTO);
		public void RemoveLinkFromNode(Guid nodeId, Guid linkId);


	}
	
	public class NodeEditorAPI:INodeEditorAPI
	{
		protected readonly IServiceProvider _serviceProvider;
		protected NodeApplicationService _nodeAS;
		protected InteractionDirecting.API.IInteractionAPI _interactionAPI;

		public IMapper Mapper { get; set; }

		public NodeEditorAPI(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_nodeAS = serviceProvider.GetService<NodeApplicationService>();
			_interactionAPI = serviceProvider.GetService<InteractionDirecting.API.IInteractionAPI>();

			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<NodeProfile>();
			});
			Mapper = mapperConfig.CreateMapper();
		}

		#region Node		
		public Guid CreateNewNode()
		{
			Guid nodeId = _nodeAS.CreateNewNode();

			NodeCreated nodeCreated = new NodeCreated() { NodeId = nodeId };
			_interactionAPI.EBusPublish<NodeCreated>(nodeCreated);

			return nodeId;
		}
		public void DeleteNode(Guid nodeId)
		{
			_nodeAS.DeleteNode(nodeId);

			NodeDeleted nodeDeleted = new NodeDeleted() { NodeId = nodeId };
			_interactionAPI.EBusPublish<NodeDeleted>(nodeDeleted);
		}
		public NodeDTO ReadNode(Guid nodeId)
		{
			NodeData nodeData = _nodeAS.ReadNode(nodeId);
			NodeDTO nodeDTO = Mapper.Map<NodeDTO>(nodeData);
			NodeRead nodeRead = new NodeRead() { NodeId = nodeId, NodeDTO = nodeDTO };
			_interactionAPI.EBusPublish<NodeRead>(nodeRead);
			return nodeDTO;
		}
		public void WriteNode(Guid nodeId, NodeDTO nodeDTO)
		{
			NodeData nodeData = Mapper.Map<NodeData>(nodeDTO);
			_nodeAS.WriteNode(nodeId, nodeData);
			NodeWritten nodeWritten = new NodeWritten() { NodeId = nodeId, NodeDTO = nodeDTO };
			_interactionAPI.EBusPublish<NodeWritten>(nodeWritten);

		}
		#endregion

		#region Note
		public NoteDTO ReadNoteOfNode(Guid nodeId)
		{
			NoteData noteData = _nodeAS.ReadNoteOfNode(nodeId);
			NoteDTO noteDTO = Mapper.Map<NoteDTO>(noteData);
			NoteRead noteRead = new NoteRead() { NodeId = nodeId, NoteDTO = noteDTO };
			_interactionAPI.EBusPublish<NoteRead>(noteRead);
			return noteDTO;
		}
		public void WriteNoteOfNode(Guid nodeId, NoteDTO noteDTO, Dictionary<Guid, ReferenceDTO> newReferenceDTOPairs)
		{
			NoteData noteData = Mapper.Map<NoteData>(noteDTO);
			Dictionary<Guid, ReferenceData> newReferenceData = Mapper.Map<Dictionary<Guid, ReferenceData>>(newReferenceDTOPairs);
			_nodeAS.WriteNoteOfNode(nodeId, noteData, newReferenceData);
			NoteWritten noteWritten = new NoteWritten() { NodeId = nodeId, NoteDTO = noteDTO, NewReferenceDTOPairs = newReferenceDTOPairs };
			_interactionAPI.EBusPublish<NoteWritten>(noteWritten);

		}
		public bool DoesReferencenRecurseInNode(Guid nodeId, Guid referenceNodeId)
		{
			bool isRecursion = _nodeAS.DoesReferencenRecurseInNode(nodeId, referenceNodeId);
			ReferenceRecurses referenceRecurses = new ReferenceRecurses() { NodeId = nodeId, ReferenceNodeId = referenceNodeId };
			_interactionAPI.EBusPublish<ReferenceRecurses>(referenceRecurses);
			return isRecursion;
		}

		#endregion

		#region Link 
		public LinkDTO ReadLinkOfNode(Guid nodeId, Guid linkId)
		{
			LinkData linkData = _nodeAS.ReadLinkOfNode(nodeId, linkId);
			LinkDTO linkDTO = Mapper.Map<LinkDTO>(linkData);
			LinkRead linkRead = new LinkRead() { NodeId = nodeId, LinkId = linkId, LinkDTO = linkDTO };
			_interactionAPI.EBusPublish<LinkRead>(linkRead);
			return linkDTO;
		}
		public void WriteLinkOfNode(Guid nodeId, Guid linkId, LinkDTO linkDTO)
		{
			LinkData linkData = Mapper.Map<LinkData>(linkDTO);
			_nodeAS.WriteLinkOfNode(nodeId, linkId, linkData);
			LinkWritten linkWritten = new LinkWritten() { NodeId = nodeId, LinkId = linkId, LinkDTO = linkDTO };
			_interactionAPI.EBusPublish<LinkWritten>(linkWritten);
		}
		public void AddLinkToNode(Guid nodeId, Guid linkId, LinkDTO linkDTO)
		{
			LinkData linkData = Mapper.Map<LinkData>(linkDTO);
			_nodeAS.AddLinkToNode(nodeId, linkId, linkData);
			LinkAdded linkAdded = new LinkAdded() { NodeId = nodeId, LinkId = linkId, LinkDTO = linkDTO };
			_interactionAPI.EBusPublish<LinkAdded>(linkAdded);
		}
		public void RemoveLinkFromNode(Guid nodeId, Guid linkId)
		{
			_nodeAS.RemoveLinkFromNode(nodeId, linkId);
			LinkRemoved linkRemoved = new LinkRemoved() { NodeId = nodeId, LinkId = linkId };
			_interactionAPI.EBusPublish<LinkRemoved>(linkRemoved);
		}
		#endregion
	}

}
