using Config;
using DTOs;
using Enums;
using NoteTaking.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoteTaking.Domain
{

	public class NodeData : INodeDTO<NoteData, NoteSegment>
	{

		public TID? Id { get; set; }
		public ENeuronClass? NeuronClass { get; set; }
		public string? ImagePath { get; set; }

		#region Aggregate Members
		public NoteData? NoteData { get; set; }
		public List<TID>? OutLinkIDs { get; set; }
		public List<TID>? InLinkIDs { get; set; }
		public List<TID>? OutReferenceIDs { get; set; }
		public List<TID>? InReferenceIDs { get; set; }
		#endregion



		public NodeData() { }
		public NodeData(NodeData neuronData)
		{
			Overwrite(neuronData);
		}

		public void Write(NodeData nodeData)
		{
			if (nodeData.Id != null)
			{
				Id = nodeData.Id;
			}
			if (nodeData.NeuronClass != null)
			{
				NeuronClass = nodeData.NeuronClass;
			}
			if (nodeData.ImagePath != null)
			{
				ImagePath = nodeData.ImagePath;
			}
		}
		protected void Overwrite(NodeData nodeData)
		{
			///neuronData = neuronData.DeepCopy();
			Id = nodeData.Id;
			NeuronClass = nodeData.NeuronClass;
			ImagePath = nodeData.ImagePath;

			NoteData = nodeData.NoteData;
			InLinkIDs = nodeData.InLinkIDs;
			OutLinkIDs = nodeData.OutLinkIDs;
			InReferenceIDs = nodeData.InReferenceIDs;
			OutReferenceIDs = nodeData.OutReferenceIDs;
		}
		public NodeData Read()
		{
			return DeepCopy();
		}
		public NodeData DeepCopy()
		{
			NodeData nodeData = new NodeData();

			nodeData.Id = Id;
			nodeData.NeuronClass = NeuronClass;
			nodeData.ImagePath = ImagePath;
			nodeData.NoteData = NoteData?.DeepCopy();
			nodeData.InLinkIDs = InLinkIDs;
			nodeData.OutLinkIDs = OutLinkIDs;
			nodeData.InReferenceIDs = InReferenceIDs;
			nodeData.OutReferenceIDs = OutReferenceIDs;
			return nodeData;
		}
	}

}
