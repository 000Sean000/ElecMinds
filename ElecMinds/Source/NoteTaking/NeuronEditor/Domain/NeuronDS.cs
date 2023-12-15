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
using PKG;
#endregion

namespace NoteTaking.Domain
{
	public interface INeuronRepository // be used by NeuronDomainService
	{
		public void ConnectStudioDB(string studioDBPath); // connect database of the specified Studio
		public Neuron CreateNeuron(); // insert an empty Neuron into database table, then return the instanciated Neuron
		public LinkData CreateLink();
		//public LinkData CreateLinkInNeuron(TID neuronId); // insert an empty Link into database table, then return created Link to let NeuronDomainService manage aggregate
		public ReferenceData CreateReference();
		//public ReferenceData CreateReferenceInNeuron(TID neuronId); // insert an empty Reference into database table, then return created Reference to let NeuronDomainService manage aggregate
		public Neuron FetchNeuron(TID neuronId); // Fetch Neuron by Neuron ID from database or cache
		public Neuron FetchSourceNeuronOfLink(TID linkId);
		public Neuron FetchSourceNeuronOfReference(TID referenceId);
		public void DeleteNeuron(TID neuronId); // also delete aggregate member entities
		public Neuron RecoverNeuron(TID neuronId); // Undo DeleteNeuron()
		public void DeleteLink(TID linkId);
		public void RecoverLink(TID linkId);
		public void DeleteReference(TID referenceId);
		public void RecoverReference(TID referenceId);
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
		public TID CreateNewNeuron()
		{
			Neuron neuron = _neuronRepo.CreateNeuron();
			return (TID)neuron.Id;
		}
		public void DeleteNeuron(TID neuronId)
		{
			_neuronRepo.DeleteNeuron(neuronId);
		}
		public void RecoverNeuron(TID neuronId)
		{
			_neuronRepo.RecoverNeuron(neuronId);
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
		public void WriteNoteOfNeuron(TID neuronId, NoteData noteData, List<ReferenceData>? newReferenceDatas = null) 
		{
			if (newReferenceDatas == null) newReferenceDatas = new List<ReferenceData> (); // empty list, no reference.

			//// check whether reference recurses-> do this job in application service
			// suppose that references do not cause reursion
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);

			// write neuron's note
			neuron.WriteNote(noteData);
			
			// update neuron's outgoing references
			List<ReferenceData> oldOutReferenceDatas = neuron.OutReferenceDatas;
			Dictionary<TID, TID> removedOutReferenceNeuronIdPairs = new Dictionary<TID, TID>();
			foreach(ReferenceData oldReferenceData in oldOutReferenceDatas) // remvoe all old(stale) references whatever
			{
				TID oldReferenceId = (TID)oldReferenceData.Id;
				if (newReferenceDatas.FirstOrDefault(newReferenceData => newReferenceData.Id == oldReferenceId) == null)
				{
					removedOutReferenceNeuronIdPairs[oldReferenceId] = (TID)oldReferenceData.TargetNeuronId; // record those who won't show up in new reference list
				}
				neuron.RemoveReference(oldReferenceId);
			}
			foreach(ReferenceData newReferenceData in newReferenceDatas) // add new reference
			{
				neuron.AddReference((TID)newReferenceData.Id, newReferenceData);				
			}

			// update outgoing reference neuron's InReferenceIds			
			foreach (var kvp in removedOutReferenceNeuronIdPairs) // remove old incoming reference ID
			{
				Neuron oldReferencedNeuron = _neuronRepo.FetchNeuron(kvp.Value);
				oldReferencedNeuron.InReferenceIds.Remove(kvp.Key);
			}
			foreach (var newReferenceData in newReferenceDatas) // add new incoming reference ID
			{
				Neuron newReferencedNeuron = _neuronRepo.FetchNeuron((TID)newReferenceData.TargetNeuronId);
				if (newReferencedNeuron.InReferenceIds.Contains((TID)newReferenceData.Id)) // remove old ID before add same ID
				{
					newReferencedNeuron.InReferenceIds.Remove((TID)newReferenceData.Id);
				}
				newReferencedNeuron.InReferenceIds.Add((TID)newReferenceData.Id);

			}

			// expire incoming reference neuron's dereference
			foreach (var inReferenceId in neuron.InReferenceIds)
			{
				Neuron referencingNeuron = _neuronRepo.FetchSourceNeuronOfReference(inReferenceId);
				referencingNeuron.ExpireNoteDereference(inReferenceId);
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
			targetNeuron.InLinkIds.Add(linkId);
		}
		public void RemoveLinkFromNeuron(TID neuronId, TID linkId)
		{
			Neuron neuron = _neuronRepo.FetchNeuron(neuronId);

			// update target neuron's incoming links
			Neuron targetNeuron = _neuronRepo.FetchSourceNeuronOfLink(linkId);
			targetNeuron.InLinkIds.Remove(linkId);

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
