using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PKG
{
	public static class Test
	{
		public static void test()
		{
			SynchronizedCollection<int> synchronizedCollection = new SynchronizedCollection<int>();

			// Number of threads for writing and reading
			int numWriterThreads = 5;
			int numReaderThreads = 5;

			// Create writer tasks
			List<Task> writerTasks = new List<Task>();
			for (int i = 0; i < numWriterThreads; i++)
			{
				Task writerTask = Task.Run(() =>
				{
					for (int j = 0; j < 1000; j++)
					{
						synchronizedCollection.Add(j);
					}
				});
				writerTasks.Add(writerTask);
			}

			// Create reader tasks
			List<Task> readerTasks = new List<Task>();
			for (int i = 0; i < numReaderThreads; i++)
			{
				Task readerTask = Task.Run(() =>
				{
					foreach (int item in synchronizedCollection)
					{
						// Do something with the item
						// Here, we just print the item for simplicity
						Debug.WriteLine(item);
					}
				});
				readerTasks.Add(readerTask);
			}

			// Wait for all writer and reader tasks to complete
			Task.WaitAll(writerTasks.ToArray());
			Task.WaitAll(readerTasks.ToArray());

			Debug.WriteLine("All tasks completed.");
		}

	}
}
