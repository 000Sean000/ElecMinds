using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Dependency
using IdentityManaging.Domain;

#endregion

namespace ServiceOrchestrating.Domain
{
	public class AppData
	{
		string? ProgramDataPath { get; set; } // directory path to keep program data & ProgramFiles
		List<string>? MachineNames { get; set; } // when ProgramData is in a cloud drive
		List<User>? Users { get; set; }

	}
}
