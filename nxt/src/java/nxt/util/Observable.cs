namespace nxt.util
{

	public interface Observable<T, E> where E : Enum<E>
	{

		bool addListener(Listener<T> listener, E eventType);

		bool removeListener(Listener<T> listener, E eventType);

	}

}