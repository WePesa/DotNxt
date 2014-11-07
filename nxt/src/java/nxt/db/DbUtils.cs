using System;

namespace nxt.db
{


	public sealed class DbUtils
	{

		public static void close(params AutoCloseable[] closeables)
		{
			foreach (AutoCloseable closeable in closeables)
			{
				if(closeable != null)
				{
					try
					{
						closeable.close();
					}
					catch(Exception ignore)
					{
					}
				}
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setBytes(PreparedStatement pstmt, int index, byte[] bytes) throws SQLException
		public static void setBytes(PreparedStatement pstmt, int index, sbyte[] bytes)
		{
			if(bytes != null)
			{
				pstmt.setBytes(index, bytes);
			}
			else
			{
				pstmt.setNull(index, Types.BINARY);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setString(PreparedStatement pstmt, int index, String s) throws SQLException
		public static void setString(PreparedStatement pstmt, int index, string s)
		{
			if(s != null)
			{
				pstmt.setString(index, s);
			}
			else
			{
				pstmt.setNull(index, Types.VARCHAR);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setIntZeroToNull(PreparedStatement pstmt, int index, int n) throws SQLException
		public static void setIntZeroToNull(PreparedStatement pstmt, int index, int n)
		{
			if(n != 0)
			{
				pstmt.setInt(index, n);
			}
			else
			{
				pstmt.setNull(index, Types.INTEGER);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static void setLongZeroToNull(PreparedStatement pstmt, int index, long l) throws SQLException
		public static void setLongZeroToNull(PreparedStatement pstmt, int index, long l)
		{
			if(l != 0)
			{
				pstmt.setLong(index, l);
			}
			else
			{
				pstmt.setNull(index, Types.BIGINT);
			}
		}

		public static string limitsClause(int from, int to)
		{
			int limit = to >=0 && to >= from && to < int.MaxValue ? to - from + 1 : 0;
			if(limit > 0 && from > 0)
			{
				return " LIMIT ? OFFSET ? ";
			}
			else if(limit > 0)
			{
				return " LIMIT ? ";
			}
			else if(from > 0)
			{
				return " OFFSET ? ";
			}
			else
			{
				return "";
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static int setLimits(int index, PreparedStatement pstmt, int from, int to) throws SQLException
		public static int setLimits(int index, PreparedStatement pstmt, int from, int to)
		{
			int limit = to >=0 && to >= from && to < int.MaxValue ? to - from + 1 : 0;
			if(limit > 0)
			{
				pstmt.setInt(index++, limit);
			}
			if(from > 0)
			{
				pstmt.setInt(index++, from);
			}
			return index;
		}

		private DbUtils() // never
		{
		}

	}

}