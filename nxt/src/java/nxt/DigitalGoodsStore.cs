using System.Collections.Generic;

namespace nxt
{

	using EncryptedData = nxt.crypto.EncryptedData;
	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using VersionedEntityDbTable = nxt.db.VersionedEntityDbTable;
	using VersionedValuesDbTable = nxt.db.VersionedValuesDbTable;
	using Convert = nxt.util.Convert;
	using Listener = nxt.util.Listener;
	using Listeners = nxt.util.Listeners;


	public sealed class DigitalGoodsStore
	{

		public enum Event
		{
			GOODS_LISTED,
			GOODS_DELISTED,
			GOODS_PRICE_CHANGE,
			GOODS_QUANTITY_CHANGE,
			PURCHASE,
			DELIVERY,
			REFUND,
			FEEDBACK
		}

		static DigitalGoodsStore()
		{
			Nxt.BlockchainProcessor.addListener(new Listener<Block>() { public void notify(Block block) { try (DbIterator<Purchase> purchases = getExpiredPendingPurchases(block.Timestamp)) { while(purchases.hasNext()) { Purchase purchase = purchases.next(); Account buyer = Account.getAccount(purchase.BuyerId); buyer.addToUnconfirmedBalanceNQT(Convert.safeMultiply(purchase.Quantity, purchase.PriceNQT)); getGoods(purchase.GoodsId).changeQuantity(purchase.Quantity); purchase.setPending(false); } } } }, BlockchainProcessor.Event.AFTER_BLOCK_APPLY);
		}

		private static readonly Listeners<Goods, Event> goodsListeners = new Listeners<>();

		private static readonly Listeners<Purchase, Event> purchaseListeners = new Listeners<>();

		public static bool addGoodsListener(Listener<Goods> listener, Event eventType)
		{
			return goodsListeners.addListener(listener, eventType);
		}

		public static bool removeGoodsListener(Listener<Goods> listener, Event eventType)
		{
			return goodsListeners.removeListener(listener, eventType);
		}

		public static bool addPurchaseListener(Listener<Purchase> listener, Event eventType)
		{
			return purchaseListeners.addListener(listener, eventType);
		}

		public static bool removePurchaseListener(Listener<Purchase> listener, Event eventType)
		{
			return purchaseListeners.removeListener(listener, eventType);
		}

		internal static void init()
		{
			Goods.init();
			Purchase.init();
		}

		public sealed class Goods
		{

			private static final DbKey.LongKeyFactory<Goods> goodsDbKeyFactory = new DbKey.LongKeyFactory<Goods>("id")
			{

				public DbKey newKey(Goods goods)
				{
					return goods.dbKey;
				}

			}

			private static final VersionedEntityDbTable<Goods> goodsTable = new VersionedEntityDbTable<Goods>("goods", goodsDbKeyFactory)
			{

				protected Goods load(Connection con, ResultSet rs) throws SQLException
				{
					return new Goods(rs);
				}

				protected void save(Connection con, Goods goods) throws SQLException
				{
					goods.save(con);
				}

				protected string defaultSort()
				{
					return " ORDER BY timestamp DESC, id ASC ";
				}

			}

			static void init()
			{
			}


			private final long id;
			private final DbKey dbKey;
			private final long sellerId;
			private final string name;
			private final string description;
			private final string tags;
			private final int timestamp;
			private int quantity;
			private long priceNQT;
			private bool delisted;

			private Goods(Transaction transaction, Attachment.DigitalGoodsListing attachment)
			{
				this.id = transaction.Id;
				this.dbKey = goodsDbKeyFactory.newKey(this.id);
				this.sellerId = transaction.SenderId;
				this.name = attachment.Name;
				this.description = attachment.Description;
				this.tags = attachment.Tags;
				this.quantity = attachment.Quantity;
				this.priceNQT = attachment.PriceNQT;
				this.delisted = false;
				this.timestamp = transaction.Timestamp;
			}

			private Goods(ResultSet rs) throws SQLException
			{
				this.id = rs.getLong("id");
				this.dbKey = goodsDbKeyFactory.newKey(this.id);
				this.sellerId = rs.getLong("seller_id");
				this.name = rs.getString("name");
				this.description = rs.getString("description");
				this.tags = rs.getString("tags");
				this.quantity = rs.getInt("quantity");
				this.priceNQT = rs.getLong("price");
				this.delisted = rs.getBoolean("delisted");
				this.timestamp = rs.getInt("timestamp");
			}

			private void save(Connection con) throws SQLException
			{
				using (PreparedStatement pstmt = con.prepareStatement("MERGE INTO goods (id, seller_id, name, " + "description, tags, timestamp, quantity, price, delisted, height, latest) KEY (id, height) " + "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, TRUE)"))
				{
					int i = 0;
					pstmt.setLong(++i, this.Id);
					pstmt.setLong(++i, this.SellerId);
					pstmt.setString(++i, this.Name);
					pstmt.setString(++i, this.Description);
					pstmt.setString(++i, this.Tags);
					pstmt.setInt(++i, this.Timestamp);
					pstmt.setInt(++i, this.Quantity);
					pstmt.setLong(++i, this.PriceNQT);
					pstmt.setBoolean(++i, this.Delisted);
					pstmt.setInt(++i, Nxt.Blockchain.Height);
					pstmt.executeUpdate();
				}
			}

			public long Id
			{
				return id;
			}

			public long SellerId
			{
				return sellerId;
			}

			public string Name
			{
				return name;
			}

			public string Description
			{
				return description;
			}

			public string Tags
			{
				return tags;
			}

			public int Timestamp
			{
				return timestamp;
			}

			public int Quantity
			{
				return quantity;
			}

			private void changeQuantity(int deltaQuantity)
			{
				quantity += deltaQuantity;
				if(quantity < 0)
				{
					quantity = 0;
				}
				else if(quantity > Constants.MAX_DGS_LISTING_QUANTITY)
				{
					quantity = Constants.MAX_DGS_LISTING_QUANTITY;
				}
				goodsTable.insert(this);
			}

			public long PriceNQT
			{
				return priceNQT;
			}

			private void changePrice(long priceNQT)
			{
				this.priceNQT = priceNQT;
				goodsTable.insert(this);
			}

			public bool Delisted
			{
				return delisted;
			}

			private void setDelisted(bool delisted)
			{
				this.delisted = delisted;
				goodsTable.insert(this);
			}

//        
//        @Override
//        public int compareTo(Goods other) {
//            if (!name.equals(other.name)) {
//                return name.compareTo(other.name);
//            }
//            if (!description.equals(other.description)) {
//                return description.compareTo(other.description);
//            }
//            return Long.compare(id, other.id);
//        }
//        

		}

		public static final class Purchase
		{

			private static final DbKey.LongKeyFactory<Purchase> purchaseDbKeyFactory = new DbKey.LongKeyFactory<Purchase>("id")
			{

				public DbKey newKey(Purchase purchase)
				{
					return purchase.dbKey;
				}

			}

			private static final VersionedEntityDbTable<Purchase> purchaseTable = new VersionedEntityDbTable<Purchase>("purchase", purchaseDbKeyFactory)
			{

				protected Purchase load(Connection con, ResultSet rs) throws SQLException
				{
					return new Purchase(rs);
				}

				protected void save(Connection con, Purchase purchase) throws SQLException
				{
					purchase.save(con);
				}

				protected string defaultSort()
				{
					return " ORDER BY timestamp DESC, id ASC ";
				}

			}

			private static final DbKey.LongKeyFactory<Purchase> feedbackDbKeyFactory = new DbKey.LongKeyFactory<Purchase>("id")
			{

				public DbKey newKey(Purchase purchase)
				{
					return purchase.dbKey;
				}

			}

			private static final VersionedValuesDbTable<Purchase, EncryptedData> feedbackTable = new VersionedValuesDbTable<Purchase, EncryptedData>("purchase_feedback", feedbackDbKeyFactory)
			{

				protected EncryptedData load(Connection con, ResultSet rs) throws SQLException
				{
					sbyte[] data = rs.getBytes("feedback_data");
					sbyte[] nonce = rs.getBytes("feedback_nonce");
					return new EncryptedData(data, nonce);
				}

				protected void save(Connection con, Purchase purchase, EncryptedData encryptedData) throws SQLException
				{
					using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO purchase_feedback (id, feedback_data, feedback_nonce, " + "height, latest) VALUES (?, ?, ?, ?, TRUE)"))
					{
						int i = 0;
						pstmt.setLong(++i, purchase.Id);
						setEncryptedData(pstmt, encryptedData, ++i);
						++i;
						pstmt.setInt(++i, Nxt.Blockchain.Height);
						pstmt.executeUpdate();
					}
				}

			}

			private static final DbKey.LongKeyFactory<Purchase> publicFeedbackDbKeyFactory = new DbKey.LongKeyFactory<Purchase>("id")
			{

				public DbKey newKey(Purchase purchase)
				{
					return purchase.dbKey;
				}

			}

			private static final VersionedValuesDbTable<Purchase, string> publicFeedbackTable = new VersionedValuesDbTable<Purchase, string>("purchase_public_feedback", publicFeedbackDbKeyFactory)
			{

				protected string load(Connection con, ResultSet rs) throws SQLException
				{
					return rs.getString("public_feedback");
				}

				protected void save(Connection con, Purchase purchase, string publicFeedback) throws SQLException
				{
					using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO purchase_public_feedback (id, public_feedback, " + "height, latest) VALUES (?, ?, ?, TRUE)"))
					{
						int i = 0;
						pstmt.setLong(++i, purchase.Id);
						pstmt.setString(++i, publicFeedback);
						pstmt.setInt(++i, Nxt.Blockchain.Height);
						pstmt.executeUpdate();
					}
				}

			}

			static void init()
			{
			}


			private final long id;
			private final DbKey dbKey;
			private final long buyerId;
			private final long goodsId;
			private final long sellerId;
			private final int quantity;
			private final long priceNQT;
			private final int deadline;
			private final EncryptedData note;
			private final int timestamp;
			private bool isPending;
			private EncryptedData encryptedGoods;
			private bool goodsIsText;
			private EncryptedData refundNote;
			private bool hasFeedbackNotes;
			private IList<EncryptedData> feedbackNotes;
			private bool hasPublicFeedbacks;
			private IList<string> publicFeedbacks;
			private long discountNQT;
			private long refundNQT;

			private Purchase(Transaction transaction, Attachment.DigitalGoodsPurchase attachment, long sellerId)
			{
				this.id = transaction.Id;
				this.dbKey = purchaseDbKeyFactory.newKey(this.id);
				this.buyerId = transaction.SenderId;
				this.goodsId = attachment.GoodsId;
				this.sellerId = sellerId;
				this.quantity = attachment.Quantity;
				this.priceNQT = attachment.PriceNQT;
				this.deadline = attachment.DeliveryDeadlineTimestamp;
				this.note = transaction.EncryptedMessage == null ? null : transaction.EncryptedMessage.EncryptedData;
				this.timestamp = transaction.Timestamp;
				this.isPending = true;
			}

			private Purchase(ResultSet rs) throws SQLException
			{
				this.id = rs.getLong("id");
				this.dbKey = purchaseDbKeyFactory.newKey(this.id);
				this.buyerId = rs.getLong("buyer_id");
				this.goodsId = rs.getLong("goods_id");
				this.sellerId = rs.getLong("seller_id");
				this.quantity = rs.getInt("quantity");
				this.priceNQT = rs.getLong("price");
				this.deadline = rs.getInt("deadline");
				this.note = loadEncryptedData(rs, "note", "nonce");
				this.timestamp = rs.getInt("timestamp");
				this.isPending = rs.getBoolean("pending");
				this.encryptedGoods = loadEncryptedData(rs, "goods", "goods_nonce");
				this.refundNote = loadEncryptedData(rs, "refund_note", "refund_nonce");
				this.hasFeedbackNotes = rs.getBoolean("has_feedback_notes");
				this.hasPublicFeedbacks = rs.getBoolean("has_public_feedbacks");
				this.discountNQT = rs.getLong("discount");
				this.refundNQT = rs.getLong("refund");
			}

			private void save(Connection con) throws SQLException
			{
				using (PreparedStatement pstmt = con.prepareStatement("MERGE INTO purchase (id, buyer_id, goods_id, seller_id, " + "quantity, price, deadline, note, nonce, timestamp, pending, goods, goods_nonce, refund_note, " + "refund_nonce, has_feedback_notes, has_public_feedbacks, discount, refund, height, latest) KEY (id, height) " + "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, TRUE)"))
				{
					int i = 0;
					pstmt.setLong(++i, this.Id);
					pstmt.setLong(++i, this.BuyerId);
					pstmt.setLong(++i, this.GoodsId);
					pstmt.setLong(++i, this.SellerId);
					pstmt.setInt(++i, this.Quantity);
					pstmt.setLong(++i, this.PriceNQT);
					pstmt.setInt(++i, this.DeliveryDeadlineTimestamp);
					setEncryptedData(pstmt, this.Note, ++i);
					++i;
					pstmt.setInt(++i, this.Timestamp);
					pstmt.setBoolean(++i, this.Pending);
					setEncryptedData(pstmt, this.EncryptedGoods, ++i);
					++i;
					setEncryptedData(pstmt, this.RefundNote, ++i);
					++i;
					pstmt.setBoolean(++i, this.FeedbackNotes != null && this.FeedbackNotes.size() > 0);
					pstmt.setBoolean(++i, this.PublicFeedback != null && this.PublicFeedback.size() > 0);
					pstmt.setLong(++i, this.DiscountNQT);
					pstmt.setLong(++i, this.RefundNQT);
					pstmt.setInt(++i, Nxt.Blockchain.Height);
					pstmt.executeUpdate();
				}
			}

			public long Id
			{
				return id;
			}

			public long BuyerId
			{
				return buyerId;
			}

			public long GoodsId
			{
				return goodsId;
			}

			public long SellerId
			{
				return sellerId;
			}

			public int Quantity
			{
				return quantity;
			}

			public long PriceNQT
			{
				return priceNQT;
			}

			public int DeliveryDeadlineTimestamp
			{
				return deadline;
			}

			public EncryptedData Note
			{
				return note;
			}

			public bool Pending
			{
				return isPending;
			}

			private void setPending(bool isPending)
			{
				this.isPending = isPending;
				purchaseTable.insert(this);
			}

			public int Timestamp
			{
				return timestamp;
			}

			public string Name
			{
				return getGoods(goodsId).Name;
			}

			public EncryptedData EncryptedGoods
			{
				return encryptedGoods;
			}

			public bool goodsIsText()
			{
				return goodsIsText;
			}

			private void setEncryptedGoods(EncryptedData encryptedGoods, bool goodsIsText)
			{
				this.encryptedGoods = encryptedGoods;
				this.goodsIsText = goodsIsText;
				purchaseTable.insert(this);
			}

			public EncryptedData RefundNote
			{
				return refundNote;
			}

			private void setRefundNote(EncryptedData refundNote)
			{
				this.refundNote = refundNote;
				purchaseTable.insert(this);
			}

			public IList<EncryptedData> FeedbackNotes
			{
				if(!hasFeedbackNotes)
				{
					return null;
				}
				feedbackNotes = feedbackTable.get(feedbackDbKeyFactory.newKey(this));
				return feedbackNotes;
			}

			private void addFeedbackNote(EncryptedData feedbackNote)
			{
				if(feedbackNotes == null)
				{
					feedbackNotes = new List<>();
				}
				feedbackNotes.add(feedbackNote);
				this.hasFeedbackNotes = true;
				purchaseTable.insert(this);
				feedbackTable.insert(this, feedbackNotes);
			}

			public IList<string> PublicFeedback
			{
				if(!hasPublicFeedbacks)
				{
					return null;
				}
				publicFeedbacks = publicFeedbackTable.get(publicFeedbackDbKeyFactory.newKey(this));
				return publicFeedbacks;
			}

			private void addPublicFeedback(string publicFeedback)
			{
				if(publicFeedbacks == null)
				{
					publicFeedbacks = new List<>();
				}
				publicFeedbacks.add(publicFeedback);
				this.hasPublicFeedbacks = true;
				purchaseTable.insert(this);
				publicFeedbackTable.insert(this, publicFeedbacks);
			}

			public long DiscountNQT
			{
				return discountNQT;
			}

			public void setDiscountNQT(long discountNQT)
			{
				this.discountNQT = discountNQT;
				purchaseTable.insert(this);
			}

			public long RefundNQT
			{
				return refundNQT;
			}

			public void setRefundNQT(long refundNQT)
			{
				this.refundNQT = refundNQT;
				purchaseTable.insert(this);
			}

//        
//        @Override
//        public int compareTo(Purchase other) {
//            if (this.timestamp < other.timestamp) {
//                return 1;
//            }
//            if (this.timestamp > other.timestamp) {
//                return -1;
//            }
//            return Long.compare(this.id, other.id);
//        }
//        

		}

		public static Goods getGoods(long goodsId)
		{
			return Goods.goodsTable.get(Goods.goodsDbKeyFactory.newKey(goodsId));
		}

		public static DbIterator<Goods> getAllGoods(int from, int to)
		{
			return Goods.goodsTable.getAll(from, to);
		}

		public static DbIterator<Goods> getGoodsInStock(int from, int to)
		{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//			DbClause dbClause = new DbClause(" delisted = FALSE AND quantity > 0 ")
//		{
//			@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//			{
//				return index;
//			}
//		};
			return Goods.goodsTable.getManyBy(dbClause, from, to);
		}

		public static DbIterator<Goods> getSellerGoods(long sellerId, bool inStockOnly, int from, int to)
		{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//			DbClause dbClause = new DbClause(" seller_id = ? " + (inStockOnly ? "AND delisted = FALSE AND quantity > 0" : ""))
//		{
//			@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//			{
//				pstmt.setLong(index++, sellerId);
//				return index;
//			}
//		};
			return Goods.goodsTable.getManyBy(dbClause, from, to, " ORDER BY name ASC, timestamp DESC, id ASC ");
		}

		public static DbIterator<Purchase> getAllPurchases(int from, int to)
		{
			return Purchase.purchaseTable.getAll(from, to);
		}

		public static DbIterator<Purchase> getSellerPurchases(long sellerId, int from, int to)
		{
			return Purchase.purchaseTable.getManyBy(new DbClause.LongClause("seller_id", sellerId), from, to);
		}

		public static DbIterator<Purchase> getBuyerPurchases(long buyerId, int from, int to)
		{
			return Purchase.purchaseTable.getManyBy(new DbClause.LongClause("buyer_id", buyerId), from, to);
		}

		public static DbIterator<Purchase> getSellerBuyerPurchases(long sellerId, final long buyerId, int from, int to)
		{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//			DbClause dbClause = new DbClause(" seller_id = ? AND buyer_id = ? ")
//		{
//			@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//			{
//				pstmt.setLong(index++, sellerId);
//				pstmt.setLong(index++, buyerId);
//				return index;
//			}
//		};
			return Purchase.purchaseTable.getManyBy(dbClause, from, to);
		}

		public static Purchase getPurchase(long purchaseId)
		{
			return Purchase.purchaseTable.get(Purchase.purchaseDbKeyFactory.newKey(purchaseId));
		}

		public static DbIterator<Purchase> getPendingSellerPurchases(long sellerId, int from, int to)
		{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//			DbClause dbClause = new DbClause(" seller_id = ? AND pending = TRUE ")
//		{
//			@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//			{
//				pstmt.setLong(index++, sellerId);
//				return index;
//			}
//		};
			return Purchase.purchaseTable.getManyBy(dbClause, from, to);
		}

		static Purchase getPendingPurchase(long purchaseId)
		{
			Purchase purchase = getPurchase(purchaseId);
			return purchase == null || ! purchase.Pending ? null : purchase;
		}

		private static DbIterator<Purchase> getExpiredPendingPurchases(int timestamp)
		{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//			DbClause dbClause = new DbClause(" deadline < ? AND pending = TRUE ")
//		{
//			@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//			{
//				pstmt.setLong(index++, timestamp);
//				return index;
//			}
//		};
			return Purchase.purchaseTable.getManyBy(dbClause, 0, -1);
		}

		private static void addPurchase(Transaction transaction, Attachment.DigitalGoodsPurchase attachment, long sellerId)
		{
			Purchase purchase = new Purchase(transaction, attachment, sellerId);
			Purchase.purchaseTable.insert(purchase);
			purchaseListeners.notify(purchase, Event.PURCHASE);
		}

		static void listGoods(Transaction transaction, Attachment.DigitalGoodsListing attachment)
		{
			Goods goods = new Goods(transaction, attachment);
			Goods.goodsTable.insert(goods);
			goodsListeners.notify(goods, Event.GOODS_LISTED);
		}

		static void delistGoods(long goodsId)
		{
			Goods goods = Goods.goodsTable.get(Goods.goodsDbKeyFactory.newKey(goodsId));
			if(! goods.Delisted)
			{
				goods.Delisted = true;
				goodsListeners.notify(goods, Event.GOODS_DELISTED);
			}
			else
			{
				throw new InvalidOperationException("Goods already delisted");
			}
		}

		static void changePrice(long goodsId, long priceNQT)
		{
			Goods goods = Goods.goodsTable.get(Goods.goodsDbKeyFactory.newKey(goodsId));
			if(! goods.Delisted)
			{
				goods.changePrice(priceNQT);
				goodsListeners.notify(goods, Event.GOODS_PRICE_CHANGE);
			}
			else
			{
				throw new InvalidOperationException("Can't change price of delisted goods");
			}
		}

		static void changeQuantity(long goodsId, int deltaQuantity)
		{
			Goods goods = Goods.goodsTable.get(Goods.goodsDbKeyFactory.newKey(goodsId));
			if(! goods.Delisted)
			{
				goods.changeQuantity(deltaQuantity);
				goodsListeners.notify(goods, Event.GOODS_QUANTITY_CHANGE);
			}
			else
			{
				throw new InvalidOperationException("Can't change quantity of delisted goods");
			}
		}

		static void purchase(Transaction transaction, Attachment.DigitalGoodsPurchase attachment)
		{
			Goods goods = Goods.goodsTable.get(Goods.goodsDbKeyFactory.newKey(attachment.GoodsId));
			if(! goods.Delisted && attachment.Quantity <= goods.Quantity && attachment.PriceNQT == goods.PriceNQT && attachment.DeliveryDeadlineTimestamp > Nxt.Blockchain.LastBlock.Timestamp)
			{
				goods.changeQuantity(-attachment.Quantity);
				addPurchase(transaction, attachment, goods.SellerId);
			}
			else
			{
				Account buyer = Account.getAccount(transaction.SenderId);
				buyer.addToUnconfirmedBalanceNQT(Convert.safeMultiply(attachment.Quantity, attachment.PriceNQT));
			// restoring the unconfirmed balance if purchase not successful, however buyer still lost the transaction fees
			}
		}

		static void deliver(Transaction transaction, Attachment.DigitalGoodsDelivery attachment)
		{
			Purchase purchase = getPendingPurchase(attachment.PurchaseId);
			purchase.Pending = false;
			long totalWithoutDiscount = Convert.safeMultiply(purchase.Quantity, purchase.PriceNQT);
			Account buyer = Account.getAccount(purchase.BuyerId);
			buyer.addToBalanceNQT(Convert.safeSubtract(attachment.DiscountNQT, totalWithoutDiscount));
			buyer.addToUnconfirmedBalanceNQT(attachment.DiscountNQT);
			Account seller = Account.getAccount(transaction.SenderId);
			seller.addToBalanceAndUnconfirmedBalanceNQT(Convert.safeSubtract(totalWithoutDiscount, attachment.DiscountNQT));
			purchase.setEncryptedGoods(attachment.Goods, attachment.goodsIsText());
			purchase.DiscountNQT = attachment.DiscountNQT;
			purchaseListeners.notify(purchase, Event.DELIVERY);
		}

		static void refund(long sellerId, long purchaseId, long refundNQT, Appendix.EncryptedMessage encryptedMessage)
		{
			Purchase purchase = Purchase.purchaseTable.get(Purchase.purchaseDbKeyFactory.newKey(purchaseId));
			Account seller = Account.getAccount(sellerId);
			seller.addToBalanceNQT(-refundNQT);
			Account buyer = Account.getAccount(purchase.BuyerId);
			buyer.addToBalanceAndUnconfirmedBalanceNQT(refundNQT);
			if(encryptedMessage != null)
			{
				purchase.RefundNote = encryptedMessage.EncryptedData;
			}
			purchase.RefundNQT = refundNQT;
			purchaseListeners.notify(purchase, Event.REFUND);
		}

		static void feedback(long purchaseId, Appendix.EncryptedMessage encryptedMessage, Appendix.Message message)
		{
			Purchase purchase = Purchase.purchaseTable.get(Purchase.purchaseDbKeyFactory.newKey(purchaseId));
			if(encryptedMessage != null)
			{
				purchase.addFeedbackNote(encryptedMessage.EncryptedData);
			}
			if(message != null)
			{
				purchase.addPublicFeedback(Convert.ToString(message.Message));
			}
			purchaseListeners.notify(purchase, Event.FEEDBACK);
		}

		private static EncryptedData loadEncryptedData(ResultSet rs, string dataColumn, string nonceColumn) throws SQLException
		{
			sbyte[] data = rs.getBytes(dataColumn);
			if(data == null)
			{
				return null;
			}
			return new EncryptedData(data, rs.getBytes(nonceColumn));
		}

		private static void setEncryptedData(PreparedStatement pstmt, EncryptedData encryptedData, int i) throws SQLException
		{
			if(encryptedData == null)
			{
				pstmt.setNull(i, Types.VARBINARY);
				pstmt.setNull(i + 1, Types.VARBINARY);
			}
			else
			{
				pstmt.setBytes(i, encryptedData.Data);
				pstmt.setBytes(i + 1, encryptedData.Nonce);
			}
		}

	}

}