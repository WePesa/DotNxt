namespace nxt
{

	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using EntityDbTable = nxt.db.EntityDbTable;


	public sealed class Asset
	{

		private static final DbKey.LongKeyFactory<Asset> assetDbKeyFactory = new DbKey.LongKeyFactory<Asset>("id")
		{

			public DbKey newKey(Asset asset)
			{
				return asset.dbKey;
			}

		}

		private static final EntityDbTable<Asset> assetTable = new EntityDbTable<Asset>("asset", assetDbKeyFactory)
		{

			protected Asset load(Connection con, ResultSet rs) throws SQLException
			{
				return new Asset(rs);
			}

			protected void save(Connection con, Asset asset) throws SQLException
			{
				asset.save(con);
			}

		}

		public static DbIterator<Asset> getAllAssets(int from, int to)
		{
			return assetTable.getAll(from, to);
		}

		public static int Count
		{
			return assetTable.Count;
		}

		public static Asset getAsset(long id)
		{
			return assetTable.get(assetDbKeyFactory.newKey(id));
		}

		public static DbIterator<Asset> getAssetsIssuedBy(long accountId, int from, int to)
		{
			return assetTable.getManyBy(new DbClause.LongClause("account_id", accountId), from, to);
		}

		static void addAsset(Transaction transaction, Attachment.ColoredCoinsAssetIssuance attachment)
		{
			assetTable.insert(new Asset(transaction, attachment));
		}

		static void init()
		{
		}


		private final long assetId;
		private final DbKey dbKey;
		private final long accountId;
		private final string name;
		private final string description;
		private final long quantityQNT;
		private final sbyte decimals;

		private Asset(Transaction transaction, Attachment.ColoredCoinsAssetIssuance attachment)
		{
			this.assetId = transaction.Id;
			this.dbKey = assetDbKeyFactory.newKey(this.assetId);
			this.accountId = transaction.SenderId;
			this.name = attachment.Name;
			this.description = attachment.Description;
			this.quantityQNT = attachment.QuantityQNT;
			this.decimals = attachment.Decimals;
		}

		private Asset(ResultSet rs) throws SQLException
		{
			this.assetId = rs.getLong("id");
			this.dbKey = assetDbKeyFactory.newKey(this.assetId);
			this.accountId = rs.getLong("account_id");
			this.name = rs.getString("name");
			this.description = rs.getString("description");
			this.quantityQNT = rs.getLong("quantity");
			this.decimals = rs.getByte("decimals");
		}

		private void save(Connection con) throws SQLException
		{
			using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO asset (id, account_id, name, " + "description, quantity, decimals, height) VALUES (?, ?, ?, ?, ?, ?, ?)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.Id);
				pstmt.setLong(++i, this.AccountId);
				pstmt.setString(++i, this.Name);
				pstmt.setString(++i, this.Description);
				pstmt.setLong(++i, this.QuantityQNT);
				pstmt.setByte(++i, this.Decimals);
				pstmt.setInt(++i, Nxt.Blockchain.Height);
				pstmt.executeUpdate();
			}
		}

		public long Id
		{
			return assetId;
		}

		public long AccountId
		{
			return accountId;
		}

		public string Name
		{
			return name;
		}

		public string Description
		{
			return description;
		}

		public long QuantityQNT
		{
			return quantityQNT;
		}

		public sbyte Decimals
		{
			return decimals;
		}

		public DbIterator<Account.AccountAsset> getAccounts(int from, int to)
		{
			return Account.getAssetAccounts(this.assetId, from, to);
		}

		public DbIterator<Account.AccountAsset> getAccounts(int height, int from, int to)
		{
			if(height < 0)
			{
				return getAccounts(from, to);
			}
			return Account.getAssetAccounts(this.assetId, height, from, to);
		}

		public DbIterator<Trade> getTrades(int from, int to)
		{
			return Trade.getAssetTrades(this.assetId, from, to);
		}

		public DbIterator<AssetTransfer> getAssetTransfers(int from, int to)
		{
			return AssetTransfer.getAssetTransfers(this.assetId, from, to);
		}
	}

}