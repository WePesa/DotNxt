namespace nxt
{

	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using DbUtils = nxt.db.DbUtils;
	using VersionedEntityDbTable = nxt.db.VersionedEntityDbTable;


	public sealed class Alias
	{

		public class Offer
		{

			private long priceNQT;
			private long buyerId;
			private readonly long aliasId;
			private readonly DbKey dbKey;

			private Offer(long aliasId, long priceNQT, long buyerId)
			{
				this.priceNQT = priceNQT;
				this.buyerId = buyerId;
				this.aliasId = aliasId;
				this.dbKey = offerDbKeyFactory.newKey(this.aliasId);
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Offer(ResultSet rs) throws SQLException
			private Offer(ResultSet rs)
			{
				this.aliasId = rs.getLong("id");
				this.dbKey = offerDbKeyFactory.newKey(this.aliasId);
				this.priceNQT = rs.getLong("price");
				this.buyerId = rs.getLong("buyer_id");
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void save(Connection con) throws SQLException
			private void save(Connection con)
			{
				using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO alias_offer (id, price, buyer_id, " + "height) VALUES (?, ?, ?, ?)"))
				{
					int i = 0;
					pstmt.setLong(++i, this.Id);
					pstmt.setLong(++i, this.PriceNQT);
					DbUtils.setLongZeroToNull(pstmt, ++i, this.BuyerId);
					pstmt.setInt(++i, Nxt.Blockchain.Height);
					pstmt.executeUpdate();
				}
			}

			public virtual long Id
			{
				get
				{
					return aliasId;
				}
			}

			public virtual long PriceNQT
			{
				get
				{
					return priceNQT;
				}
			}

			public virtual long BuyerId
			{
				get
				{
					return buyerId;
				}
			}

		}

		private static final DbKey.LongKeyFactory<Alias> aliasDbKeyFactory = new DbKey.LongKeyFactory<Alias>("id")
		{

			public DbKey newKey(Alias alias)
			{
				return alias.dbKey;
			}

		}

		private static final VersionedEntityDbTable<Alias> aliasTable = new VersionedEntityDbTable<Alias>("alias", aliasDbKeyFactory)
		{

			protected Alias load(Connection con, ResultSet rs) throws SQLException
			{
				return new Alias(rs);
			}

			protected void save(Connection con, Alias alias) throws SQLException
			{
				alias.save(con);
			}

			protected string defaultSort()
			{
				return " ORDER BY alias_name_lower ";
			}

		}

		private static final DbKey.LongKeyFactory<Offer> offerDbKeyFactory = new DbKey.LongKeyFactory<Offer>("id")
		{

			public DbKey newKey(Offer offer)
			{
				return offer.dbKey;
			}

		}

		private static final VersionedEntityDbTable<Offer> offerTable = new VersionedEntityDbTable<Offer>("alias_offer", offerDbKeyFactory)
		{

			protected Offer load(Connection con, ResultSet rs) throws SQLException
			{
				return new Offer(rs);
			}

			protected void save(Connection con, Offer offer) throws SQLException
			{
				offer.save(con);
			}

		}

		public static int Count
		{
			return aliasTable.Count;
		}

		public static DbIterator<Alias> getAliasesByOwner(long accountId, int from, int to)
		{
			return aliasTable.getManyBy(new DbClause.LongClause("account_id", accountId), from, to);
		}

		public static Alias getAlias(string aliasName)
		{
			return aliasTable.getBy(new DbClause.StringClause("alias_name_lower", aliasName.ToLower()));
		}

		public static Alias getAlias(long id)
		{
			return aliasTable.get(aliasDbKeyFactory.newKey(id));
		}

		public static Offer getOffer(Alias alias)
		{
			return offerTable.get(offerDbKeyFactory.newKey(alias.Id));
		}

		static void addOrUpdateAlias(Transaction transaction, Attachment.MessagingAliasAssignment attachment)
		{
			Alias alias = getAlias(attachment.AliasName);
			if(alias == null)
			{
				alias = new Alias(transaction.Id, transaction, attachment);
			}
			else
			{
				alias.accountId = transaction.SenderId;
				alias.aliasURI = attachment.AliasURI;
				alias.timestamp = transaction.BlockTimestamp;
			}
			aliasTable.insert(alias);
		}

		static void sellAlias(Transaction transaction, Attachment.MessagingAliasSell attachment)
		{
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String aliasName = attachment.getAliasName();
			string aliasName = attachment.AliasName;
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long priceNQT = attachment.getPriceNQT();
			long priceNQT = attachment.PriceNQT;
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long buyerId = transaction.getRecipientId();
			long buyerId = transaction.RecipientId;
			if(priceNQT > 0)
			{
				Alias alias = getAlias(aliasName);
				Offer offer = getOffer(alias);
				if(offer == null)
				{
					offerTable.insert(new Offer(alias.id, priceNQT, buyerId));
				}
				else
				{
					offer.priceNQT = priceNQT;
					offer.buyerId = buyerId;
					offerTable.insert(offer);
				}
			}
			else
			{
				changeOwner(buyerId, aliasName, transaction.BlockTimestamp);
			}

		}

		static void changeOwner(long newOwnerId, string aliasName, int timestamp)
		{
			Alias alias = getAlias(aliasName);
			alias.accountId = newOwnerId;
			alias.timestamp = timestamp;
			aliasTable.insert(alias);
			Offer offer = getOffer(alias);
			offerTable.delete(offer);
		}

		static void init()
		{
		}


		private long accountId;
		private final long id;
		private final DbKey dbKey;
		private final string aliasName;
		private string aliasURI;
		private int timestamp;

		private Alias(long id, long accountId, string aliasName, string aliasURI, int timestamp)
		{
			this.id = id;
			this.dbKey = aliasDbKeyFactory.newKey(this.id);
			this.accountId = accountId;
			this.aliasName = aliasName;
			this.aliasURI = aliasURI;
			this.timestamp = timestamp;
		}

		private Alias(long aliasId, Transaction transaction, Attachment.MessagingAliasAssignment attachment)
		{
			this(aliasId, transaction.SenderId, attachment.AliasName, attachment.AliasURI, transaction.BlockTimestamp);
		}

		private Alias(ResultSet rs) throws SQLException
		{
			this.id = rs.getLong("id");
			this.dbKey = aliasDbKeyFactory.newKey(this.id);
			this.accountId = rs.getLong("account_id");
			this.aliasName = rs.getString("alias_name");
			this.aliasURI = rs.getString("alias_uri");
			this.timestamp = rs.getInt("timestamp");
		}

		private void save(Connection con) throws SQLException
		{
			using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO alias (id, account_id, alias_name, " + "alias_uri, timestamp, height) " + "VALUES (?, ?, ?, ?, ?, ?)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.Id);
				pstmt.setLong(++i, this.AccountId);
				pstmt.setString(++i, this.AliasName);
				pstmt.setString(++i, this.AliasURI);
				pstmt.setInt(++i, this.Timestamp);
				pstmt.setInt(++i, Nxt.Blockchain.Height);
				pstmt.executeUpdate();
			}
		}

		public long Id
		{
			return id;
		}

		public string AliasName
		{
			return aliasName;
		}

		public string AliasURI
		{
			return aliasURI;
		}

		public int Timestamp
		{
			return timestamp;
		}

		public long AccountId
		{
			return accountId;
		}

	}

}