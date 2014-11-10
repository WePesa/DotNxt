using System;

namespace nxt.db
{

	using Nxt = nxt.Nxt;


	public abstract class DerivedDbTable
	{

		protected internal readonly string table;

		protected internal DerivedDbTable(string table)
		{
			this.table = table;
			Nxt.BlockchainProcessor.registerDerivedTable(this);
		}

		public virtual void rollback(int height)
		{
			if(!Db.InTransaction)
			{
				throw new InvalidOperationException("Not in transaction");
			}
			using (Connection con = Db.Connection, PreparedStatement pstmtDelete = con.prepareStatement("DELETE FROM " + table + " WHERE height > ?"))
			{
				pstmtDelete.setInt(1, height);
				pstmtDelete.executeUpdate();
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public virtual void truncate()
		{
			if(!Db.InTransaction)
			{
				throw new InvalidOperationException("Not in transaction");
			}
			using (Connection con = Db.Connection, Statement stmt = con.createStatement())
			{
				stmt.executeUpdate("TRUNCATE TABLE " + table);
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public virtual void trim(int height)
		{
		//nothing to trim
		}

	}

}