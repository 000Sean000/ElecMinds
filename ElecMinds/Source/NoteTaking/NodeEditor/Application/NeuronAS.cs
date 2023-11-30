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
using Config;
using NoteTaking.Domain;
using EvntObj;
using InteractionDirecting.API;

#endregion

namespace NoteTaking.Application
{
	public class NeuronApplicationService
	{
		protected readonly IServiceProvider _serviceProvider;
		protected INeuronRepository _neuronRepository;
		protected NeuronDomainService _neuronDS;
		protected InteractionDirecting.API.IInteractionAPI _interactionAPI;
		
		public NeuronApplicationService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_neuronDS = serviceProvider.GetRequiredService<NeuronDomainService>();

			_interactionAPI = serviceProvider.GetService<InteractionDirecting.API.IInteractionAPI>();
		}

		#region Neuron		
		public TID CreateNewNeuron()
		{
			CreateNewNeuron cmd = new CreateNewNeuron(_neuronDS);
			_interactionAPI.Execute(cmd);
			return (TID)cmd.NeuronData.Id;
		}
		public void DeleteNeuron(TID neuronId)
		{
			DeleteNeuron cmd = new DeleteNeuron(_neuronDS, neuronId);
			_interactionAPI.Execute(cmd);
		}
		public NeuronData ReadNeuron(TID neuronId)
		{
			return _neuronDS.ReadNeuron(neuronId);
		}
		public void WriteNeuron(TID neuronId, NeuronData neuronData)
		{
			WriteNeuron cmd = new WriteNeuron(_neuronDS, neuronId, neuronData);
			_interactionAPI.Execute(cmd);

		}
		#endregion

		#region Note
		public NoteData ReadNoteOfNeuron(TID neuronId)
		{
			return _neuronDS.ReadNoteOfNeuron(neuronId);
		}
		public void WriteNoteOfNeuron(TID neuronId, NoteData noteData, Dictionary<TID, ReferenceData> newReferenceDataPairs)
		{
			WriteNoteOfNeuron cmd = new WriteNoteOfNeuron(_neuronDS, neuronId, noteData, newReferenceDataPairs);
			_interactionAPI.Execute(cmd);

		}
		public bool DoesReferencenRecurseInNeuron(TID neuronId, TID referenceNeuronId)
		{
			return _neuronDS.DoesReferencenRecurseInNeuron(neuronId, referenceNeuronId);
		}

		#endregion

		#region Link 
		public LinkData ReadLinkOfNeuron(TID neuronId, TID linkId)
		{
			return _neuronDS.ReadLinkOfNeuron(neuronId, linkId);
		}
		public void WriteLinkOfNeuron(TID neuronId, TID linkId, LinkData linkData)
		{
			WriteLinkOfNeuron cmd = new WriteLinkOfNeuron(_neuronDS, neuronId, linkId, linkData);
			_interactionAPI.Execute(cmd);
		}
		public void AddLinkToNeuron(TID neuronId, TID linkId, LinkData linkData)
		{			
			AddLinkToNeuron cmd = new AddLinkToNeuron(_neuronDS, neuronId, linkId, linkData);
			_interactionAPI.Execute(cmd);
		}
		public void RemoveLinkFromNeuron(TID neuronId, TID linkId)
		{
			RemoveLinkFromNeuron cmd = new RemoveLinkFromNeuron(_neuronDS, neuronId, linkId); 
			_interactionAPI.Execute(cmd);
		}
		#endregion
	}
	#region 
	#endregion
	#region
	#endregion
	#region Command with Undo

	public class CreateNewNeuron : ICommandWithUndo
	{
		protected NeuronDomainService _neuronDS;
		public NeuronData? NeuronData { get; set; } 
		public CreateNewNeuron(NeuronDomainService neuronDS)
		{
			_neuronDS = neuronDS;
		}
		public void Execute()
		{
			if (NeuronData == null)
			{
				NeuronData = _neuronDS.CreateNewNeuron();
			}
			else
			{
				_neuronDS.RecoverNeuron(NeuronData);
			}
		}
		public void Undo()
		{
			_neuronDS.DeleteNeuron((TID)NeuronData.Id);
		}
	}
	public class DeleteNeuron : ICommandWithUndo
	{
		protected NeuronDomainService _neuronDS;
		public NeuronData NeuronData { get; set; }
		public DeleteNeuron(NeuronDomainService neuronDS, TID neuronId)
		{
			_neuronDS = neuronDS;
			NeuronData = _neuronDS.ReadNeuron(neuronId);
		}
		public void Execute()
		{
			_neuronDS.DeleteNeuron((TID)NeuronData.Id);
		}
		public void Undo()
		{
			_neuronDS.RecoverNeuron(NeuronData);
		}
	}
	public class WriteNeuron : ICommandWithUndo
	{
		protected NeuronDomainService _neuronDS;
		public TID NeuronId { get; set; }
		public NeuronData OldNeuronData { get; set; }
		public NeuronData NewNeuronData { get; set; }
		public WriteNeuron(NeuronDomainService neuronDS, TID neuronId, NeuronData neuronData)
		{
			_neuronDS = neuronDS;
			NeuronId = neuronId;
			OldNeuronData = _neuronDS.ReadNeuron(neuronId);
			NewNeuronData = neuronData;

		}
		public void Execute()
		{
			_neuronDS.WriteNeuron(NeuronId, NewNeuronData);
		}
		public void Undo()
		{
			_neuronDS.WriteNeuron(NeuronId, OldNeuronData);
		}
	}
	public class WriteNoteOfNeuron : ICommandWithUndo
	{
		protected NeuronDomainService _neuronDS;
		public TID NeuronId { get; set; }
		public NoteData OldNoteData { get; set; }
		public NoteData NewNoteData { get; set; }
		Dictionary<TID, ReferenceData> OldReferenceDataPairs { get; set; }
		Dictionary<TID, ReferenceData> NewReferenceDataPairs { get; set; }

		public WriteNoteOfNeuron(NeuronDomainService neuronDS, TID neuronId, NoteData noteData, Dictionary<TID, ReferenceData> newReferenceDataPairs)
		{
			_neuronDS = neuronDS;
			OldNoteData = _neuronDS.ReadNoteOfNeuron(neuronId);
			NewNoteData = noteData;
			OldReferenceDataPairs = _neuronDS.ReadNeuron(neuronId).OutReferenceData;
			NewReferenceDataPairs = newReferenceDataPairs;
		}
		public void Execute()
		{
			_neuronDS.WriteNoteOfNeuron(NeuronId, NewNoteData, NewReferenceDataPairs);
		}
		public void Undo()
		{
			_neuronDS.WriteNoteOfNeuron(NeuronId, OldNoteData, OldReferenceDataPairs);
		}
	}
	public class WriteLinkOfNeuron : ICommandWithUndo
	{
		protected NeuronDomainService _neuronDS;
		public TID NeuronId { get; set; }
		public TID LinkId { get; set; }
		public LinkData OldLinkData { get; set; }
		public LinkData NewLinkData { get; set; }
		public WriteLinkOfNeuron(NeuronDomainService neuronDS, TID neuronId, TID linkId, LinkData linkData)
		{
			_neuronDS = neuronDS;
			NeuronId = neuronId;
			LinkId = linkId;
			OldLinkData = _neuronDS.ReadLinkOfNeuron(neuronId, linkId);
			NewLinkData = linkData;
		}
		public void Execute()
		{
			_neuronDS.WriteLinkOfNeuron(NeuronId, LinkId, NewLinkData);
		}
		public void Undo()
		{
			_neuronDS.WriteLinkOfNeuron(NeuronId, LinkId, OldLinkData);
		}
	}
	public class AddLinkToNeuron : ICommandWithUndo
	{
		protected NeuronDomainService _neuronDS;
		public TID NeuronId { get; set; }
		public TID LinkId { get; set; }
		public LinkData LinkData { get; set; }
		public AddLinkToNeuron(NeuronDomainService neuronDS, TID neuronId, TID linkId, LinkData linkData)
		{
			_neuronDS = neuronDS;
			NeuronId = neuronId;
			LinkId = linkId;
			LinkData = linkData;
		}
		public void Execute()
		{
			_neuronDS.AddLinkToNeuron(NeuronId, LinkId, LinkData);
		}
		public void Undo()
		{
			_neuronDS.RemoveLinkFromNeuron(NeuronId, LinkId);
		}
	}
	public class RemoveLinkFromNeuron : ICommandWithUndo
	{
		protected NeuronDomainService _neuronDS;
		public TID NeuronId { get; set; }
		public TID LinkId { get; set; }
		public LinkData LinkData { get; set; }
		public RemoveLinkFromNeuron(NeuronDomainService neuronDS, TID neuronId, TID linkId)
		{
			_neuronDS = neuronDS;
			NeuronId = neuronId;	
			LinkId = linkId;
			LinkData = _neuronDS.ReadLinkOfNeuron(neuronId, linkId);
		}
		public void Execute()
		{
			_neuronDS.RemoveLinkFromNeuron(NeuronId, LinkId) ;
		}
		public void Undo()
		{
			_neuronDS.AddLinkToNeuron(NeuronId, LinkId, LinkData) ;
		}
	}




	#endregion
	public class Op : ICommandWithUndo
	{
		protected NeuronDomainService _neuronDS;

		public Op(NeuronDomainService neuronDS)
		{
			_neuronDS = neuronDS;
		}
		public void Execute()
		{

		}
		public void Undo()
		{

		}
	}

}
