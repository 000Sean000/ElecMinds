using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Resources.ResXFileRef;

#region Dependency
using Enums;
using DTOs;
using Config;
#endregion

namespace NoteTaking.Domain
{
	public delegate string Dereferencer(string text, object? arg = null);
	
	public static class RefConfig
	{
		public static Dictionary<EDereferencerType, Dereferencer> Dereferencer { get;}
		static RefConfig()
		{
			Dereferencer = new Dictionary<EDereferencerType, Dereferencer>();
			Dereferencer[EDereferencerType.Direct] = DirectConvert;
		}
		public static string DirectConvert(string text, object? arg = null)
		{
			return text;
		}
	}
	public class ReferenceData: IReferenceDTO
	{
		public TID? Id { get; set; }
		public TID? SourceNeuronId { get; set; } // will be used by target neuron to check back
		public TID? TargetNeuronId { get; set; }
		public EDereferencerType? DereferencerType { get; set; }


		public ReferenceData() { }

		public ReferenceData(ReferenceData? referenceData)
		{
			if (referenceData != null)
			{
				Overwrite(referenceData);
			}
		}

		public void Write(ReferenceData referenceData)
		{
			if (referenceData.Id != null)
			{
				Id = referenceData.Id;
			}
			if (referenceData.SourceNeuronId != null)
			{
				SourceNeuronId = referenceData.SourceNeuronId;
			}
			if (referenceData.TargetNeuronId != null)
			{
				TargetNeuronId = referenceData.TargetNeuronId;
			}
			
			if (referenceData.DereferencerType != null)
			{
				DereferencerType = referenceData.DereferencerType;
			}
		}
		protected void Overwrite(ReferenceData referenceData)
		{
			///referenceData = referenceData.DeepCopy();

			Id = referenceData.Id;
			SourceNeuronId = referenceData.SourceNeuronId;
			TargetNeuronId = referenceData.TargetNeuronId;
			DereferencerType = referenceData.DereferencerType;

		}


		public ReferenceData Read()
		{
			return DeepCopy();
		}
		public ReferenceData DeepCopy()
		{
			ReferenceData referenceData = new ReferenceData();

			referenceData.Id = Id;
			referenceData.SourceNeuronId = SourceNeuronId;
			referenceData.TargetNeuronId = TargetNeuronId;
			referenceData.DereferencerType = DereferencerType;
			return referenceData;
		}
	}

	public class Reference: ReferenceData
	{

		public Dereferencer? Dereferencer { get; set; }

		public Reference() : base()
		{
			EnsurePropertyNotNull();
		}
		public Reference(ReferenceData? referenceData) : base(referenceData)
		{
			EnsurePropertyNotNull();
		}
		public void EnsurePropertyNotNull()
		{
			if (Id == null)
			{
				Id = default(TID);
			}
			if (SourceNeuronId == null)
			{
				SourceNeuronId = default(TID);
			}
			if (TargetNeuronId == null)
			{
				TargetNeuronId = default(TID);
			}
			if (DereferencerType == null)
			{
				DereferencerType = default(EDereferencerType);
			}
		}

	}
}
