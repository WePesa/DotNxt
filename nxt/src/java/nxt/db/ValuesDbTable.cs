using System;
using System.Collections.Generic;

namespace nxt.db
{


	public abstract class ValuesDbTable<T, V> : DerivedDbTable
	{

		private readonly bool multiversion;
		protected internal readonly DbKey.Factory<T> dbKeyFactory;

		protected internal ValuesDbTable(string table, DbKey.Factory<T> dbKeyFactory) : this(table, dbKeyFactory, false)
		{
		}

		internal ValuesDbTable(string table, DbKey.Factory<T> dbKeyFactory, bool multiversion) : base(table)
		{
			this.dbKeyFactory = dbKeyFactory;
			this.multiversion = multiversion;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract V load(Connection con, ResultSet rs) throws SQLException;
		protected internal abstract V load(Connection con, ResultSet rs);

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract void save(Connection con, T t, V v) throws SQLException;
		protected internal abstract void save(Connection con, T t, V v);

		public IList<V> get(DbKey dbKey)
		{
			IList<V> values;
			if(Db.InTransaction)
			{
				values = (IList<V>)Db.getCache(table)[dbKey];
				if(values != null)
				{
					return values;
				}
			}
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM " + table + dbKeyFactory.PKClause + (multiversion ? " AND latest = TRUE" : "") + " ORDER BY db_id DESC"))
			{
				dbKey.PK = pstmt;
				values = get(con, pstmt);
				if(Db.InTransaction)
				{
					Db.getCache(table).Add(dbKey, values);
				}
				return values;
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		private IList<V> get(Connection con, PreparedStatement pstmt)
		{
			try
			{
				IList<V> result = new List<>();
				using (ResultSet rs = pstmt.executeQuery())
				{
					while(rs.next())
					{
						result.Add(load(con, rs));
					}
				}
				return result;
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public void insert(T t, IList<V> values)
		{
			if(!Db.InTransaction)
			{
				throw new IllegalStateException("Not in transaction");
			}
			DbKey dbKey = dbKeyFactory.newKey(t);
			Db.getCache(table).Add(dbKey, values);
			using (Connection con = Db.Connection)
			{
				if(multiversion)
				{
					using (PreparedStatement pstmt = con.prepareStatement("UPDATE " + table + " SET latest = FALSE " + dbKeyFactory.PKClause + " AND latest = TRUE"))
					{
						dbKey.PK = pstmt;
						pstmt.executeUpdate();
					}
				}
				foreach (V v in values)
				{
					save(con, t, v);
				}
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