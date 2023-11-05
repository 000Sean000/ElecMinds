using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Resources.ResXFileRef;

#region Dependency
using Enums;
using DTOs;
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
		public Guid? Id { get; set; }
		public Guid? SourceNodeId { get; set; } // will be used by target node to check back
		public Guid? TargetNodeId { get; set; }
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
			if (referenceData.SourceNodeId != null)
			{
				SourceNodeId = referenceData.SourceNodeId;
			}
			if (referenceData.TargetNodeId != null)
			{
				TargetNodeId = referenceData.TargetNodeId;
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
			SourceNodeId = referenceData.SourceNodeId;
			TargetNodeId = referenceData.TargetNodeId;
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
			referenceData.SourceNodeId = SourceNodeId;
			referenceData.TargetNodeId = TargetNodeId;
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
				Id = default(Guid);
			}
			if (SourceNodeId == null)
			{
				SourceNodeId = default(Guid);
			}
			if (TargetNodeId == null)
			{
				TargetNodeId = default(Guid);
			}
			if (DereferencerType == null)
			{
				DereferencerType = default(EDereferencerType);
			}
		}

	}
}
