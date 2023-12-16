using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#region Dependency
using Config;
using NoteTaking.API;
using BasicService;
using DTOs;
#endregion
namespace DataStructuring.Domain
{
	public class Unit
	{
		protected TID _rootId;
		public Unit(TID rootId)
		{
			_rootId = rootId;
		}

	}
	public class Group: Unit
	{
		public List<TID> Members { get; set; }
		public Group(TID rootId):base(rootId)
		{
			Members = new List<TID>();
		}
		public 
	}
}
