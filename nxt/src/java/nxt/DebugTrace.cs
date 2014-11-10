using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace nxt
{

	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using Listener = nxt.util.Listener;
	using Logger = nxt.util.Logger;


	public sealed class DebugTrace
	{

		internal const string QUOTE = Nxt.getStringProperty("nxt.debugTraceQuote", "");
		internal const string SEPARATOR = Nxt.getStringProperty("nxt.debugTraceSeparator", "\t");
		internal const bool LOG_UNCONFIRMED = Nxt.getBooleanProperty("nxt.debugLogUnconfirmed");

		internal static void init()
		{
			IList<string> accountIdStrings = Nxt.getStringListProperty("nxt.debugTraceAccounts");
			string logName = Nxt.getStringProperty("nxt.debugTraceLog");
			if(accountIdStrings.Count == 0 || logName == null)
			{
				return;
			}
			Set<long?> accountIds = new HashSet<>();
			foreach (string accountId in accountIdStrings)
			{
				if("*".Equals(accountId))
				{
					accountIds.clear();
					break;
				}
				accountIds.add(Convert.parseUnsignedLong(accountId));
			}
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DebugTrace debugTrace = addDebugTrace(accountIds, logName);
			DebugTrace debugTrace = addDebugTrace(accountIds, logName);
			Nxt.BlockchainProcessor.addListener(new Listener<Block>() { public void notify(Block block) { debugTrace.resetLog(); } }, BlockchainProcessor.Event.RESCAN_BEGIN);
			Logger.logDebugMessage("Debug tracing of " + (accountIdStrings.Contains("*") ? "ALL" : Convert.ToString(accountIds.size())) + " accounts enabled");
		}

		public static DebugTrace addDebugTrace(Set<long?> accountIds, string logName)
		{
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DebugTrace debugTrace = new DebugTrace(accountIds, logName);
			DebugTrace debugTrace = new DebugTrace(accountIds, logName);
			Trade.addListener(new Listener<Trade>() { public void notify(Trade trade) { debugTrace.trace(trade); } }, Trade.Event.TRADE);
			Account.addListener(new Listener<Account>() { public void notify(Account account) { debugTrace.trace(account, false); } }, Account.Event.BALANCE);
			if(LOG_UNCONFIRMED)
			{
				Account.addListener(new Listener<Account>() { public void notify(Account account) { debugTrace.trace(account, true); } }, Account.Event.UNCONFIRMED_BALANCE);
			}
			Account.addAssetListener(new Listener<Account.AccountAsset>() { public void notify(Account.AccountAsset accountAsset) { debugTrace.trace(accountAsset, false); } }, Account.Event.ASSET_BALANCE);
			if(LOG_UNCONFIRMED)
			{
				Account.addAssetListener(new Listener<Account.AccountAsset>() { public void notify(Account.AccountAsset accountAsset) { debugTrace.trace(accountAsset, true); } }, Account.Event.UNCONFIRMED_ASSET_BALANCE);
			}
			Account.addLeaseListener(new Listener<Account.AccountLease>() { public void notify(Account.AccountLease accountLease) { debugTrace.trace(accountLease, true); } }, Account.Event.LEASE_STARTED);
			Account.addLeaseListener(new Listener<Account.AccountLease>() { public void notify(Account.AccountLease accountLease) { debugTrace.trace(accountLease, false); } }, Account.Event.LEASE_ENDED);
			Nxt.BlockchainProcessor.addListener(new Listener<Block>() { public void notify(Block block) { debugTrace.traceBeforeAccept(block); } }, BlockchainProcessor.Event.BEFORE_BLOCK_ACCEPT);
			Nxt.BlockchainProcessor.addListener(new Listener<Block>() { public void notify(Block block) { debugTrace.trace(block); } }, BlockchainProcessor.Event.BEFORE_BLOCK_APPLY);
			return debugTrace;
		}

		private static readonly string[] columns = {"height", "event", "account", "asset", "balance", "unconfirmed balance", "asset balance", "unconfirmed asset balance", "transaction amount", "transaction fee", "generation fee", "effective balance", "order", "order price", "order quantity", "order cost", "trade price", "trade quantity", "trade cost", "asset quantity", "transaction", "lessee", "lessor guaranteed balance", "purchase", "purchase price", "purchase quantity", "purchase cost", "discount", "refund", "sender", "recipient", "block", "timestamp"};

		private static readonly IDictionary<string, string> headers = new Dictionary<>();
		static DebugTrace()
		{
			foreach (string entry in columns)
			{
				headers.Add(entry, entry);
			}
		}

		private readonly Set<long?> accountIds;
		private readonly string logName;
		private PrintWriter log;

		private DebugTrace(Set<long?> accountIds, string logName)
		{
			this.accountIds = accountIds;
			this.logName = logName;
			resetLog();
		}

		internal void resetLog()
		{
			if(log != null)
			{
				log.close();
			}
			try
			{
				log = new PrintWriter((new BufferedWriter(new OutputStreamWriter(new FileOutputStream(logName)))), true);
			}
			catch(IOException e)
			{
				Logger.logDebugMessage("Debug tracing to " + logName + " not possible", e);
				throw new Exception(e);
			}
			this.log(headers);
		}

		private bool include(long accountId)
		{
			return accountId != 0 && (accountIds.Empty || accountIds.contains(accountId));
		}

		private bool include(Attachment attachment)
		{
			if(attachment is Attachment.DigitalGoodsPurchase)
			{
				long sellerId = DigitalGoodsStore.getGoods(((Attachment.DigitalGoodsPurchase)attachment).GoodsId).SellerId;
				return include(sellerId);
			}
			else if(attachment is Attachment.DigitalGoodsDelivery)
			{
				long buyerId = DigitalGoodsStore.getPurchase(((Attachment.DigitalGoodsDelivery)attachment).PurchaseId).BuyerId;
				return include(buyerId);
			}
			else if(attachment is Attachment.DigitalGoodsRefund)
			{
				long buyerId = DigitalGoodsStore.getPurchase(((Attachment.DigitalGoodsRefund)attachment).PurchaseId).BuyerId;
				return include(buyerId);
			}
			return false;
		}

	// Note: Trade events occur before the change in account balances
		private void trace(Trade trade)
		{
			long askAccountId = Order.Ask.getAskOrder(trade.AskOrderId).AccountId;
			long bidAccountId = Order.Bid.getBidOrder(trade.BidOrderId).AccountId;
			if(include(askAccountId))
			{
				log(getValues(askAccountId, trade, true));
			}
			if(include(bidAccountId))
			{
				log(getValues(bidAccountId, trade, false));
			}
		}

		private void trace(Account account, bool unconfirmed)
		{
			if(include(account.Id))
			{
				log(getValues(account.Id, unconfirmed));
			}
		}

		private void trace(Account.AccountAsset accountAsset, bool unconfirmed)
		{
			if(! include(accountAsset.AccountId))
			{
				return;
			}
			log(getValues(accountAsset.AccountId, accountAsset, unconfirmed));
		}

		private void trace(Account.AccountLease accountLease, bool start)
		{
			if(! include(accountLease.lesseeId) && ! include(accountLease.lessorId))
			{
				return;
			}
			log(getValues(accountLease.lessorId, accountLease, start));
		}

		private void traceBeforeAccept(Block block)
		{
			long generatorId = block.GeneratorId;
			if(include(generatorId))
			{
				log(getValues(generatorId, block));
			}
			foreach (long accountId in accountIds)
			{
				Account account = Account.getAccount(accountId);
				if(account != null)
				{
					using (DbIterator<Account> lessors = account.Lessors)
					{
						while(lessors.hasNext())
						{
							log(lessorGuaranteedBalance(lessors.next(), accountId));
						}
					}
				}
			}
		}

		private void trace(Block block)
		{
			foreach (Transaction transaction in block.Transactions)
			{
				long senderId = transaction.SenderId;
				if(include(senderId))
				{
					log(getValues(senderId, transaction, false));
					log(getValues(senderId, transaction, transaction.Attachment, false));
				}
				long recipientId = transaction.RecipientId;
				if(include(recipientId))
				{
					log(getValues(recipientId, transaction, true));
					log(getValues(recipientId, transaction, transaction.Attachment, true));
				}
				else
				{
					Attachment attachment = transaction.Attachment;
					if(include(attachment))
					{
						log(getValues(recipientId, transaction, transaction.Attachment, true));
					}
				}
			}
		}

		private IDictionary<string, string> lessorGuaranteedBalance(Account account, long lesseeId)
		{
			IDictionary<string, string> map = new Dictionary<>();
			map.Add("account", Convert.toUnsignedLong(account.Id));
			map.Add("lessor guaranteed balance", Convert.ToString(account.getGuaranteedBalanceNQT(1440)));
			map.Add("lessee", Convert.toUnsignedLong(lesseeId));
			map.Add("timestamp", Convert.ToString(Nxt.Blockchain.LastBlock.Timestamp));
			map.Add("height", Convert.ToString(Nxt.Blockchain.Height));
			map.Add("event", "lessor guaranteed balance");
			return map;
		}

		private IDictionary<string, string> getValues(long accountId, bool unconfirmed)
		{
			IDictionary<string, string> map = new Dictionary<>();
			map.Add("account", Convert.toUnsignedLong(accountId));
			Account account = Account.getAccount(accountId);
			map.Add("balance", Convert.ToString(account != null ? account.BalanceNQT : 0));
			map.Add("unconfirmed balance", Convert.ToString(account != null ? account.UnconfirmedBalanceNQT : 0));
			map.Add("timestamp", Convert.ToString(Nxt.Blockchain.LastBlock.Timestamp));
			map.Add("height", Convert.ToString(Nxt.Blockchain.Height));
			map.Add("event", unconfirmed ? "unconfirmed balance" : "balance");
			return map;
		}

		private IDictionary<string, string> getValues(long accountId, Trade trade, bool isAsk)
		{
			IDictionary<string, string> map = getValues(accountId, false);
			map.Add("asset", Convert.toUnsignedLong(trade.AssetId));
			map.Add("trade quantity", Convert.ToString(isAsk ? - trade.QuantityQNT : trade.QuantityQNT));
			map.Add("trade price", Convert.ToString(trade.PriceNQT));
			long tradeCost = Convert.safeMultiply(trade.QuantityQNT, trade.PriceNQT);
			map.Add("trade cost", Convert.ToString((isAsk ? tradeCost : - tradeCost)));
			map.Add("event", "trade");
			return map;
		}

		private IDictionary<string, string> getValues(long accountId, Transaction transaction, bool isRecipient)
		{
			long amount = transaction.AmountNQT;
			long fee = transaction.FeeNQT;
			if(isRecipient)
			{
				fee = 0; // fee doesn't affect recipient account
			}
			else
			{
			// for sender the amounts are subtracted
				amount = - amount;
				fee = - fee;
			}
			if(fee == 0 && amount == 0)
			{
				return Collections.emptyMap();
			}
			IDictionary<string, string> map = getValues(accountId, false);
			map.Add("transaction amount", Convert.ToString(amount));
			map.Add("transaction fee", Convert.ToString(fee));
			map.Add("transaction", transaction.StringId);
			if(isRecipient)
			{
				map.Add("sender", Convert.toUnsignedLong(transaction.SenderId));
			}
			else
			{
				map.Add("recipient", Convert.toUnsignedLong(transaction.RecipientId));
			}
			map.Add("event", "transaction");
			return map;
		}

		private IDictionary<string, string> getValues(long accountId, Block block)
		{
			long fee = block.TotalFeeNQT;
			if(fee == 0)
			{
				return Collections.emptyMap();
			}
			IDictionary<string, string> map = getValues(accountId, false);
			map.Add("generation fee", Convert.ToString(fee));
			map.Add("block", block.StringId);
			map.Add("event", "block");
			map.Add("effective balance", Convert.ToString(Account.getAccount(accountId).EffectiveBalanceNXT));
			map.Add("timestamp", Convert.ToString(block.Timestamp));
			map.Add("height", Convert.ToString(block.Height));
			return map;
		}

		private IDictionary<string, string> getValues(long accountId, Account.AccountAsset accountAsset, bool unconfirmed)
		{
			IDictionary<string, string> map = new Dictionary<>();
			map.Add("account", Convert.toUnsignedLong(accountId));
			map.Add("asset", Convert.toUnsignedLong(accountAsset.AssetId));
			if(unconfirmed)
			{
				map.Add("unconfirmed asset balance", Convert.ToString(accountAsset.UnconfirmedQuantityQNT));
			}
			else
			{
				map.Add("asset balance", Convert.ToString(accountAsset.QuantityQNT));
			}
			map.Add("timestamp", Convert.ToString(Nxt.Blockchain.LastBlock.Timestamp));
			map.Add("height", Convert.ToString(Nxt.Blockchain.Height));
			map.Add("event", "asset balance");
			return map;
		}

		private IDictionary<string, string> getValues(long accountId, Account.AccountLease accountLease, bool start)
		{
			IDictionary<string, string> map = new Dictionary<>();
			map.Add("account", Convert.toUnsignedLong(accountId));
			map.Add("event", start ? "lease begin" : "lease end");
			map.Add("timestamp", Convert.ToString(Nxt.Blockchain.LastBlock.Timestamp));
			map.Add("height", Convert.ToString(Nxt.Blockchain.Height));
			map.Add("lessee", Convert.toUnsignedLong(accountLease.lesseeId));
			return map;
		}

		private IDictionary<string, string> getValues(long accountId, Transaction transaction, Attachment attachment, bool isRecipient)
		{
			IDictionary<string, string> map = getValues(accountId, false);
			if(attachment is Attachment.ColoredCoinsOrderPlacement)
			{
				if(isRecipient)
				{
					return Collections.emptyMap();
				}
				Attachment.ColoredCoinsOrderPlacement orderPlacement = (Attachment.ColoredCoinsOrderPlacement)attachment;
				bool isAsk = orderPlacement is Attachment.ColoredCoinsAskOrderPlacement;
				map.Add("asset", Convert.toUnsignedLong(orderPlacement.AssetId));
				map.Add("order", transaction.StringId);
				map.Add("order price", Convert.ToString(orderPlacement.PriceNQT));
				long quantity = orderPlacement.QuantityQNT;
				if(isAsk)
				{
					quantity = - quantity;
				}
				map.Add("order quantity", Convert.ToString(quantity));
				BigInteger orderCost = BigInteger.valueOf(orderPlacement.PriceNQT).multiply(BigInteger.valueOf(orderPlacement.QuantityQNT));
				if(! isAsk)
				{
					orderCost = orderCost.negate();
				}
				map.Add("order cost", orderCost.ToString());
				string @event = (isAsk ? "ask" : "bid") + " order";
				map.Add("event", @event);
			}
			else if(attachment is Attachment.ColoredCoinsAssetIssuance)
			{
				if(isRecipient)
				{
					return Collections.emptyMap();
				}
				Attachment.ColoredCoinsAssetIssuance assetIssuance = (Attachment.ColoredCoinsAssetIssuance)attachment;
				map.Add("asset", transaction.StringId);
				map.Add("asset quantity", Convert.ToString(assetIssuance.QuantityQNT));
				map.Add("event", "asset issuance");
			}
			else if(attachment is Attachment.ColoredCoinsAssetTransfer)
			{
				Attachment.ColoredCoinsAssetTransfer assetTransfer = (Attachment.ColoredCoinsAssetTransfer)attachment;
				map.Add("asset", Convert.toUnsignedLong(assetTransfer.AssetId));
				long quantity = assetTransfer.QuantityQNT;
				if(! isRecipient)
				{
					quantity = - quantity;
				}
				map.Add("asset quantity", Convert.ToString(quantity));
				map.Add("event", "asset transfer");
			}
			else if(attachment is Attachment.ColoredCoinsOrderCancellation)
			{
				Attachment.ColoredCoinsOrderCancellation orderCancellation = (Attachment.ColoredCoinsOrderCancellation)attachment;
				map.Add("order", Convert.toUnsignedLong(orderCancellation.OrderId));
				map.Add("event", "order cancel");
			}
			else if(attachment is Attachment.DigitalGoodsPurchase)
			{
				Attachment.DigitalGoodsPurchase purchase = (Attachment.DigitalGoodsPurchase)transaction.Attachment;
				if(isRecipient)
				{
					map = getValues(DigitalGoodsStore.getGoods(purchase.GoodsId).SellerId, false);
				}
				map.Add("event", "purchase");
				map.Add("purchase", transaction.StringId);
			}
			else if(attachment is Attachment.DigitalGoodsDelivery)
			{
				Attachment.DigitalGoodsDelivery delivery = (Attachment.DigitalGoodsDelivery)transaction.Attachment;
				DigitalGoodsStore.Purchase purchase = DigitalGoodsStore.getPurchase(delivery.PurchaseId);
				if(isRecipient)
				{
					map = getValues(purchase.BuyerId, false);
				}
				map.Add("event", "delivery");
				map.Add("purchase", Convert.toUnsignedLong(delivery.PurchaseId));
				long discount = delivery.DiscountNQT;
				map.Add("purchase price", Convert.ToString(purchase.PriceNQT));
				map.Add("purchase quantity", Convert.ToString(purchase.Quantity));
				long cost = Convert.safeMultiply(purchase.PriceNQT, purchase.Quantity);
				if(isRecipient)
				{
					cost = - cost;
				}
				map.Add("purchase cost", Convert.ToString(cost));
				if(! isRecipient)
				{
					discount = - discount;
				}
				map.Add("discount", Convert.ToString(discount));
			}
			else if(attachment is Attachment.DigitalGoodsRefund)
			{
				Attachment.DigitalGoodsRefund refund = (Attachment.DigitalGoodsRefund)transaction.Attachment;
				if(isRecipient)
				{
					map = getValues(DigitalGoodsStore.getPurchase(refund.PurchaseId).BuyerId, false);
				}
				map.Add("event", "refund");
				map.Add("purchase", Convert.toUnsignedLong(refund.PurchaseId));
				long refundNQT = refund.RefundNQT;
				if(! isRecipient)
				{
					refundNQT = - refundNQT;
				}
				map.Add("refund", Convert.ToString(refundNQT));
			}
			else if(attachment == Attachment.ARBITRARY_MESSAGE)
			{
				map = new Dictionary<>();
				map.Add("account", Convert.toUnsignedLong(accountId));
				map.Add("timestamp", Convert.ToString(Nxt.Blockchain.LastBlock.Timestamp));
				map.Add("height", Convert.ToString(Nxt.Blockchain.Height));
				map.Add("event", attachment == Attachment.ARBITRARY_MESSAGE ? "message" : "encrypted message");
				if(isRecipient)
				{
					map.Add("sender", Convert.toUnsignedLong(transaction.SenderId));
				}
				else
				{
					map.Add("recipient", Convert.toUnsignedLong(transaction.RecipientId));
				}
			}
			else
			{
				return Collections.emptyMap();
			}
			return map;
		}

		private void log(IDictionary<string, string> map)
		{
			if(map.Count == 0)
			{
				return;
			}
			StringBuilder buf = new StringBuilder();
			foreach (string column in columns)
			{
				if(!LOG_UNCONFIRMED && column.StartsWith("unconfirmed"))
				{
					continue;
				}
				string value = map[column];
				if(value != null)
				{
					buf.Append(QUOTE).append(value).append(QUOTE);
				}
				buf.Append(SEPARATOR);
			}
			log.println(buf.ToString());
		}

	}

}