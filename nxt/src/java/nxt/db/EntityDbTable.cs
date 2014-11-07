using System;

namespace nxt.db
{

	using Nxt = nxt.Nxt;


	public abstract class EntityDbTable<T> : DerivedDbTable
	{

		private readonly bool multiversion;
		protected internal readonly DbKey.Factory<T> dbKeyFactory;
		private readonly string defaultSort;

		protected internal EntityDbTable(string table, DbKey.Factory<T> dbKeyFactory) : this(table, dbKeyFactory, false)
		{
		}

		internal EntityDbTable(string table, DbKey.Factory<T> dbKeyFactory, bool multiversion) : base(table)
		{
			this.dbKeyFactory = dbKeyFactory;
			this.multiversion = multiversion;
			this.defaultSort = " ORDER BY " + (multiversion ? dbKeyFactory.PKColumns : " height DESC ");
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract T load(Connection con, ResultSet rs) throws SQLException;
		protected internal abstract T load(Connection con, ResultSet rs);

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract void save(Connection con, T t) throws SQLException;
		protected internal abstract void save(Connection con, T t);

		protected internal virtual string defaultSort()
		{
			return defaultSort;
		}

		public void checkAvailable(int height)
		{
			if(multiversion && height < Nxt.BlockchainProcessor.MinRollbackHeight)
			{
				throw new System.ArgumentException("Historical data as of height " + height +" not available, set nxt.trimDerivedTables=false and re-scan");
			}
		}

		public T get(DbKey dbKey)
		{
			if(Db.InTransaction)
			{
				T t = (T)Db.getCache(table)[dbKey];
				if(t != null)
				{
					return t;
				}
			}
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + dbKeyFactory.PKClause + (multiversion ? " AND latest = TRUE LIMIT 1" : "")))
			{
				dbKey.PK = pstmt;
				return get(con, pstmt, true);
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public T get(DbKey dbKey, int height)
		{
			checkAvailable(height);
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + dbKeyFactory.PKClause + " AND height <= ?" + (multiversion ? " AND (latest = TRUE OR EXISTS (" + "SELECT 1 FROM " + table + dbKeyFactory.PKClause + " AND height > ?)) ORDER BY height DESC LIMIT 1" : "")))
			{
				int i = dbKey.PK = pstmt;
				pstmt.setInt(i, height);
				if(multiversion)
				{
					i = dbKey.setPK(pstmt, ++i);
					pstmt.setInt(i, height);
				}
				return get(con, pstmt, false);
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public T getBy(DbClause dbClause)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + " WHERE " + dbClause.Clause + (multiversion ? " AND latest = TRUE LIMIT 1" : "")))
			{
				dbClause.set(pstmt, 1);
				return get(con, pstmt, true);
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public T getBy(DbClause dbClause, int height)
		{
			checkAvailable(height);
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + " AS a WHERE " + dbClause.Clause + " AND height <= ?" + (multiversion ? " AND (latest = TRUE OR EXISTS (" + "SELECT 1 FROM " + table + " AS b WHERE " + dbKeyFactory.SelfJoinClause + " AND b.height > ?)) ORDER BY height DESC LIMIT 1" : "")))
			{
				int i = 0;
				i = dbClause.set(pstmt, ++i);
				pstmt.setInt(i, height);
				if(multiversion)
				{
					pstmt.setInt(++i, height);
				}
				return get(con, pstmt, false);
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private T get(Connection con, PreparedStatement pstmt, boolean cache) throws SQLException
		private T get(Connection con, PreparedStatement pstmt, bool cache)
		{
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean doCache = cache && Db.isInTransaction();
			bool doCache = cache && Db.InTransaction;
			using (ResultSet rs = pstmt.executeQuery())
			{
				if(!rs.next())
				{
					return null;
				}
				T t = null;
				DbKey dbKey = null;
				if(doCache)
				{
					dbKey = dbKeyFactory.newKey(rs);
					t = (T) Db.getCache(table)[dbKey];
				}
				if(t == null)
				{
					t = load(con, rs);
					if(doCache)
					{
						Db.getCache(table).Add(dbKey, t);
					}
				}
				if(rs.next())
				{
					throw new Exception("Multiple records found");
				}
				return t;
			}
		}

		public DbIterator<T> getManyBy(DbClause dbClause, int from, int to)
		{
			return getManyBy(dbClause, from, to, defaultSort());
		}

		public DbIterator<T> getManyBy(DbClause dbClause, int from, int to, string sort)
		{
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + " WHERE " + dbClause.Clause + (multiversion ? " AND latest = TRUE " : " ") + sort + DbUtils.limitsClause(from, to));
				int i = 0;
				i = dbClause.set(pstmt, ++i);
				i = DbUtils.setLimits(i, pstmt, from, to);
				return getManyBy(con, pstmt, true);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public DbIterator<T> getManyBy(DbClause dbClause, int height, int from, int to)
		{
			return getManyBy(dbClause, height, from, to, defaultSort());
		}

		public DbIterator<T> getManyBy(DbClause dbClause, int height, int from, int to, string sort)
		{
			checkAvailable(height);
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + " AS a WHERE " + dbClause.Clause + "AND a.height <= ?" + (multiversion ? " AND (a.latest = TRUE OR (a.latest = FALSE " + "AND EXISTS (SELECT 1 FROM " + table + " AS b WHERE " + dbKeyFactory.SelfJoinClause + " AND b.height > ?) " + "AND NOT EXISTS (SELECT 1 FROM " + table + " AS b WHERE " + dbKeyFactory.SelfJoinClause + " AND b.height <= ? AND b.height > a.height))) " : " ") + sort + DbUtils.limitsClause(from, to));
				int i = 0;
				i = dbClause.set(pstmt, ++i);
				pstmt.setInt(i, height);
				if(multiversion)
				{
					pstmt.setInt(++i, height);
					pstmt.setInt(++i, height);
				}
				i = DbUtils.setLimits(++i, pstmt, from, to);
				return getManyBy(con, pstmt, false);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public DbIterator<T> getManyBy(Connection con, PreparedStatement pstmt, bool cache)
		{
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean doCache = cache && Db.isInTransaction();
			bool doCache = cache && Db.InTransaction;
			return new DbIterator<>(con, pstmt, new DbIterator.ResultSetReader<T>() { public T get(Connection con, ResultSet rs) throws Exception { T t = null; DbKey dbKey = null; if(doCache) { dbKey = dbKeyFactory.newKey(rs); t = (T) Db.getCache(table)[dbKey]; } if(t == null) { t = load(con, rs); if(doCache) { Db.getCache(table).Add(dbKey, t); } } return t; } });
		}

		public DbIterator<T> getAll(int from, int to)
		{
			return getAll(from, to, defaultSort());
		}

		public DbIterator<T> getAll(int from, int to, string sort)
		{
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + (multiversion ? " WHERE latest = TRUE " : " ") + sort + DbUtils.limitsClause(from, to));
				DbUtils.setLimits(1, pstmt, from, to);
				return getManyBy(con, pstmt, true);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public DbIterator<T> getAll(int height, int from, int to)
		{
			return getAll(height, from, to, defaultSort());
		}

		public DbIterator<T> getAll(int height, int from, int to, string sort)
		{
			checkAvailable(height);
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + " AS a WHERE height <= ?" + (multiversion ? " AND (latest = TRUE OR (latest = FALSE " + "AND EXISTS (SELECT 1 FROM " + table + " AS b WHERE b.height > ? AND " + dbKeyFactory.SelfJoinClause + ") AND NOT EXISTS (SELECT 1 FROM " + table + " AS b WHERE b.height <= ? AND " + dbKeyFactory.SelfJoinClause + " AND b.height > a.height))) " : " ") + sort + DbUtils.limitsClause(from, to));
				int i = 0;
				pstmt.setInt(++i, height);
				if(multiversion)
				{
					pstmt.setInt(++i, height);
					pstmt.setInt(++i, height);
				}
				i = DbUtils.setLimits(++i, pstmt, from, to);
				return getManyBy(con, pstmt, false);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public int Count
		{
			get
			{
				using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT COUNT(*) FROM " + table + (multiversion ? " WHERE latest = TRUE" : "")), ResultSet rs = pstmt.executeQuery())
				{
					rs.next();
					return rs.getInt(1);
				}
				catch(SQLException e)
				{
					throw new Exception(e.ToString(), e);
				}
			}
		}

		public int RowCount
		{
			get
			{
				using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT COUNT(*) FROM " + table), ResultSet rs = pstmt.executeQuery())
				{
					rs.next();
					return rs.getInt(1);
				}
				catch(SQLException e)
				{
					throw new Exception(e.ToString(), e);
				}
			}
		}

		public void insert(T t)
		{
			if(!Db.InTransaction)
			{
				throw new IllegalStateException("Not in transaction");
			}
			DbKey dbKey = dbKeyFactory.newKey(t);
			T cachedT = (T)Db.getCache(table)[dbKey];
			if(cachedT == null)
			{
				Db.getCache(table).Add(dbKey, t);
			} // not a bug
			else if(t != cachedT)
			{
				throw new IllegalStateException("Different instance found in Db cache, perhaps trying to save an object " + "that was read outside the current transaction");
			}
			using (Connection con = Db.Connection)
			{
				if(multiversion)
				{
					using (PreparedStatement pstmt = con.prepareStatement("UPDATE " + table + " SET latest = FALSE " + dbKeyFactory.PKClause + " AND latest = TRUE LIMIT 1"))
					{
						dbKey.PK = pstmt;
						pstmt.executeUpdate();
					}
				}
				save(con, t);
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public override void rollback(int height)
		{
			base.rollback(height);
			Db.getCache(table).Clear();
		}

		public override sealed void truncate()
		{
			base.truncate();
			Db.getCache(table).Clear();
		}

	}

}