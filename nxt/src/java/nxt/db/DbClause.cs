namespace nxt.db
{


	public abstract class DbClause
	{

		private readonly string clause;

		protected internal DbClause(string clause)
		{
			this.clause = clause;
		}

		internal string Clause
		{
			get
			{
				return clause;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected abstract int set(PreparedStatement pstmt, int index) throws SQLException;
		protected internal abstract int set(PreparedStatement pstmt, int index);


		public sealed class StringClause : DbClause
		{

			private readonly string value;

			public StringClause(string columnName, string value) : base(" " + columnName + " = ? ")
			{
				this.value = value;
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected int set(PreparedStatement pstmt, int index) throws SQLException
			protected internal override int set(PreparedStatement pstmt, int index)
			{
				pstmt.setString(index, value);
				return index + 1;
			}

		}

		public sealed class LongClause : DbClause
		{

			private readonly long value;

			public LongClause(string columnName, long value) : base(" " + columnName + " = ? ")
			{
				this.value = value;
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected int set(PreparedStatement pstmt, int index) throws SQLException
			protected internal override int set(PreparedStatement pstmt, int index)
			{
				pstmt.setLong(index, value);
				return index + 1;
			}

		}

	}

}