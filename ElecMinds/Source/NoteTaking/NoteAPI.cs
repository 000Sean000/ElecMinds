
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

using Config;
using DTOs;
using EvntObj;
using InteractionDirecting.API;
using NoteTaking.Application;
using NoteTaking.Domain;

#endregion

namespace NoteTaking.API
{
	public class NeuronProfile : Profile
	{
		public NeuronProfile()
		{
			CreateMap<NeuronData, NeuronDTO>().ReverseMap();
			CreateMap<NoteSegment, NoteSegmentDTO>().ReverseMap();
			CreateMap<NoteData, NoteDTO>().ReverseMap();
			CreateMap<LinkData, LinkDTO>().ReverseMap();
			CreateMap<ReferenceData, ReferenceDTO>().ReverseMap();
		}
	}
	public interface INeuronEditorAPI
	{
		public TID CreateNewNeuron();
		public void DeleteNeuron(TID neuronId);
		public NeuronDTO ReadNeuron(TID neuronId);
		public void WriteNeuron(TID neuronId, NeuronDTO neuronDTO);
		public NoteDTO ReadNoteOfNeuron(TID neuronId);
		public void WriteNoteOfNeuron(TID neuronId, NoteDTO noteDTO, Dictionary<TID, ReferenceDTO> newReferenceDTOPairs);
		public bool DoesReferencenRecurseInNeuron(TID neuronId, TID referenceNeuronId);
		public LinkDTO ReadLinkOfNeuron(TID neuronId, TID linkId);
		public void WriteLinkOfNeuron(TID neuronId, TID linkId, LinkDTO linkDTO);
		public void AddLinkToNeuron(TID neuronId, TID linkId, LinkDTO linkDTO);
		public void RemoveLinkFromNeuron(TID neuronId, TID linkId);


	}
	
	public class NeuronEditorAPI:INeuronEditorAPI
	{
		protected readonly IServiceProvider _serviceProvider;
		protected NeuronApplicationService _neuronAS;
		protected InteractionDirecting.API.IInteractionAPI _interactionAPI;

		public IMapper Mapper { get; set; }

		public NeuronEditorAPI(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_neuronAS = serviceProvider.GetService<NeuronApplicationService>();
			_interactionAPI = serviceProvider.GetService<InteractionDirecting.API.IInteractionAPI>();

			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<NeuronProfile>();
			});
			Mapper = mapperConfig.CreateMapper();
		}

		#region Neuron		
		public TID CreateNewNeuron()
		{
			TID neuronId = _neuronAS.CreateNewNeuron();

			NeuronCreated neuronCreated = new NeuronCreated() { NeuronId = neuronId };
			_interactionAPI.EBusPublish<NeuronCreated>(neuronCreated);

			return neuronId;
		}
		public void DeleteNeuron(TID neuronId)
		{
			_neuronAS.DeleteNeuron(neuronId);

			NeuronDeleted neuronDeleted = new NeuronDeleted() { NeuronId = neuronId };
			_interactionAPI.EBusPublish<NeuronDeleted>(neuronDeleted);
		}
		public NeuronDTO ReadNeuron(TID neuronId)
		{
			NeuronData neuronData = _neuronAS.ReadNeuron(neuronId);
			NeuronDTO neuronDTO = Mapper.Map<NeuronDTO>(neuronData);
			NeuronRead neuronRead = new NeuronRead() { NeuronId = neuronId, NeuronDTO = neuronDTO };
			_interactionAPI.EBusPublish<NeuronRead>(neuronRead);
			return neuronDTO;
		}
		public void WriteNeuron(TID neuronId, NeuronDTO neuronDTO)
		{
			NeuronData neuronData = Mapper.Map<NeuronData>(neuronDTO);
			_neuronAS.WriteNeuron(neuronId, neuronData);
			NeuronWritten neuronWritten = new NeuronWritten() { NeuronId = neuronId, NeuronDTO = neuronDTO };
			_interactionAPI.EBusPublish<NeuronWritten>(neuronWritten);

		}
		#endregion

		#region Note
		public NoteDTO ReadNoteOfNeuron(TID neuronId)
		{
			NoteData noteData = _neuronAS.ReadNoteOfNeuron(neuronId);
			NoteDTO noteDTO = Mapper.Map<NoteDTO>(noteData);
			NoteRead noteRead = new NoteRead() { NeuronId = neuronId, NoteDTO = noteDTO };
			_interactionAPI.EBusPublish<NoteRead>(noteRead);
			return noteDTO;
		}
		public void WriteNoteOfNeuron(TID neuronId, NoteDTO noteDTO, Dictionary<TID, ReferenceDTO> newReferenceDTOPairs)
		{
			NoteData noteData = Mapper.Map<NoteData>(noteDTO);
			Dictionary<TID, ReferenceData> newReferenceData = Mapper.Map<Dictionary<TID, ReferenceData>>(newReferenceDTOPairs);
			_neuronAS.WriteNoteOfNeuron(neuronId, noteData, newReferenceData);
			NoteWritten noteWritten = new NoteWritten() { NeuronId = neuronId, NoteDTO = noteDTO, NewReferenceDTOPairs = newReferenceDTOPairs };
			_interactionAPI.EBusPublish<NoteWritten>(noteWritten);

		}
		public bool DoesReferencenRecurseInNeuron(TID neuronId, TID referenceNeuronId)
		{
			bool isRecursion = _neuronAS.DoesReferencenRecurseInNeuron(neuronId, referenceNeuronId);
			ReferenceRecurses referenceRecurses = new ReferenceRecurses() { NeuronId = neuronId, ReferenceNeuronId = referenceNeuronId };
			_interactionAPI.EBusPublish<ReferenceRecurses>(referenceRecurses);
			return isRecursion;
		}

		#endregion

		#region Link 
		public LinkDTO ReadLinkOfNeuron(TID neuronId, TID linkId)
		{
			LinkData linkData = _neuronAS.ReadLinkOfNeuron(neuronId, linkId);
			LinkDTO linkDTO = Mapper.Map<LinkDTO>(linkData);
			LinkRead linkRead = new LinkRead() { NeuronId = neuronId, LinkId = linkId, LinkDTO = linkDTO };
			_interactionAPI.EBusPublish<LinkRead>(linkRead);
			return linkDTO;
		}
		public void WriteLinkOfNeuron(TID neuronId, TID linkId, LinkDTO linkDTO)
		{
			LinkData linkData = Mapper.Map<LinkData>(linkDTO);
			_neuronAS.WriteLinkOfNeuron(neuronId, linkId, linkData);
			LinkWritten linkWritten = new LinkWritten() { NeuronId = neuronId, LinkId = linkId, LinkDTO = linkDTO };
			_interactionAPI.EBusPublish<LinkWritten>(linkWritten);
		}
		public void AddLinkToNeuron(TID neuronId, TID linkId, LinkDTO linkDTO)
		{
			LinkData linkData = Mapper.Map<LinkData>(linkDTO);
			_neuronAS.AddLinkToNeuron(neuronId, linkId, linkData);
			LinkAdded linkAdded = new LinkAdded() { NeuronId = neuronId, LinkId = linkId, LinkDTO = linkDTO };
			_interactionAPI.EBusPublish<LinkAdded>(linkAdded);
		}
		public void RemoveLinkFromNeuron(TID neuronId, TID linkId)
		{
			_neuronAS.RemoveLinkFromNeuron(neuronId, linkId);
			LinkRemoved linkRemoved = new LinkRemoved() { NeuronId = neuronId, LinkId = linkId };
			_interactionAPI.EBusPublish<LinkRemoved>(linkRemoved);
		}
		#endregion
	}

}
