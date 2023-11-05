using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;

namespace PKG
{
	public static class threadPKG
	{
		//public delegate void Actions();

	}

	public class Resource<T>
	{
		private T data;
		private ReaderWriterLockSlim rwLock;

		public Resource()
		{
			data = default(T);
			rwLock = new ReaderWriterLockSlim();
		}

		public Resource(T initialValue)
		{
			data = initialValue;
			rwLock = new ReaderWriterLockSlim();
		}

		public T GetData()
		{
			rwLock.EnterReadLock();
			try
			{
				return DeepCopy(data);
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public void SetData(T newValue)
		{
			rwLock.EnterWriteLock();
			try
			{
				data = DeepCopy(newValue);
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		private static T DeepCopy(T source)
		{
			if (source == null)
			{
				return default(T);
			}

			if (source is ICloneable cloneable)
			{
				return (T)cloneable.Clone();
			}

			var json = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<T>(json);
		}
	}



	public class SynchronizedCollection<T> : IEnumerable<T>
	{
		private List<T> data;
		private ReaderWriterLockSlim rwLock;
		
		public SynchronizedCollection()
		{
			data = new List<T>();
			rwLock = new ReaderWriterLockSlim();
		}

		public int Count
		{
			get
			{
				rwLock.EnterReadLock();
				try
				{
					return data.Count;
				}
				finally
				{
					rwLock.ExitReadLock();
				}
			}
		}

		public T this[int index]
		{
			get
			{
				rwLock.EnterReadLock();
				try
				{
					return data[index];
				}
				finally
				{
					rwLock.ExitReadLock();
				}
			}
			set
			{
				rwLock.EnterWriteLock();
				try
				{
					data[index] = value;
				}
				finally
				{
					rwLock.ExitWriteLock();
				}
			}
		}

		public void Add(T item)
		{
			rwLock.EnterWriteLock();
			try
			{
				data.Add(item);
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public void Remove(T item)
		{
			rwLock.EnterWriteLock();
			try
			{
				data.Remove(item);
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public bool Contains(T item)
		{
			rwLock.EnterReadLock();
			try
			{
				return data.Contains(item);
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public void Clear()
		{
			rwLock.EnterWriteLock();
			try
			{
				data.Clear();
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		// Implementation of IEnumerable<T>
		public IEnumerator<T> GetEnumerator()
		{
			rwLock.EnterReadLock();
			try
			{
				foreach (T item in data)
				{
					yield return item;
				}
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		// Implementation of IEnumerable
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public class SynchronizedCollection<TKey, TValue> : IEnumerable<TValue>
	{
		private Dictionary<TKey, TValue> data;
		private ReaderWriterLockSlim rwLock;

		public SynchronizedCollection()
		{
			data = new Dictionary<TKey, TValue>();
			rwLock = new ReaderWriterLockSlim();
		}

		public int Count
		{
			get
			{
				rwLock.EnterReadLock();
				try
				{
					return data.Count;
				}
				finally
				{
					rwLock.ExitReadLock();
				}
			}
		}

		public TValue this[TKey key]
		{
			get
			{
				rwLock.EnterReadLock();
				try
				{
					return data[key];
				}
				finally
				{
					rwLock.ExitReadLock();
				}
			}
			set
			{
				rwLock.EnterWriteLock();
				try
				{
					data[key] = value;
				}
				finally
				{
					rwLock.ExitWriteLock();
				}
			}
		}

		public void Add(TKey key, TValue value)
		{
			rwLock.EnterWriteLock();
			try
			{
				data.Add(key, value);
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public void Remove(TKey key)
		{
			rwLock.EnterWriteLock();
			try
			{
				data.Remove(key);
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public bool ContainsKey(TKey key)
		{
			rwLock.EnterReadLock();
			try
			{
				return data.ContainsKey(key);
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public void Clear()
		{
			rwLock.EnterWriteLock();
			try
			{
				data.Clear();
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		// Implementation of IEnumerable<TValue>
		public IEnumerator<TValue> GetEnumerator()
		{
			rwLock.EnterReadLock();
			try
			{
				foreach (var value in data.Values)
				{
					yield return value;
				}
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		// Implementation of IEnumerable
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

}
