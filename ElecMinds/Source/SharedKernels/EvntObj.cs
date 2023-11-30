
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Dependency

using Config;
using DTOs;
#endregion
namespace EvntObj // [Event Bus] Event Objects
{
	#region
	#endregion
	#region NoteTaking.NeuronApplicationService
	public class NeuronCreated
	{
		public TID NeuronId { get; set; }
	}
	public class NeuronDeleted
	{
		public TID NeuronId { get; set;}
	}
	public class NeuronRead
	{
		public TID NeuronId { get; set; }
		public NeuronDTO NeuronDTO { get; set; }
	}
	public class NeuronWritten : NeuronRead { }
	
	public class NoteRead
	{
		public TID NeuronId { get; set;}
		public NoteDTO NoteDTO { get; set;}
	}
	public class NoteWritten : NoteRead 
	{
		public Dictionary<TID, ReferenceDTO> NewReferenceDTOPairs {  get; set; }
	}

	public class ReferenceRecurses
	{
		public TID NeuronId { get; set;}
		public TID ReferenceNeuronId { get; set;}
	}
	public class LinkRead
	{
		public TID NeuronId { get; set;}
		public TID LinkId { get; set;}
		public LinkDTO LinkDTO { get; set;}
	}
	public class LinkWritten: LinkRead { }
	public class LinkRemoved
	{
		public TID NeuronId { get; set;}
		public TID LinkId { get; set;}
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
