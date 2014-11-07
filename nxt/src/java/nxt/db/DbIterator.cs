using System;
using System.Collections.Generic;

namespace nxt.db
{


	public sealed class DbIterator<T> : IEnumerator<T>, IEnumerable<T>, AutoCloseable
	{

		public interface ResultSetReader<T>
		{
//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: T get(Connection con, ResultSet rs) throws Exception;
			T get(Connection con, ResultSet rs);
		}

		private readonly Connection con;
		private readonly PreparedStatement pstmt;
		private readonly ResultSetReader<T> rsReader;
		private readonly ResultSet rs;

		private bool hasNext;
		private bool iterated;

		public DbIterator(Connection con, PreparedStatement pstmt, ResultSetReader<T> rsReader)
		{
			this.con = con;
			this.pstmt = pstmt;
			this.rsReader = rsReader;
			try
			{
				this.rs = pstmt.executeQuery();
				this.hasNext = rs.next();
			}
			catch(SQLException e)
			{
				DbUtils.close(pstmt, con);
				throw new Exception(e.ToString(), e);
			}
		}

		public override bool hasNext()
		{
			if(! hasNext)
			{
				DbUtils.close(rs, pstmt, con);
			}
			return hasNext;
		}

		public override T next()
		{
			if(! hasNext)
			{
				DbUtils.close(rs, pstmt, con);
				throw new NoSuchElementException();
			}
			try
			{
				T result = rsReader.get(con, rs);
				hasNext = rs.next();
				return result;
			}
			catch(Exception e)
			{
				DbUtils.close(rs, pstmt, con);
				throw new Exception(e.ToString(), e);
			}
		}

		public override void remove()
		{
			throw new UnsupportedOperationException("Removal not suported");
		}

		public override void close()
		{
			DbUtils.close(rs, pstmt, con);
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