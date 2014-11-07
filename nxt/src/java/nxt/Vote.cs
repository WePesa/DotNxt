using System.Collections.Generic;

namespace nxt
{

	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using EntityDbTable = nxt.db.EntityDbTable;


	public sealed class Vote
	{

		private static readonly DbKey.LongKeyFactory<Vote> voteDbKeyFactory = null;

		private static readonly EntityDbTable<Vote> voteTable = null;

		static Vote addVote(Transaction transaction, Attachment.MessagingVoteCasting attachment)
		{
			Vote vote = new Vote(transaction, attachment);
			voteTable.insert(vote);
			return vote;
		}

		public static int Count
		{
			get
			{
				return voteTable.Count;
			}
		}

		public static Vote getVote(long id)
		{
			return voteTable.get(voteDbKeyFactory.newKey(id));
		}

		public static IDictionary<long?, long?> getVoters(Poll poll)
		{
			IDictionary<long?, long?> map = new Dictionary<>();
			using (DbIterator<Vote> voteIterator = voteTable.getManyBy(new DbClause.LongClause("poll_id", poll.Id), 0, -1))
			{
				while(voteIterator.hasNext())
				{
					Vote vote = voteIterator.next();
					map.Add(vote.VoterId, vote.Id);
				}
			}
			return map;
		}

		internal static void init()
		{
		}


		private readonly long id;
		private readonly DbKey dbKey;
		private readonly long pollId;
		private readonly long voterId;
		private readonly sbyte[] voteBytes;

		private Vote(Transaction transaction, Attachment.MessagingVoteCasting attachment)
		{
			this.id = transaction.Id;
			this.dbKey = voteDbKeyFactory.newKey(this.id);
			this.pollId = attachment.PollId;
			this.voterId = transaction.SenderId;
			this.voteBytes = attachment.PollVote;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Vote(ResultSet rs) throws SQLException
		private Vote(ResultSet rs)
		{
			this.id = rs.getLong("id");
			this.dbKey = voteDbKeyFactory.newKey(this.id);
			this.pollId = rs.getLong("poll_id");
			this.voterId = rs.getLong("voter_id");
			this.voteBytes = rs.getBytes("vote_bytes");
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void save(Connection con) throws SQLException
		private void save(Connection con)
		{
			using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO vote (id, poll_id, voter_id, " + "vote_bytes, height) VALUES (?, ?, ?, ?, ?)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.Id);
				pstmt.setLong(++i, this.PollId);
				pstmt.setLong(++i, this.VoterId);
				pstmt.setBytes(++i, this.Vote);
				pstmt.setInt(++i, Nxt.Blockchain.Height);
				pstmt.executeUpdate();
			}
		}

		public long Id
		{
			get
			{
				return id;
			}
		}

		public long PollId
		{
			get
			{
				return pollId;
			}
		}

		public long VoterId
		{
			get
			{
				return voterId;
			}
		}

		public sbyte[] Vote
		{
			get
			{
				return voteBytes;
			}
		}

	}

}