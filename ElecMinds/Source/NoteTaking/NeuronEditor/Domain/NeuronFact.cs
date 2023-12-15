using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
#region Dependency

using Config;

#endregion
namespace NoteTaking.Domain
{
	public class NeuronFactory:INeuronFactory
	{
		protected readonly IServiceProvider _serviceProvider;
		protected readonly INeuronRepository _repo;
		public NeuronFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			_repo = _serviceProvider.GetRequiredService<INeuronRepository>(); 
		}
		public Neuron MakeNeuron(NeuronData neuronData)
		{
			TID neuronId = (TID)neuronData.Id;
			Neuron neuron;
			if (neuronId != null)
			{
				_repo.RecoverNeuron(neuronId);
				neuron  = _repo.FetchNeuron(neuronId);
			}
			else
			{
				neuron = _repo.CreateNeuron();
			}
			neuron.Write(neuronData);
			return neuron;
		}
		public LinkData MakeLink(LinkData linkData)
		{
			TID linkId = (TID)linkData.Id;
			LinkData link;
			if (linkId != null)
			{
				_repo.RecoverLink(linkId);
				link = _repo.FetchLink(linkId);
			}
			else
			{
				link = _repo.CreateLink();
			}
			link.Write(linkData);
			return link;
		}
		public ReferenceData MakeReference(ReferenceData referenceData)
		{
			TID referenceId = (TID)referenceData.Id;
			ReferenceData reference;
			if (referenceId != null)
			{
				_repo.RecoverReference(referenceId);
				reference = _repo.FetchReference(referenceId);
			}
			else
			{
				reference = _repo.CreateReference();
			}
			reference.Write(referenceData);
			return reference;
		}
	}
}
