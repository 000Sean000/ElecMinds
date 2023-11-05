
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Dependency
using DTOs;
#endregion
namespace EvntObj // [Event Bus] Event Objects
{
	#region
	#endregion
	#region NoteTaking.NodeApplicationService
	public class NodeCreated
	{
		public Guid NodeId { get; set; }
	}
	public class NodeDeleted
	{
		public Guid NodeId { get; set;}
	}
	public class NodeRead
	{
		public Guid NodeId { get; set; }
		public NodeDTO NodeDTO { get; set; }
	}
	public class NodeWritten : NodeRead { }
	
	public class NoteRead
	{
		public Guid NodeId { get; set;}
		public NoteDTO NoteDTO { get; set;}
	}
	public class NoteWritten : NoteRead 
	{
		public Dictionary<Guid, ReferenceDTO> NewReferenceDTOPairs {  get; set; }
	}

	public class ReferenceRecurses
	{
		public Guid NodeId { get; set;}
		public Guid ReferenceNodeId { get; set;}
	}
	public class LinkRead
	{
		public Guid NodeId { get; set;}
		public Guid LinkId { get; set;}
		public LinkDTO LinkDTO { get; set;}
	}
	public class LinkWritten: LinkRead { }
	public class LinkRemoved
	{
		public Guid NodeId { get; set;}
		public Guid LinkId { get; set;}
	}
	public class LinkAdded:LinkRemoved
	{
		public LinkDTO LinkDTO { get; set;}
	}
	#endregion
	#region
	#endregion
	#region
	#endregion

}
