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

using Config;
using Enums;
#endregion

namespace NoteTaking.Domain
{
	public interface INeuronRepository // be used by NeuronDomainService
	{
		public void ConnectStudioDB(string studioDBPath); // connect database of the specified Studio
		public Neuron CreateNeuron(); // insert an empty Neuron into database table, then return the instanciated Neuron
		public TID CreateLinkInNeuron(TID neuronId); // insert an empty Link into database table, then return the ID of created Link to let NeuronDomainService manage aggregate
		public TID CreateReferenceInNeuron(TID neuronId); // insert an empty Reference into database table, then return the ID of created Reference to let NeuronDomainService manage aggregate
		public Neuron FetchNeuron(TID neuronId); // Fetch Neuron by Neuron ID from database or cache
		public Neuron FetchSourceNeuronOfLink(TID LinkId);
		public Neuron FetchSourceNeuronOfReference(TID ReferenceId);
		public void DeleteNeuron(TID neuronId); // also delete aggregate member entities
		public void RecoverNeuron(NeuronData neuronData); // Undo DeleteNeuron()
		public void SaveChanges(); // only save changes to database in User's order
	}

	// don't return Aggregate instance to outside, just return data instance
	public class NeuronDomainService: INeuronDomainService
	{
		protected INeuronRepository _neuronRepo { get; set; }

		public NeuronDomainService(IServiceProvider serviceProvider)
		{
			_neuronRepo = serviceProvider.GetRequiredService<INeuronRepository>();
		}
		#region Neuron
		public NeuronData CreateNewNeuron()
		{
			Neuron neuron = _neuronRepo.CreateNeuron();
			return neuron.Read();
		}
		public void DeleteNeuron(TID neuronId)
		{
			_neuronRepo.DeleteNeuron(neuronId);
		}
		public void RecoverNeuron(NeuronData neuronData)
		{
			_neuronRepo.RecoverNeuron(neuronData);
		}
		public NeuronData ReadNeuron(TID neuronId)
		{
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);
			return neuron.Read();
		}
		public void WriteNeuron(TID neuronId, NeuronData neuronData)
		{
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);
			neuron.Write(neuronData);
		}
		#endregion

		#region Note
		public void WriteNoteOfNeuron(TID neuronId, NoteData noteData, Dictionary<TID, ReferenceData> newReferenceDataPairs) 
		{
			//// check whether reference recurses-> do this job in application service
			// suppose that references do not cause reursion
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);

			// write neuron's note
			neuron.WriteNote(noteData);
			
			// update neuron's outgoing references
			Dictionary<TID, ReferenceData> oldOutReferenceData = neuron.OutReferenceData;
			Dictionary<TID, TID> removedOutReferenceNeuronIds = new Dictionary<TID, TID>();
			foreach(var oldReferenceId in neuron.OutReferenceData.Keys) // remvoe all old references
			{
				if (!newReferenceDataPairs.ContainsKey(oldReferenceId))
				{
					removedOutReferenceNeuronIds[oldReferenceId] = (TID)oldOutReferenceData[oldReferenceId].TargetNeuronId;
				}
				neuron.RemoveReference(oldReferenceId);
			}
			foreach(var kvp in newReferenceDataPairs) // add new reference
			{
				neuron.AddReference(kvp.Key, kvp.Value);				
			}

			// update outgoing reference neuron's InReferenceNeuronIdPairs

			foreach (var kvp in removedOutReferenceNeuronIds) // remove old reference
			{
				Neuron removedNeuron = _neuronRepo.FetchNeuron(kvp.Value);
				removedNeuron.RemoveReference(kvp.Key);
			}
			foreach (var kvp in newReferenceDataPairs) // add new reference
			{
				Neuron outNeuron = _neuronRepo.FetchNeuron((TID)kvp.Value.TargetNeuronId);
				outNeuron.AddReference(kvp.Key, kvp.Value);
			}

			// expire incoming reference neuron's dereference
			foreach (var kvp in neuron.InReferenceNeuronIdPairs)
			{
				Neuron inNeuron = _neuronRepo.FetchNeuron(kvp.Value);
				inNeuron.ExpireNoteDereference(kvp.Key);
			}
		
		}
		public NoteData ReadNoteOfNeuron(TID neuronId)
		{
			return _neuronRepo.FetchNeuron(neuronId).ReadNote();
		}
		/*
		public void ExpireNoteDereferenceOfNeuron(TID neuronId, TID referenceId)
		{
			Neuron neuron = NeuronRepo.FetchNeuron(neuronId);
			neuron.ExpireNoteDereference(referenceId);
		}
		*/
		public string GetNoteDereferenceOfNeuron(TID neuronId)
		{
			return GetNoteDereferenceWithUpdate(new List<TID> { neuronId });
		}
		protected string GetNoteDereferenceWithUpdate(List<TID> branchVisitedNeuronIds)
		{
			string dereference = string.Empty;

			TID neuronId = branchVisitedNeuronIds.Last();
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);
			
			List<NoteSegment> segments = neuron.ReadNote().Segments;
			foreach (var seg in segments)
			{
				if (seg.ReferenceId != null) // this is a reference
				{
					TID referenceId = (TID)seg.ReferenceId;
					TID referenceTargetNeuronId = (TID)neuron.ReadReference(referenceId).TargetNeuronId;
					
					if (seg.Text == null) // dereference had expired
					{
						if (branchVisitedNeuronIds.Contains((TID)referenceTargetNeuronId))
						{
							throw new Exception("[Recursive Reference!]");
						}
						else
						{
							List<TID> nextBranchVisitedNeuronIds = new List<TID>(branchVisitedNeuronIds);
							nextBranchVisitedNeuronIds.Add(referenceTargetNeuronId);
							string text = GetNoteDereferenceWithUpdate(nextBranchVisitedNeuronIds);
							EDereferencerType dereferencerType = (EDereferencerType)neuron.ReadReference(referenceId).DereferencerType;
							seg.Text = RefConfig.Dereferencer[dereferencerType](text);
						}
					}
				}
				dereference += seg.Text;
			}
			neuron.WriteNote(new NoteData() { Segments = segments });
			return dereference;
		}
		public bool DoesReferencenRecurseInNeuron(TID neuronId, TID referenceNeuronId)
		{
			List<TID> neuronIds = new List<TID>() { referenceNeuronId, neuronId };
			return IsReferenceRecursion(neuronIds);
		}
		protected bool IsReferenceRecursion(List<TID> branchVisitedNeuronIds)
		{
			
			TID neuronId = branchVisitedNeuronIds.Last();
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);

			List<NoteSegment> segments = neuron.ReadNote().Segments;
			foreach (var seg in segments)
			{
				if (seg.ReferenceId != null) // this is a reference
				{
					TID referenceId = (TID)seg.ReferenceId;
					TID referenceTargetNeuronId = (TID)neuron.ReadReference(referenceId).TargetNeuronId;

					if (branchVisitedNeuronIds.Contains((TID)referenceTargetNeuronId))
					{
						return true;
					}
					else
					{
						List<TID> nextBranchVisitedNeuronIds = new List<TID>(branchVisitedNeuronIds);
						nextBranchVisitedNeuronIds.Add(referenceTargetNeuronId);
						if (IsReferenceRecursion(nextBranchVisitedNeuronIds))
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
		public void WriteLinkOfNeuron(TID neuronId, TID linkId, LinkData linkData)
		{
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);
			neuron.WriteLink(linkId, linkData);
				
		}
		public LinkData ReadLinkOfNeuron(TID neuronId, TID linkId)
		{
			return _neuronRepo.FetchNeuron(neuronId).ReadLink(linkId);
		}
		public void AddLinkToNeuron(TID neuronId, TID linkId, LinkData linkData)
		{
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);

			neuron.AddLink((TID)linkId, linkData);

			// update target neuron's incoming links
			Neuron targetNeuron = _neuronRepo.FetchNeuron((TID)linkData.TargetNeuronId);
			targetNeuron.InLinkNeuronIdPairs[(TID)linkId] = neuronId;
		}
		public void RemoveLinkFromNeuron(TID neuronId, TID linkId)
		{
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);

			// update target neuron's incoming links
			TID targetNeuronId = neuron.InLinkNeuronIdPairs[linkId];
			Neuron targetNeuron = _neuronRepo.FetchNeuron(targetNeuronId);
			targetNeuron.InLinkNeuronIdPairs.Remove(linkId);

			neuron.RemoveLink(linkId);
		}
		#endregion

		#region Reference (Reference is only required in Note operation)
		/*
		public void WriteReferenceOfNeuron(TID neuronId, TID referenceId, ReferenceData referenceData)
		{

		}
		public ReferenceData ReadReferenceOfNeuron(TID neuronId, TID referenceId)
		{
			return NeuronRepo.FetchNeuron(neuronId).ReadReference(referenceId);
		}
		public void AddReferenceToNeuron(TID neuronId, TID referenceId, ReferenceData referenceData)
		{

		}
		public void RemoveReferenceToNeuron(TID neuronId, TID referenceId)
		{

		}
		*/
		#endregion

	}
}
