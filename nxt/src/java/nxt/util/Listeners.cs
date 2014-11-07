using System.Collections.Generic;

namespace nxt.util
{


	public sealed class Listeners<T, E> where E : Enum<E>
	{

		private readonly ConcurrentHashMap<Enum<E>, IList<Listener<T>>> listenersMap = new ConcurrentHashMap<>();

		public bool addListener(Listener<T> listener, Enum<E> eventType)
		{
			lock (eventType)
			{
				IList<Listener<T>> listeners = listenersMap.get(eventType);
				if(listeners == null)
				{
					listeners = new CopyOnWriteArrayList<>();
					listenersMap.put(eventType, listeners);
				}
				return listeners.Add(listener);
			}
		}

		public bool removeListener(Listener<T> listener, Enum<E> eventType)
		{
			lock (eventType)
			{
				IList<Listener<T>> listeners = listenersMap.get(eventType);
				if(listeners != null)
				{
					return listeners.Remove(listener);
				}
			}
			return false;
		}

		public void notify(T t, Enum<E> eventType)
		{
			IList<Listener<T>> listeners = listenersMap.get(eventType);
			if(listeners != null)
			{
				foreach (Listener<T> listener in listeners)
				{
					listener.notify(t);
				}
			}
		}

	}

}