using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region Dependency
using Const;

#endregion

namespace NoteTaking.Domain
{
	public interface IStudioRepository
	{
		public Studio CreateStudio();
		public Studio FetchStudio(Guid studioId);
	}

	public class StudioData
	{
		public Guid? Id { get; set; }
		public string? Name { get; set; }
		public string? DirPath { get; set; } // Directory Path
		public StudioData() { }
		public StudioData(StudioData studioData)
		{
			Overwrite(studioData);
		}
		public void Write(StudioData studioData)
		{

		}
		protected void Overwrite(StudioData studioData)
		{

		}
		public StudioData Read()
		{
			return DeepCopy();
		}
		public StudioData DeepCopy()
		{
			return new StudioData();
		}
	
	}
	public class Studio: StudioData
	{
		public string? NoteRepoPath;
	}
}
