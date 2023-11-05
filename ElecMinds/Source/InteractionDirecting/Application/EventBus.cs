using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractionDirecting.Application
{
	public class EventBus
	{
		private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

		public void Subscribe<TEvent>(Action<TEvent> handler)
		{
			var eventType = typeof(TEvent);
			if (!_subscribers.ContainsKey(eventType))
			{
				_subscribers[eventType] = new List<Delegate>();
			}
			if
			(!_subscribers[eventType].Contains((Delegate)handler))
			{
				_subscribers[eventType].Add((Delegate)handler);
			}

		}

		public void Unsubscribe<TEvent>(Action<TEvent> handler)
		{
			var eventType = typeof(TEvent);
			if (_subscribers.ContainsKey(eventType))
			{
				_subscribers[eventType].Remove(handler);
			}
		}

		public void Publish<TEvent>(TEvent eventToPublish)
		{
			var eventType = typeof(TEvent);
			if (_subscribers.ContainsKey(eventType))
			{
				foreach (var handler in _subscribers[eventType])
				{
					((Action<TEvent>)handler)(eventToPublish);
				}
			}
		}
	}
}
