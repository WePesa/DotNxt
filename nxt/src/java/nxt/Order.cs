using System;

namespace nxt
{

	using Db = nxt.db.Db;
	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using VersionedEntityDbTable = nxt.db.VersionedEntityDbTable;
	using Convert = nxt.util.Convert;


	public abstract class Order
	{

		private static void matchOrders(long assetId)
		{

			Order.Ask askOrder;
			Order.Bid bidOrder;

			while((askOrder = Ask.getNextOrder(assetId)) != null && (bidOrder = Bid.getNextOrder(assetId)) != null)
			{

				if(askOrder.PriceNQT > bidOrder.PriceNQT)
				{
					break;
				}


				Trade trade = Trade.addTrade(assetId, Nxt.Blockchain.LastBlock, askOrder, bidOrder);

				askOrder.updateQuantityQNT(Convert.safeSubtract(askOrder.QuantityQNT, trade.QuantityQNT));
				Account askAccount = Account.getAccount(askOrder.AccountId);
				askAccount.addToBalanceAndUnconfirmedBalanceNQT(Convert.safeMultiply(trade.QuantityQNT, trade.PriceNQT));
				askAccount.addToAssetBalanceQNT(assetId, -trade.QuantityQNT);

				bidOrder.updateQuantityQNT(Convert.safeSubtract(bidOrder.QuantityQNT, trade.QuantityQNT));
				Account bidAccount = Account.getAccount(bidOrder.AccountId);
				bidAccount.addToAssetAndUnconfirmedAssetBalanceQNT(assetId, trade.QuantityQNT);
				bidAccount.addToBalanceNQT(-Convert.safeMultiply(trade.QuantityQNT, trade.PriceNQT));
				bidAccount.addToUnconfirmedBalanceNQT(Convert.safeMultiply(trade.QuantityQNT, (bidOrder.PriceNQT - trade.PriceNQT)));

			}

		}

		internal static void init()
		{
			Ask.init();
			Bid.init();
		}


		private readonly long id;
		private readonly long accountId;
		private readonly long assetId;
		private readonly long priceNQT;
		private readonly int creationHeight;

		private long quantityQNT;

		private Order(Transaction transaction, Attachment.ColoredCoinsOrderPlacement attachment)
		{
			this.id = transaction.Id;
			this.accountId = transaction.SenderId;
			this.assetId = attachment.AssetId;
			this.quantityQNT = attachment.QuantityQNT;
			this.priceNQT = attachment.PriceNQT;
			this.creationHeight = transaction.Height;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private Order(ResultSet rs) throws SQLException
		private Order(ResultSet rs)
		{
			this.id = rs.getLong("id");
			this.accountId = rs.getLong("account_id");
			this.assetId = rs.getLong("asset_id");
			this.priceNQT = rs.getLong("price");
			this.quantityQNT = rs.getLong("quantity");
			this.creationHeight = rs.getInt("creation_height");
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void save(Connection con, String table) throws SQLException
		private void save(Connection con, string table)
		{
			using (PreparedStatement pstmt = con.prepareStatement("MERGE INTO " + table + " (id, account_id, asset_id, " + "price, quantity, creation_height, height, latest) KEY (id, height) VALUES (?, ?, ?, ?, ?, ?, ?, TRUE)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.Id);
				pstmt.setLong(++i, this.AccountId);
				pstmt.setLong(++i, this.AssetId);
				pstmt.setLong(++i, this.PriceNQT);
				pstmt.setLong(++i, this.QuantityQNT);
				pstmt.setInt(++i, this.Height);
				pstmt.setInt(++i, Nxt.Blockchain.Height);
				pstmt.executeUpdate();
			}
		}

		public virtual long Id
		{
			get
			{
				return id;
			}
		}

		public virtual long AccountId
		{
			get
			{
				return accountId;
			}
		}

		public virtual long AssetId
		{
			get
			{
				return assetId;
			}
		}

		public virtual long PriceNQT
		{
			get
			{
				return priceNQT;
			}
		}

		public long QuantityQNT
		{
			get
			{
				return quantityQNT;
			}
			set
			{
				this.quantityQNT = value;
			}
		}

		public virtual int Height
		{
			get
			{
				return creationHeight;
			}
		}

		public override string ToString()
		{
			return this.GetType().SimpleName + " id: " + Convert.toUnsignedLong(id) + " account: " + Convert.toUnsignedLong(accountId) + " asset: " + Convert.toUnsignedLong(assetId) + " price: " + priceNQT + " quantity: " + quantityQNT + " height: " + creationHeight;
		}


//    
//    private int compareTo(Order o) {
//        if (height < o.height) {
//            return -1;
//        } else if (height > o.height) {
//            return 1;
//        } else {
//            if (id < o.id) {
//                return -1;
//            } else if (id > o.id) {
//                return 1;
//            } else {
//                return 0;
//            }
//        }
//
//    }
//    

		public sealed class Ask : Order
		{

			private static final DbKey.LongKeyFactory<Ask> askOrderDbKeyFactory = new DbKey.LongKeyFactory<Ask>("id")
			{

				public DbKey newKey(Ask ask)
				{
					return ask.dbKey;
				}

			}

			private static final VersionedEntityDbTable<Ask> askOrderTable = new VersionedEntityDbTable<Ask>("ask_order", askOrderDbKeyFactory)
			{
				protected Ask load(Connection con, ResultSet rs) throws SQLException
				{
					return new Ask(rs);
				}

				protected void save(Connection con, Ask ask) throws SQLException
				{
					ask.save(con, table);
				}

				protected string defaultSort()
				{
					return " ORDER BY creation_height DESC ";
				}

			}

			public static int Count
			{
				return askOrderTable.Count;
			}

			public static Ask getAskOrder(long orderId)
			{
				return askOrderTable.get(askOrderDbKeyFactory.newKey(orderId));
			}

			public static DbIterator<Ask> getAll(int from, int to)
			{
				return askOrderTable.getAll(from, to);
			}

//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
			public static DbIterator<Ask> getAskOrdersByAccount(long accountId, int from, int to)
			{
//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
				return askOrderTable.getManyBy(new DbClause.LongClause("account_id", accountId), from, to);
			}

//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
			public static DbIterator<Ask> getAskOrdersByAsset(long assetId, int from, int to)
			{
//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
				return askOrderTable.getManyBy(new DbClause.LongClause("asset_id", assetId), from, to);
			}

//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
			public static DbIterator<Ask> getAskOrdersByAccountAsset(final long accountId, final long assetId, int from, int to)
			{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//				DbClause dbClause = new DbClause(" account_id = ? AND asset_id = ? ")
//			{
//				@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//				{
//					pstmt.setLong(index++, accountId);
//					pstmt.setLong(index++, assetId);
//					return index;
//				}
//			};
				return askOrderTable.getManyBy(dbClause, from, to);
			}

//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
			public static DbIterator<Ask> getSortedOrders(long assetId, int from, int to)
			{
//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
				return askOrderTable.getManyBy(new DbClause.LongClause("asset_id", assetId), from, to, " ORDER BY price ASC, creation_height ASC, id ASC ");
			}

//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
			private static Ask getNextOrder(long assetId)
			{
				using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM ask_order WHERE asset_id = ? " + "AND latest = TRUE ORDER BY price ASC, creation_height ASC, id ASC LIMIT 1"))
				{
//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
					pstmt.setLong(1, assetId);
					using (DbIterator<Ask> askOrders = askOrderTable.getManyBy(con, pstmt, true))
					{
						return askOrders.hasNext() ? askOrders.next() : null;
					}
				}
				catch(SQLException e)
				{
					throw new Exception(e.ToString(), e);
				}
			}

			static void addOrder(Transaction transaction, Attachment.ColoredCoinsAskOrderPlacement attachment)
			{
				Ask order = new Ask(transaction, attachment);
				askOrderTable.insert(order);
				matchOrders(attachment.AssetId);
			}

			static void removeOrder(long orderId)
			{
				askOrderTable.delete(getAskOrder(orderId));
			}

			static void init()
			{
			}


			private final DbKey dbKey;

			private Ask(Transaction transaction, Attachment.ColoredCoinsAskOrderPlacement attachment)
			{
				base(transaction, attachment);
				this.dbKey = askOrderDbKeyFactory.newKey(base.id);
			}

			private Ask(ResultSet rs) throws SQLException
			{
				base(rs);
				this.dbKey = askOrderDbKeyFactory.newKey(base.id);
			}

			private void save(Connection con, string table) throws SQLException
			{
				base.save(con, table);
			}

//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
			private void updateQuantityQNT(long quantityQNT)
			{
//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
				base.QuantityQNT = quantityQNT;
//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
				if(quantityQNT > 0)
				{
					askOrderTable.insert(this);
				}
//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
				else if(quantityQNT == 0)
				{
					askOrderTable.delete(this);
				}
				else
				{
//JAVA TO VB & C# CONVERTER TODO TASK: C# doesn't allow accessing outer class instance members within a nested class:
					throw new System.ArgumentException("Negative quantity: " + quantityQNT + " for order: " + Convert.toUnsignedLong(Id));
				}
			}

//        
//        @Override
//        public int compareTo(Ask o) {
//            if (this.getPriceNQT() < o.getPriceNQT()) {
//                return -1;
//            } else if (this.getPriceNQT() > o.getPriceNQT()) {
//                return 1;
//            } else {
//                return super.compareTo(o);
//            }
//        }
//        

		}

		public static final class Bid extends Order
		{

			private static final DbKey.LongKeyFactory<Bid> bidOrderDbKeyFactory = new DbKey.LongKeyFactory<Bid>("id")
			{

				public DbKey newKey(Bid bid)
				{
					return bid.dbKey;
				}

			}

			private static final VersionedEntityDbTable<Bid> bidOrderTable = new VersionedEntityDbTable<Bid>("bid_order", bidOrderDbKeyFactory)
			{

				protected Bid load(Connection con, ResultSet rs) throws SQLException
				{
					return new Bid(rs);
				}

				protected void save(Connection con, Bid bid) throws SQLException
				{
					bid.save(con, table);
				}

				protected string defaultSort()
				{
					return " ORDER BY creation_height DESC ";
				}

			}

			public static int Count
			{
				return bidOrderTable.Count;
			}

			public static Bid getBidOrder(long orderId)
			{
				return bidOrderTable.get(bidOrderDbKeyFactory.newKey(orderId));
			}

			public static DbIterator<Bid> getAll(int from, int to)
			{
				return bidOrderTable.getAll(from, to);
			}

			public static DbIterator<Bid> getBidOrdersByAccount(long accountId, int from, int to)
			{
				return bidOrderTable.getManyBy(new DbClause.LongClause("account_id", accountId), from, to);
			}

			public static DbIterator<Bid> getBidOrdersByAsset(long assetId, int from, int to)
			{
				return bidOrderTable.getManyBy(new DbClause.LongClause("asset_id", assetId), from, to);
			}

			public static DbIterator<Bid> getBidOrdersByAccountAsset(final long accountId, final long assetId, int from, int to)
			{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//				DbClause dbClause = new DbClause(" account_id = ? AND asset_id = ? ")
//			{
//				@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//				{
//					pstmt.setLong(index++, accountId);
//					pstmt.setLong(index++, assetId);
//					return index;
//				}
//			};
				return bidOrderTable.getManyBy(dbClause, from, to);
			}

			public static DbIterator<Bid> getSortedOrders(long assetId, int from, int to)
			{
				return bidOrderTable.getManyBy(new DbClause.LongClause("asset_id", assetId), from, to, " ORDER BY price DESC, creation_height ASC, id ASC ");
			}

			private static Bid getNextOrder(long assetId)
			{
				using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM bid_order WHERE asset_id = ? " + "AND latest = TRUE ORDER BY price DESC, creation_height ASC, id ASC LIMIT 1"))
				{
					pstmt.setLong(1, assetId);
					using (DbIterator<Bid> bidOrders = bidOrderTable.getManyBy(con, pstmt, true))
					{
						return bidOrders.hasNext() ? bidOrders.next() : null;
					}
				}
				catch(SQLException e)
				{
					throw new Exception(e.ToString(), e);
				}
			}

			static void addOrder(Transaction transaction, Attachment.ColoredCoinsBidOrderPlacement attachment)
			{
				Bid order = new Bid(transaction, attachment);
				bidOrderTable.insert(order);
				matchOrders(attachment.AssetId);
			}

			static void removeOrder(long orderId)
			{
				bidOrderTable.delete(getBidOrder(orderId));
			}

			static void init()
			{
			}


			private final DbKey dbKey;

			private Bid(Transaction transaction, Attachment.ColoredCoinsBidOrderPlacement attachment)
			{
				base(transaction, attachment);
				this.dbKey = bidOrderDbKeyFactory.newKey(base.id);
			}

			private Bid(ResultSet rs) throws SQLException
			{
				base(rs);
				this.dbKey = bidOrderDbKeyFactory.newKey(base.id);
			}

			private void save(Connection con, string table) throws SQLException
			{
				base.save(con, table);
			}

			private void updateQuantityQNT(long quantityQNT)
			{
				base.QuantityQNT = quantityQNT;
				if(quantityQNT > 0)
				{
					bidOrderTable.insert(this);
				}
				else if(quantityQNT == 0)
				{
					bidOrderTable.delete(this);
				}
				else
				{
					throw new System.ArgumentException("Negative quantity: " + quantityQNT + " for order: " + Convert.toUnsignedLong(Id));
				}
			}

//        
//        @Override
//        public int compareTo(Bid o) {
//            if (this.getPriceNQT() > o.getPriceNQT()) {
//                return -1;
//            } else if (this.getPriceNQT() < o.getPriceNQT()) {
//                return 1;
//            } else {
//                return super.compareTo(o);
//            }
//        }
//        
		}
	}

}