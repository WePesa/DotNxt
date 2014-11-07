using System;

namespace nxt
{

	using Db = nxt.db.Db;
	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using DbUtils = nxt.db.DbUtils;
	using EntityDbTable = nxt.db.EntityDbTable;
	using Convert = nxt.util.Convert;
	using Listener = nxt.util.Listener;
	using Listeners = nxt.util.Listeners;


	public sealed class Trade
	{

		public enum Event
		{
			TRADE
		}

		private static readonly Listeners<Trade, Event> listeners = new Listeners<>();

		private static final DbKey.LinkKeyFactory<Trade> tradeDbKeyFactory = new DbKey.LinkKeyFactory<Trade>("ask_order_id", "bid_order_id")
		{

			public DbKey newKey(Trade trade)
			{
				return trade.dbKey;
			}

		}

		private static final EntityDbTable<Trade> tradeTable = new EntityDbTable<Trade>("trade", tradeDbKeyFactory)
		{

			protected Trade load(Connection con, ResultSet rs) throws SQLException
			{
				return new Trade(rs);
			}

			protected void save(Connection con, Trade trade) throws SQLException
			{
				trade.save(con);
			}

		}

		public static DbIterator<Trade> getAllTrades(int from, int to)
		{
			return tradeTable.getAll(from, to);
		}

		public static int Count
		{
			return tradeTable.Count;
		}

		public static bool addListener(Listener<Trade> listener, Event eventType)
		{
			return listeners.addListener(listener, eventType);
		}

		public static bool removeListener(Listener<Trade> listener, Event eventType)
		{
			return listeners.removeListener(listener, eventType);
		}

		public static DbIterator<Trade> getAssetTrades(long assetId, int from, int to)
		{
			return tradeTable.getManyBy(new DbClause.LongClause("asset_id", assetId), from, to);
		}

		public static DbIterator<Trade> getAccountTrades(long accountId, int from, int to)
		{
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM trade WHERE seller_id = ?" + " UNION ALL SELECT * FROM trade WHERE buyer_id = ? AND seller_id <> ? ORDER BY height DESC" + DbUtils.limitsClause(from, to));
				int i = 0;
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, accountId);
				DbUtils.setLimits(++i, pstmt, from, to);
				return tradeTable.getManyBy(con, pstmt, false);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public static DbIterator<Trade> getAccountAssetTrades(long accountId, long assetId, int from, int to)
		{
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM trade WHERE seller_id = ? AND asset_id = ?" + " UNION ALL SELECT * FROM trade WHERE buyer_id = ? AND seller_id <> ? AND asset_id = ? ORDER BY height DESC" + DbUtils.limitsClause(from, to));
				int i = 0;
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, assetId);
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, accountId);
				pstmt.setLong(++i, assetId);
				DbUtils.setLimits(++i, pstmt, from, to);
				return tradeTable.getManyBy(con, pstmt, false);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public static int getTradeCount(long assetId)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT COUNT(*) FROM trade WHERE asset_id = ?"))
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

		static Trade addTrade(long assetId, Block block, Order.Ask askOrder, Order.Bid bidOrder)
		{
			Trade trade = new Trade(assetId, block, askOrder, bidOrder);
			tradeTable.insert(trade);
			listeners.notify(trade, Event.TRADE);
			return trade;
		}

		static void init()
		{
		}


		private final int timestamp;
		private final long assetId;
		private final long blockId;
		private final int height;
		private final long askOrderId;
		private final long bidOrderId;
		private final int askOrderHeight;
		private final int bidOrderHeight;
		private final long sellerId;
		private final long buyerId;
		private final DbKey dbKey;
		private final long quantityQNT;
		private final long priceNQT;
		private final bool isBuy;

		private Trade(long assetId, Block block, Order.Ask askOrder, Order.Bid bidOrder)
		{
			this.blockId = block.Id;
			this.height = block.Height;
			this.assetId = assetId;
			this.timestamp = block.Timestamp;
			this.askOrderId = askOrder.Id;
			this.bidOrderId = bidOrder.Id;
			this.askOrderHeight = askOrder.Height;
			this.bidOrderHeight = bidOrder.Height;
			this.sellerId = askOrder.AccountId;
			this.buyerId = bidOrder.AccountId;
			this.dbKey = tradeDbKeyFactory.newKey(this.askOrderId, this.bidOrderId);
			this.quantityQNT = Math.Min(askOrder.QuantityQNT, bidOrder.QuantityQNT);
			this.isBuy = askOrderHeight < bidOrderHeight || (askOrderHeight == bidOrderHeight && askOrderId < bidOrderId);
			this.priceNQT = isBuy ? askOrder.PriceNQT : bidOrder.PriceNQT;
		}

		private Trade(ResultSet rs) throws SQLException
		{
			this.assetId = rs.getLong("asset_id");
			this.blockId = rs.getLong("block_id");
			this.askOrderId = rs.getLong("ask_order_id");
			this.bidOrderId = rs.getLong("bid_order_id");
			this.askOrderHeight = rs.getInt("ask_order_height");
			this.bidOrderHeight = rs.getInt("bid_order_height");
			this.sellerId = rs.getLong("seller_id");
			this.buyerId = rs.getLong("buyer_id");
			this.dbKey = tradeDbKeyFactory.newKey(this.askOrderId, this.bidOrderId);
			this.quantityQNT = rs.getLong("quantity");
			this.priceNQT = rs.getLong("price");
			this.timestamp = rs.getInt("timestamp");
			this.height = rs.getInt("height");
			this.isBuy = askOrderHeight < bidOrderHeight || (askOrderHeight == bidOrderHeight && askOrderId < bidOrderId);
		}

		private void save(Connection con) throws SQLException
		{
			using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO trade (asset_id, block_id, " + "ask_order_id, bid_order_id, ask_order_height, bid_order_height, seller_id, buyer_id, quantity, price, timestamp, height) " + "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.AssetId);
				pstmt.setLong(++i, this.BlockId);
				pstmt.setLong(++i, this.AskOrderId);
				pstmt.setLong(++i, this.BidOrderId);
				pstmt.setInt(++i, this.AskOrderHeight);
				pstmt.setInt(++i, this.BidOrderHeight);
				pstmt.setLong(++i, this.SellerId);
				pstmt.setLong(++i, this.BuyerId);
				pstmt.setLong(++i, this.QuantityQNT);
				pstmt.setLong(++i, this.PriceNQT);
				pstmt.setInt(++i, this.Timestamp);
				pstmt.setInt(++i, this.Height);
				pstmt.executeUpdate();
			}
		}

		public long BlockId
		{
			return blockId;
		}

		public long AskOrderId
		{
			return askOrderId;
		}

		public long BidOrderId
		{
			return bidOrderId;
		}

		public int AskOrderHeight
		{
			return askOrderHeight;
		}

		public int BidOrderHeight
		{
			return bidOrderHeight;
		}

		public long SellerId
		{
			return sellerId;
		}

		public long BuyerId
		{
			return buyerId;
		}

		public long QuantityQNT
		{
			return quantityQNT;
		}

		public long PriceNQT
		{
			return priceNQT;
		}

		public long AssetId
		{
			return assetId;
		}

		public int Timestamp
		{
			return timestamp;
		}

		public int Height
		{
			return height;
		}

		public bool Buy
		{
			return isBuy;
		}

		public string ToString()
		{
			return "Trade asset: " + Convert.toUnsignedLong(assetId) + " ask: " + Convert.toUnsignedLong(askOrderId) + " bid: " + Convert.toUnsignedLong(bidOrderId) + " price: " + priceNQT + " quantity: " + quantityQNT + " height: " + height;
		}

	}

}