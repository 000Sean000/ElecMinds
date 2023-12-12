using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PKG
{
	
	public static class ListPKG
	{
		/*
			To populate data to original instance loaded from database.
			=> compare 2 List, then do update, add, remove on original list according to the change of timely list.
			original list is for database, such as List<LinkData>;
			timely list is for domain service , such as List<Link>;
		*/
		public static void UpdateEntityList<TOriginal, TTimely>(
			List<TOriginal> originalList,
			List<TTimely> timelyList,
			Func<TOriginal, TTimely, bool> match, //  matching condition between an item of type TOriginal and an item of type TTimely. return true/false; It's used to determine if two items are considered equivalent or correspond to each other (typically by comparing IDs or some other unique identifier).
			Action<TOriginal, TTimely> update,
			Func<TTimely, TOriginal> createNew // define how a new TOriginal type object is created from a TTimely type object. 
			)
		{
			// Handle additions and updates
			foreach (var timelyItem in timelyList)
			{
				var originalItem = originalList.FirstOrDefault(ol => match(ol, timelyItem));
				// FirstOrDefault is a LINQ method used to search for an element in a collection that matches a certain condition. If it finds an element that satisfies the condition, it returns that element; otherwise, it returns the default value for the type (null for reference types, 0 for numeric types, etc.).
				if (originalItem == null)
				{
					// Add new item
					originalList.Add(createNew(timelyItem));
				}
				else
				{
					// Update existing item
					update(originalItem, timelyItem);
				}
			}

			// Handle deletions
			originalList.RemoveAll(ol => !timelyList.Any(tl => match(ol, tl)));
			// RemoveAll() is a LINQ method to remove all elements from the list that match a specific condition.
			// Any() is a LINQ method used to check whether any elements in a collection satisfy a particular condition.
		}
	}
}
