using System;
using System.Collections.Generic;

namespace nxt.db
{


	public class FilteredConnection : Connection
	{

		private readonly Connection con;

		public FilteredConnection(Connection con)
		{
			this.con = con;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Statement createStatement() throws SQLException
		public override Statement createStatement()
		{
			return con.createStatement();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PreparedStatement prepareStatement(String sql) throws SQLException
		public override PreparedStatement prepareStatement(string sql)
		{
			return con.prepareStatement(sql);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CallableStatement prepareCall(String sql) throws SQLException
		public override CallableStatement prepareCall(string sql)
		{
			return con.prepareCall(sql);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String nativeSQL(String sql) throws SQLException
		public override string nativeSQL(string sql)
		{
			return con.nativeSQL(sql);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setAutoCommit(boolean autoCommit) throws SQLException
		public override bool AutoCommit
		{
			set
			{
				con.AutoCommit = value;
			}
			get
			{
				return con.AutoCommit;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean getAutoCommit() throws SQLException

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void commit() throws SQLException
		public override void commit()
		{
			con.commit();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void rollback() throws SQLException
		public override void rollback()
		{
			con.rollback();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws SQLException
		public override void close()
		{
			con.close();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isClosed() throws SQLException
		public override bool isClosed()
		{
			get
			{
				return con.Closed;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public DatabaseMetaData getMetaData() throws SQLException
		public override DatabaseMetaData MetaData
		{
			get
			{
				return con.MetaData;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setReadOnly(boolean readOnly) throws SQLException
		public override bool ReadOnly
		{
			set
			{
				con.ReadOnly = value;
			}
			get
			{
				return con.ReadOnly;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isReadOnly() throws SQLException

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setCatalog(String catalog) throws SQLException
		public override string Catalog
		{
			set
			{
				con.Catalog = value;
			}
			get
			{
				return con.Catalog;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getCatalog() throws SQLException

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTransactionIsolation(int level) throws SQLException
		public override int TransactionIsolation
		{
			set
			{
				con.TransactionIsolation = value;
			}
			get
			{
				return con.TransactionIsolation;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getTransactionIsolation() throws SQLException

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SQLWarning getWarnings() throws SQLException
		public override SQLWarning Warnings
		{
			get
			{
				return con.Warnings;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void clearWarnings() throws SQLException
		public override void clearWarnings()
		{
			con.clearWarnings();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Statement createStatement(int resultSetType, int resultSetConcurrency) throws SQLException
		public override Statement createStatement(int resultSetType, int resultSetConcurrency)
		{
			return con.createStatement(resultSetType, resultSetConcurrency);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PreparedStatement prepareStatement(String sql, int resultSetType, int resultSetConcurrency) throws SQLException
		public override PreparedStatement prepareStatement(string sql, int resultSetType, int resultSetConcurrency)
		{
			return con.prepareStatement(sql, resultSetType, resultSetConcurrency);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CallableStatement prepareCall(String sql, int resultSetType, int resultSetConcurrency) throws SQLException
		public override CallableStatement prepareCall(string sql, int resultSetType, int resultSetConcurrency)
		{
			return con.prepareCall(sql, resultSetType, resultSetConcurrency);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Map<String, Class<?>> getTypeMap() throws SQLException
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
		public override IDictionary<string, Type<?>> TypeMap
		{
			get
			{
				return con.TypeMap;
			}
			set
			{
				con.TypeMap = value;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setTypeMap(Map<String, Class<?>> map) throws SQLException

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setHoldability(int holdability) throws SQLException
		public override int Holdability
		{
			set
			{
				con.Holdability = value;
			}
			get
			{
				return con.Holdability;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getHoldability() throws SQLException

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Savepoint setSavepoint() throws SQLException
		public override Savepoint setSavepoint()
		{
			return con.setSavepoint();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Savepoint setSavepoint(String name) throws SQLException
		public override Savepoint Savepoint
		{
			set
			{
				return con.Savepoint = value;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void rollback(Savepoint savepoint) throws SQLException
		public override void rollback(Savepoint savepoint)
		{
			con.rollback(savepoint);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void releaseSavepoint(Savepoint savepoint) throws SQLException
		public override void releaseSavepoint(Savepoint savepoint)
		{
			con.releaseSavepoint(savepoint);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Statement createStatement(int resultSetType, int resultSetConcurrency, int resultSetHoldability) throws SQLException
		public override Statement createStatement(int resultSetType, int resultSetConcurrency, int resultSetHoldability)
		{
			return con.createStatement(resultSetType, resultSetConcurrency, resultSetHoldability);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PreparedStatement prepareStatement(String sql, int resultSetType, int resultSetConcurrency, int resultSetHoldability) throws SQLException
		public override PreparedStatement prepareStatement(string sql, int resultSetType, int resultSetConcurrency, int resultSetHoldability)
		{
			return con.prepareStatement(sql, resultSetType, resultSetConcurrency, resultSetHoldability);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CallableStatement prepareCall(String sql, int resultSetType, int resultSetConcurrency, int resultSetHoldability) throws SQLException
		public override CallableStatement prepareCall(string sql, int resultSetType, int resultSetConcurrency, int resultSetHoldability)
		{
			return con.prepareCall(sql, resultSetType, resultSetConcurrency, resultSetHoldability);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PreparedStatement prepareStatement(String sql, int autoGeneratedKeys) throws SQLException
		public override PreparedStatement prepareStatement(string sql, int autoGeneratedKeys)
		{
			return con.prepareStatement(sql, autoGeneratedKeys);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PreparedStatement prepareStatement(String sql, int[] columnIndexes) throws SQLException
		public override PreparedStatement prepareStatement(string sql, int[] columnIndexes)
		{
			return con.prepareStatement(sql, columnIndexes);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public PreparedStatement prepareStatement(String sql, String[] columnNames) throws SQLException
		public override PreparedStatement prepareStatement(string sql, string[] columnNames)
		{
			return con.prepareStatement(sql, columnNames);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Clob createClob() throws SQLException
		public override Clob createClob()
		{
			return con.createClob();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Blob createBlob() throws SQLException
		public override Blob createBlob()
		{
			return con.createBlob();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public NClob createNClob() throws SQLException
		public override NClob createNClob()
		{
			return con.createNClob();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public SQLXML createSQLXML() throws SQLException
		public override SQLXML createSQLXML()
		{
			return con.createSQLXML();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isValid(int timeout) throws SQLException
		public override bool isValid(int timeout)
		{
			return con.isValid(timeout);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setClientInfo(String name, String value) throws SQLClientInfoException
		public override void setClientInfo(string name, string value)
		{
			con.setClientInfo(name, value);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setClientInfo(Properties properties) throws SQLClientInfoException
		public override Properties ClientInfo
		{
			set
			{
				con.ClientInfo = value;
			}
			get
			{
				return con.ClientInfo;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getClientInfo(String name) throws SQLException
		public override string getClientInfo(string name)
		{
			return con.getClientInfo(name);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Properties getClientInfo() throws SQLException

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Array createArrayOf(String typeName, Object[] elements) throws SQLException
		public override Array createArrayOf(string typeName, object[] elements)
		{
			return con.createArrayOf(typeName, elements);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Struct createStruct(String typeName, Object[] attributes) throws SQLException
		public override Struct createStruct(string typeName, object[] attributes)
		{
			return con.createStruct(typeName, attributes);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setSchema(String schema) throws SQLException
		public override string Schema
		{
			set
			{
				con.Schema = value;
			}
			get
			{
				return con.Schema;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public String getSchema() throws SQLException

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void abort(Executor executor) throws SQLException
		public override void abort(Executor executor)
		{
			con.abort(executor);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void setNetworkTimeout(Executor executor, int milliseconds) throws SQLException
		public override void setNetworkTimeout(Executor executor, int milliseconds)
		{
			con.setNetworkTimeout(executor, milliseconds);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int getNetworkTimeout() throws SQLException
		public override int NetworkTimeout
		{
			get
			{
				return con.NetworkTimeout;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public <T> T unwrap(Class<T> iface) throws SQLException
		public override T unwrap<T>(Type<T> iface)
		{
			return con.unwrap(iface);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public boolean isWrapperFor(Class<?> iface) throws SQLException
		public override bool isWrapperFor<T1>(Type<T1> iface)
		{
			return con.isWrapperFor(iface);
		}
	}

}