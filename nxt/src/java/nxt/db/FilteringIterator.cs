using System.Collections.Generic;

namespace nxt.db
{


	public sealed class FilteringIterator<T> : IEnumerator<T>, IEnumerable<T>, AutoCloseable
	{

		public interface Filter<T>
		{
			bool ok(T t);
		}

		private readonly DbIterator<T> dbIterator;
		private readonly Filter<T> filter;
		private readonly int from;
		private readonly int to;
		private T next;
		private bool hasNext;
		private bool iterated;
		private int count;

		public FilteringIterator(DbIterator<T> dbIterator, Filter<T> filter) : this(dbIterator, filter, 0, int.MAX_VALUE)
		{
		}

		public FilteringIterator(DbIterator<T> dbIterator, int from, int to) : this(dbIterator, new Filter<T>() { Override public bool ok(T t) { return true; } }, from, to)
		{
		}

		public FilteringIterator(DbIterator<T> dbIterator, Filter<T> filter, int from, int to)
		{
			this.dbIterator = dbIterator;
			this.filter = filter;
			this.from = from;
			this.to = to;
		}

		public override bool hasNext()
		{
			if(hasNext)
			{
				return true;
			}
			while(dbIterator.hasNext() && count <= to)
			{
				next = dbIterator.next();
				if(filter.ok(next))
				{
					if(count >= from)
					{
						count += 1;
						hasNext = true;
						return true;
					}
					count += 1;
				}
			}
			hasNext = false;
			return false;
		}

		public override T next()
		{
			if(hasNext)
			{
				hasNext = false;
				return next;
			}
			while(dbIterator.hasNext() && count <= to)
			{
				next = dbIterator.next();
				if(filter.ok(next))
				{
					if(count >= from)
					{
						count += 1;
						hasNext = false;
						return next;
					}
					count += 1;
				}
			}
			throw new NoSuchElementException();
		}

		public override void close()
		{
			dbIterator.close();
		}

		public override void remove()
		{
			throw new UnsupportedOperationException();
		}

		public override IEnumerator<T> iterator()
		{
			if(iterated)
			{
				throw new IllegalStateException("Already iterated");
			}
			iterated = true;
			return this;
		}

	}

}