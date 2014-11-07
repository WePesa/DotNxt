using System.Collections.Generic;

namespace nxt
{

	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using EntityDbTable = nxt.db.EntityDbTable;


	public sealed class Poll
	{

		private static readonly DbKey.LongKeyFactory<Poll> pollDbKeyFactory = null;

		private static readonly EntityDbTable<Poll> pollTable = null;

		internal static void init()
		{
		}


		private readonly long id;
		private readonly DbKey dbKey;
		private readonly string name;
		private readonly string description;
		private readonly string[] options;
		private readonly sbyte minNumberOfOptions, maxNumberOfOptions;
		private readonly bool optionsAreBinary;

		private Poll(long id, Attachment.MessagingPollCreation attachment)
		{
			this.id = id;
			this.dbKey = pollDbKeyFactory.newKey(this.id);
			this.name = attachment.PollName;
			this.description = attachment.PollDescription;
			this.options = attachment.PollOptions;
			this.minNumberOfOptions = attachment.MinNumberOfOptions;
			this.maxNumberOfOptions = attachment.MaxNumberOfOptions;
			this.optionsAreBinary = attachment.OptionsAreBinary;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Poll(ResultSet rs) throws SQLException
		private Poll(ResultSet rs)
		{
			this.id = rs.getLong("id");
			this.dbKey = pollDbKeyFactory.newKey(this.id);
			this.name = rs.getString("name");
			this.description = rs.getString("description");
			this.options = (string[])rs.getArray("options").Array;
			this.minNumberOfOptions = rs.getByte("min_num_options");
			this.maxNumberOfOptions = rs.getByte("max_num_options");
			this.optionsAreBinary = rs.getBoolean("binary_options");
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void save(Connection con) throws SQLException
		private void save(Connection con)
		{
			using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO poll (id, name, description, " + "options, min_num_options, max_num_options, binary_options, height) VALUES (?, ?, ?, ?, ?, ?, ?, ?)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.Id);
				pstmt.setString(++i, this.Name);
				pstmt.setString(++i, this.Description);
				pstmt.setObject(++i, this.Options);
				pstmt.setByte(++i, this.MinNumberOfOptions);
				pstmt.setByte(++i, this.MaxNumberOfOptions);
				pstmt.setBoolean(++i, this.OptionsAreBinary);
				pstmt.setInt(++i, Nxt.Blockchain.Height);
				pstmt.executeUpdate();
			}
		}

		internal static void addPoll(Transaction transaction, Attachment.MessagingPollCreation attachment)
		{
			pollTable.insert(new Poll(transaction.Id, attachment));
		}

		public static Poll getPoll(long id)
		{
			return pollTable.get(pollDbKeyFactory.newKey(id));
		}

		public static DbIterator<Poll> getAllPolls(int from, int to)
		{
			return pollTable.getAll(from, to);
		}

		public static int Count
		{
			get
			{
				return pollTable.Count;
			}
		}


		public long Id
		{
			get
			{
				return id;
			}
		}

		public string Name
		{
			get
			{
				return name;
			}
		}

		public string Description
		{
			get
			{
				return description;
			}
		}

		public string[] Options
		{
			get
			{
				return options;
			}
		}

		public sbyte MinNumberOfOptions
		{
			get
			{
				return minNumberOfOptions;
			}
		}

		public sbyte MaxNumberOfOptions
		{
			get
			{
				return maxNumberOfOptions;
			}
		}

		public bool isOptionsAreBinary()
		{
			get
			{
				return optionsAreBinary;
			}
		}

		public IDictionary<long?, long?> Voters
		{
			get
			{
				return Vote.getVoters(this);
			}
		}

	}

}