using System;

namespace nxt
{

	using Db = nxt.db.Db;
	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using DbUtils = nxt.db.DbUtils;
	using EntityDbTable = nxt.db.EntityDbTable;
	using Listener = nxt.util.Listener;
	using Listeners = nxt.util.Listeners;


	public sealed class AssetTransfer
	{

		public enum Event
		{
			ASSET_TRANSFER
		}

		private static readonly Listeners<AssetTransfer, Event> listeners = new Listeners<>();

		private static final DbKey.LongKeyFactory<AssetTransfer> transferDbKeyFactory = new DbKey.LongKeyFactory<AssetTransfer>("id")
		{

			public DbKey newKey(AssetTransfer assetTransfer)
			{
				return assetTransfer.dbKey;
			}

		}

		private static final EntityDbTable<AssetTransfer> assetTransferTable = new EntityDbTable<AssetTransfer>("asset_transfer", transferDbKeyFactory)
		{

			protected AssetTransfer load(Connection con, ResultSet rs) throws SQLException
			{
				return new AssetTransfer(rs);
			}

			protected void save(Connection con, AssetTransfer assetTransfer) throws SQLException
			{
				assetTransfer.save(con);
			}

		}

		public static DbIterator<AssetTransfer> getAllTransfers(int from, int to)
		{
			return assetTransferTable.getAll(from, to);
		}

		public static int Count
		{
			return assetTransferTable.Count;
		}

		public static bool addListener(Listener<AssetTransfer> listener, Event eventType)
		{
			return listeners.addListener(listener, eventType);
		}

		public static bool removeListener(Listener<AssetTransfer> listener, Event eventType)
		{
			return listeners.removeListener(listener, eventType);
		}

		public static DbIterator<AssetTransfer> getAssetTransfers(long assetId, int from, int to)
		{
			return assetTransferTable.getManyBy(new DbClause.LongClause("asset_id", assetId), from, to);
		}

		public static DbIterator<AssetTransfer> getAccountAssetTransfers(long accountId, int from, int to)
		{
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM asset_transfer WHERE sender_id = ?" + " UNION ALL SELECT * FROM asset_transfer WHERE recipient_id = ? AND sender_id <> ? ORDER BY height DESC" + DbUtils.limitsClause(from, to));
				int i = 0;
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, accountId);
				DbUtils.setLimits(++i, pstmt, from, to);
				return assetTransferTable.getManyBy(con, pstmt, false);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public static DbIterator<AssetTransfer> getAccountAssetTransfers(long accountId, long assetId, int from, int to)
		{
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM asset_transfer WHERE sender_id = ? AND asset_id = ?" + " UNION ALL SELECT * FROM asset_transfer WHERE recipient_id = ? AND sender_id <> ? AND asset_id = ? ORDER BY height DESC" + DbUtils.limitsClause(from, to));
				int i = 0;
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, assetId);
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, assetId);
				DbUtils.setLimits(++i, pstmt, from, to);
				return assetTransferTable.getManyBy(con, pstmt, false);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public static int getTransferCount(long assetId)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT COUNT(*) FROM asset_transfer WHERE asset_id = ?"))
			{
				pstmt.setLong(1, assetId);
				using (ResultSet rs = pstmt.executeQuery())
				{
					rs.next();
					return rs.getInt(1);
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		static AssetTransfer addAssetTransfer(Transaction transaction, Attachment.ColoredCoinsAssetTransfer attachment)
		{
			AssetTransfer assetTransfer = new AssetTransfer(transaction, attachment);
			assetTransferTable.insert(assetTransfer);
			listeners.notify(assetTransfer, Event.ASSET_TRANSFER);
			return assetTransfer;
		}

		static void init()
		{
		}


		private final long id;
		private final DbKey dbKey;
		private final long assetId;
		private final int height;
		private final long senderId;
		private final long recipientId;
		private final long quantityQNT;
		private final int timestamp;

		private AssetTransfer(Transaction transaction, Attachment.ColoredCoinsAssetTransfer attachment)
		{
			this.id = transaction.Id;
			this.dbKey = transferDbKeyFactory.newKey(this.id);
			this.height = transaction.Height;
			this.assetId = attachment.AssetId;
			this.senderId = transaction.SenderId;
			this.recipientId = transaction.RecipientId;
			this.quantityQNT = attachment.QuantityQNT;
			this.timestamp = transaction.BlockTimestamp;
		}

		private AssetTransfer(ResultSet rs) throws SQLException
		{
			this.id = rs.getLong("id");
			this.dbKey = transferDbKeyFactory.newKey(this.id);
			this.assetId = rs.getLong("asset_id");
			this.senderId = rs.getLong("sender_id");
			this.recipientId = rs.getLong("recipient_id");
			this.quantityQNT = rs.getLong("quantity");
			this.timestamp = rs.getInt("timestamp");
			this.height = rs.getInt("height");
		}

		private void save(Connection con) throws SQLException
		{
			using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO asset_transfer (id, asset_id, " + "sender_id, recipient_id, quantity, timestamp, height) " + "VALUES (?, ?, ?, ?, ?, ?, ?)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.Id);
				pstmt.setLong(++i, this.AssetId);
				pstmt.setLong(++i, this.SenderId);
				pstmt.setLong(++i, this.RecipientId);
				pstmt.setLong(++i, this.QuantityQNT);
				pstmt.setInt(++i, this.Timestamp);
				pstmt.setInt(++i, this.Height);
				pstmt.executeUpdate();
			}
		}

		public long Id
		{
			return id;
		}

		public long AssetId
		{
			return assetId;
		}

		public long SenderId
		{
			return senderId;
		}

		public long RecipientId
		{
			return recipientId;
		}

		public long QuantityQNT
		{
			return quantityQNT;
		}

		public int Timestamp
		{
			return timestamp;
		}

		public int Height
		{
			return height;
		}

	}

}