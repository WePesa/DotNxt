using System.Collections.Generic;

namespace nxt
{

	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using VersionedEntityDbTable = nxt.db.VersionedEntityDbTable;


	public class Hub
	{

		public class Hit : Comparable<Hit>
		{

			public readonly Hub hub;
			public readonly long hitTime;

			private Hit(Hub hub, long hitTime)
			{
				this.hub = hub;
				this.hitTime = hitTime;
			}

			public override int compareTo(Hit hit)
			{
				if(this.hitTime < hit.hitTime)
				{
					return -1;
				}
				else if(this.hitTime > hit.hitTime)
				{
					return 1;
				}
				else
				{
					return long.compare(this.hub.accountId, hit.hub.accountId);
				}
			}

		}

		private static readonly DbKey.LongKeyFactory<Hub> hubDbKeyFactory = null;

		private static readonly VersionedEntityDbTable<Hub> hubTable = null;

		internal static void addOrUpdateHub(Transaction transaction, Attachment.MessagingHubAnnouncement attachment)
		{
			hubTable.insert(new Hub(transaction, attachment));
		}

		private static long lastBlockId;
		private static IList<Hit> lastHits;

		public static IList<Hit> getHubHits(Block block)
		{

			lock (typeof(Hub))
			{
				if(block.Id == lastBlockId && lastHits != null)
				{
					return lastHits;
				}
				IList<Hit> currentHits = new List<>();
				long currentLastBlockId;

				lock (BlockchainImpl.Instance)
				{
					currentLastBlockId = BlockchainImpl.Instance.LastBlock.Id;
					if(currentLastBlockId != block.Id)
					{
						return Collections.emptyList();
					}
					using (DbIterator<Hub> hubs = hubTable.getAll(0, -1))
					{
						while(hubs.hasNext())
						{
							Hub hub = hubs.next();
							Account account = Account.getAccount(hub.AccountId);
							if(account != null && account.EffectiveBalanceNXT >= Constants.MIN_HUB_EFFECTIVE_BALANCE && account.PublicKey != null)
							{
								currentHits.Add(new Hit(hub, Generator.getHitTime(account, block)));
							}
						}
					}
				}

				Collections.sort(currentHits);
				lastHits = currentHits;
				lastBlockId = currentLastBlockId;
			}
			return lastHits;

		}

		internal static void init()
		{
		}


		private readonly long accountId;
		private readonly DbKey dbKey;
		private readonly long minFeePerByteNQT;
		private readonly IList<string> uris;

		private Hub(Transaction transaction, Attachment.MessagingHubAnnouncement attachment)
		{
			this.accountId = transaction.SenderId;
			this.dbKey = hubDbKeyFactory.newKey(this.accountId);
			this.minFeePerByteNQT = attachment.MinFeePerByteNQT;
			this.uris = Collections.unmodifiableList(attachment.Uris);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Hub(ResultSet rs) throws SQLException
		private Hub(ResultSet rs)
		{
			this.accountId = rs.getLong("account_id");
			this.dbKey = hubDbKeyFactory.newKey(this.accountId);
			this.minFeePerByteNQT = rs.getLong("min_fee_per_byte");
			this.uris = Collections.unmodifiableList((string[])rs.getObject("uris"));
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void save(Connection con) throws SQLException
		private void save(Connection con)
		{
			using (PreparedStatement pstmt = con.prepareStatement("MERGE INTO hub (account_id, min_fee_per_byte, " + "uris, height) KEY (account_id, height) VALUES (?, ?, ?, ?)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.AccountId);
				pstmt.setLong(++i, this.MinFeePerByteNQT);
				pstmt.setObject(++i, this.Uris.ToArray());
				pstmt.setInt(++i, Nxt.Blockchain.Height);
				pstmt.executeUpdate();
			}
		}

		public virtual long AccountId
		{
			get
			{
				return accountId;
			}
		}

		public virtual long MinFeePerByteNQT
		{
			get
			{
				return minFeePerByteNQT;
			}
		}

		public virtual IList<string> Uris
		{
			get
			{
				return uris;
			}
		}

	}

}