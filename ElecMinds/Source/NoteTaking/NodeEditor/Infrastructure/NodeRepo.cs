using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#region Dependency
using NoteTaking.Domain;

#endregion

namespace NoteTaking.Infrastructure
{
	public class NodeRepository: INodeRepository
	{
		protected readonly IServiceProvider _serviceProvider;
		public NodeRepository(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public Node CreateNode()
		{
			return new Node(new NodeData());////
		}
		public Node FetchNode(Guid nodeId)
		{
			return new Node(new NodeData());////
		}
		public void DeleteNode(Guid nodeId)
		{

		}
		public void RecoverNode(NodeData nodeData)
		{

		}
		protected Node LoadNode(Guid nodeId)
		{
			return new Node(new NodeData());////
		}
		protected void SaveNode(Guid nodeId)
		{

		}
	}
}
