using System;
using System.Collections.Generic;

namespace nxt.db
{

	using Nxt = nxt.Nxt;


	public abstract class VersionedEntityDbTable<T> : EntityDbTable<T>
	{

		protected internal VersionedEntityDbTable(string table, DbKey.Factory<T> dbKeyFactory) : base(table, dbKeyFactory, true)
		{
		}

		public override void rollback(int height)
		{
			rollback(table, height, dbKeyFactory);
		}

		public bool delete(T t)
		{
			if(t == null)
			{
				return false;
			}
			if(!Db.InTransaction)
			{
				throw new InvalidOperationException("Not in transaction");
			}
			DbKey dbKey = dbKeyFactory.newKey(t);
			using (Connection con = Db.Connection, PreparedStatement pstmtCount = con.prepareStatement("SELECT COUNT(*) AS count FROM " + table + dbKeyFactory.PKClause + " AND height < ?"))
			{
				int i = dbKey.PK = pstmtCount;
				pstmtCount.setInt(i, Nxt.Blockchain.Height);
				using (ResultSet rs = pstmtCount.executeQuery())
				{
					rs.next();
					if(rs.getInt("count") > 0)
					{
						using (PreparedStatement pstmt = con.prepareStatement("UPDATE " + table + " SET latest = FALSE " + dbKeyFactory.PKClause + " AND latest = TRUE LIMIT 1"))
						{
							dbKey.PK = pstmt;
							pstmt.executeUpdate();
							save(con, t);
							pstmt.executeUpdate(); // delete after the save
						}
						return true;
					}
					else
					{
						using (PreparedStatement pstmtDelete = con.prepareStatement("DELETE FROM " + table + dbKeyFactory.PKClause))
						{
							dbKey.PK = pstmtDelete;
							return pstmtDelete.executeUpdate() > 0;
						}
					}
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			finally
			{
				Db.getCache(table).Remove(dbKey);
			}
		}

		public override void trim(int height)
		{
			Trim(table, height, dbKeyFactory);
		}

//JAVA TO VB & C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: static void rollback(final String table, final int height, final DbKey.Factory dbKeyFactory)
		internal static void rollback(string table, int height, DbKey.Factory dbKeyFactory)
		{
			if(!Db.InTransaction)
			{
				throw new InvalidOperationException("Not in transaction");
			}
			using (Connection con = Db.Connection, PreparedStatement pstmtSelectToDelete = con.prepareStatement("SELECT DISTINCT " + dbKeyFactory.PKColumns + " FROM " + table + " WHERE height > ?"), PreparedStatement pstmtDelete = con.prepareStatement("DELETE FROM " + table + " WHERE height > ?"), PreparedStatement pstmtSetLatest = con.prepareStatement("UPDATE " + table + " SET latest = TRUE " + dbKeyFactory.PKClause + " AND height =" + " (SELECT MAX(height) FROM " + table + dbKeyFactory.PKClause + ")"))
			{
				pstmtSelectToDelete.setInt(1, height);
				IList<DbKey> dbKeys = new List<>();
				using (ResultSet rs = pstmtSelectToDelete.executeQuery())
				{
					while(rs.next())
					{
						dbKeys.Add(dbKeyFactory.newKey(rs));
					}
				}
				pstmtDelete.setInt(1, height);
				pstmtDelete.executeUpdate();
				foreach (DbKey dbKey in dbKeys)
				{
					int i = 1;
					i = dbKey.setPK(pstmtSetLatest, i);
					i = dbKey.setPK(pstmtSetLatest, i);
					pstmtSetLatest.executeUpdate();
				//Db.getCache(table).remove(dbKey);
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			Db.getCache(table).Clear();
		}

//JAVA TO VB & C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: static void trim(final String table, final int height, final DbKey.Factory dbKeyFactory)
		internal static void trim(string table, int height, DbKey.Factory dbKeyFactory)
		{
			if(!Db.InTransaction)
			{
				throw new InvalidOperationException("Not in transaction");
			}
			using (Connection con = Db.Connection, PreparedStatement pstmtSelect = con.prepareStatement("SELECT " + dbKeyFactory.PKColumns + ", MAX(height) AS max_height" + " FROM " + table + " WHERE height < ? GROUP BY " + dbKeyFactory.PKColumns + " HAVING COUNT(DISTINCT height) > 1"), PreparedStatement pstmtDelete = con.prepareStatement("DELETE FROM " + table + dbKeyFactory.PKClause + " AND height < ?"), PreparedStatement pstmtDeleteDeleted = con.prepareStatement("DELETE FROM " + table + " WHERE height < ? AND latest = FALSE " + " AND (" + dbKeyFactory.PKColumns + ") NOT IN (SELECT (" + dbKeyFactory.PKColumns + ") FROM " + table + " WHERE height >= ?)"))
			{
				pstmtSelect.setInt(1, height);
				using (ResultSet rs = pstmtSelect.executeQuery())
				{
					while(rs.next())
					{
						DbKey dbKey = dbKeyFactory.newKey(rs);
						int maxHeight = rs.getInt("max_height");
						int i = 1;
						i = dbKey.setPK(pstmtDelete, i);
						pstmtDelete.setInt(i, maxHeight);
						pstmtDelete.executeUpdate();
					}
					pstmtDeleteDeleted.setInt(1, height);
					pstmtDeleteDeleted.setInt(2, height);
					pstmtDeleteDeleted.executeUpdate();
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

	}

}