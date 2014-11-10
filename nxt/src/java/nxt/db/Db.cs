using System;
using System.Collections.Generic;
using System.Threading;

namespace nxt.db
{

	using Constants = nxt.Constants;
	using Nxt = nxt.Nxt;
	using Logger = nxt.util.Logger;
	using JdbcConnectionPool = org.h2.jdbcx.JdbcConnectionPool;


	public sealed class Db
	{

		private static readonly JdbcConnectionPool cp;
		private static volatile int maxActiveConnections;

		private static readonly ThreadLocal<DbConnection> localConnection = new ThreadLocal<>();
		private static readonly ThreadLocal<IDictionary<string, IDictionary<DbKey, object>>> transactionCaches = new ThreadLocal<>();

		private sealed class DbConnection : FilteredConnection
		{

			private DbConnection(Connection con) : base(con)
			{
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setAutoCommit(boolean autoCommit) throws SQLException
			public override bool AutoCommit
			{
				set
				{
					throw new UnsupportedOperationException("Use Db.beginTransaction() to start a new transaction");
				}
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void commit() throws SQLException
			public override void commit()
			{
				if(localConnection.get() == null)
				{
					base.commit();
				}
				else if(! this.Equals(localConnection.get()))
				{
					throw new IllegalStateException("Previous connection not committed");
				}
				else
				{
					throw new UnsupportedOperationException("Use Db.commitTransaction() to commit the transaction");
				}
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void doCommit() throws SQLException
			private void doCommit()
			{
				base.commit();
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void rollback() throws SQLException
			public override void rollback()
			{
				if(localConnection.get() == null)
				{
					base.rollback();
				}
				else if(! this.Equals(localConnection.get()))
				{
					throw new IllegalStateException("Previous connection not committed");
				}
				else
				{
					throw new UnsupportedOperationException("Use Db.rollbackTransaction() to rollback the transaction");
				}
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void doRollback() throws SQLException
			private void doRollback()
			{
				base.rollback();
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws SQLException
			public override void close()
			{
				if(localConnection.get() == null)
				{
					base.close();
				}
				else if(! this.Equals(localConnection.get()))
				{
					throw new IllegalStateException("Previous connection not committed");
				}
			}

		}

		public static void init()
		{
		}

		static Db()
		{
			long maxCacheSize = Nxt.getIntProperty("nxt.dbCacheKB");
			if(maxCacheSize == 0)
			{
				maxCacheSize = Math.Min(256, Math.Max(16, (Runtime.Runtime.maxMemory() / (1024 * 1024) - 128)/2)) * 1024;
			}
			string dbUrl = Constants.isTestnet ? Nxt.getStringProperty("nxt.testDbUrl") : Nxt.getStringProperty("nxt.dbUrl");
			if(! dbUrl.Contains("CACHE_SIZE="))
			{
				dbUrl += ";CACHE_SIZE=" + maxCacheSize;
			}
			Logger.logDebugMessage("Database jdbc url set to: " + dbUrl);
			cp = JdbcConnectionPool.create(dbUrl, "sa", "sa");
			cp.MaxConnections = Nxt.getIntProperty("nxt.maxDbConnections");
			cp.LoginTimeout = Nxt.getIntProperty("nxt.dbLoginTimeout");
			int defaultLockTimeout = Nxt.getIntProperty("nxt.dbDefaultLockTimeout") * 1000;
			using (Connection con = cp.Connection, Statement stmt = con.createStatement())
			{
				stmt.executeUpdate("SET DEFAULT_LOCK_TIMEOUT " + defaultLockTimeout);
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public static void analyzeTables()
		{
			using (Connection con = cp.Connection, Statement stmt = con.createStatement())
			{
				stmt.execute("ANALYZE SAMPLE_SIZE 0");
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public static void shutdown()
		{
			try
			{
				Connection con = cp.Connection;
				Statement stmt = con.createStatement();
				stmt.execute("SHUTDOWN COMPACT");
				Logger.logShutdownMessage("Database shutdown completed");
			}
			catch(SQLException e)
			{
				Logger.logShutdownMessage(e.ToString(), e);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static Connection getPooledConnection() throws SQLException
		private static Connection PooledConnection
		{
			get
			{
				Connection con = cp.Connection;
				int activeConnections = cp.ActiveConnections;
				if(activeConnections > maxActiveConnections)
				{
					maxActiveConnections = activeConnections;
					Logger.logDebugMessage("Database connection pool current size: " + activeConnections);
				}
				return con;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static Connection getConnection() throws SQLException
		public static Connection Connection
		{
			get
			{
				Connection con = localConnection.get();
				if(con != null)
				{
					return con;
				}
				con = PooledConnection;
				con.AutoCommit = true;
				return new DbConnection(con);
			}
		}

		internal static IDictionary<DbKey, object> getCache(string tableName)
		{
			if(!InTransaction)
			{
				throw new IllegalStateException("Not in transaction");
			}
			IDictionary<DbKey, object> cacheMap = transactionCaches.get().get(tableName);
			if(cacheMap == null)
			{
				cacheMap = new Dictionary<>();
				transactionCaches.get().put(tableName, cacheMap);
			}
			return cacheMap;
		}

		public static bool isInTransaction()
		{
			get
			{
				return localConnection.get() != null;
			}
		}

		public static Connection beginTransaction()
		{
			if(localConnection.get() != null)
			{
				throw new IllegalStateException("Transaction already in progress");
			}
			try
			{
				Connection con = PooledConnection;
				con.AutoCommit = false;
				con = new DbConnection(con);
				localConnection.set((DbConnection)con);
				transactionCaches.set(new Dictionary<string, IDictionary<DbKey, object>>());
				return con;
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public static void commitTransaction()
		{
			DbConnection con = localConnection.get();
			if(con == null)
			{
				throw new IllegalStateException("Not in transaction");
			}
			try
			{
				con.doCommit();
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public static void rollbackTransaction()
		{
			DbConnection con = localConnection.get();
			if(con == null)
			{
				throw new IllegalStateException("Not in transaction");
			}
			try
			{
				con.doRollback();
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			transactionCaches.get().clear();
		}

		public static void endTransaction()
		{
			Connection con = localConnection.get();
			if(con == null)
			{
				throw new IllegalStateException("Not in transaction");
			}
			localConnection.set(null);
			transactionCaches.get().clear();
			transactionCaches.set(null);
			DbUtils.close(con);
		}

		private Db() // never
		{
		}

	}

}