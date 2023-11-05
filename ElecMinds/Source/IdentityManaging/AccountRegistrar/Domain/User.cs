using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Dependency
using NoteTaking.Domain;

#endregion

namespace IdentityManaging.Domain
{
	public class UserData
	{
		public List<Studio>? RecentStudios { get; set; }

		// preferences...
	}
	public class User:UserData
	{

	}
}
