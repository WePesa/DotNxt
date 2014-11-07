using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Alias = nxt.Alias;
	using Appendix = nxt.Appendix;
	using Asset = nxt.Asset;
	using AssetTransfer = nxt.AssetTransfer;
	using Block = nxt.Block;
	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using Nxt = nxt.Nxt;
	using Order = nxt.Order;
	using Poll = nxt.Poll;
	using Token = nxt.Token;
	using Trade = nxt.Trade;
	using Transaction = nxt.Transaction;
	using Crypto = nxt.crypto.Crypto;
	using EncryptedData = nxt.crypto.EncryptedData;
	using Hallmark = nxt.peer.Hallmark;
	using Peer = nxt.peer.Peer;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;


	internal sealed class JSONData
	{

		internal static JSONObject alias(Alias alias)
		{
			JSONObject json = new JSONObject();
			putAccount(json, "account", alias.AccountId);
			json.put("aliasName", alias.AliasName);
			json.put("aliasURI", alias.AliasURI);
			json.put("timestamp", alias.Timestamp);
			json.put("alias", Convert.toUnsignedLong(alias.Id));
			Alias.Offer offer = Alias.getOffer(alias);
			if(offer != null)
			{
				json.put("priceNQT", Convert.ToString(offer.PriceNQT));
				if(offer.BuyerId != 0)
				{
					json.put("buyer", Convert.toUnsignedLong(offer.BuyerId));
				}
			}
			return json;
		}

		internal static JSONObject accountBalance(Account account)
		{
			JSONObject json = new JSONObject();
			if(account == null)
			{
				json.put("balanceNQT", "0");
				json.put("unconfirmedBalanceNQT", "0");
				json.put("effectiveBalanceNXT", "0");
				json.put("forgedBalanceNQT", "0");
				json.put("guaranteedBalanceNQT", "0");
			}
			else
			{
				json.put("balanceNQT", Convert.ToString(account.BalanceNQT));
				json.put("unconfirmedBalanceNQT", Convert.ToString(account.UnconfirmedBalanceNQT));
				json.put("effectiveBalanceNXT", account.EffectiveBalanceNXT);
				json.put("forgedBalanceNQT", Convert.ToString(account.ForgedBalanceNQT));
				json.put("guaranteedBalanceNQT", Convert.ToString(account.getGuaranteedBalanceNQT(1440)));
			}
			return json;
		}

		internal static JSONObject asset(Asset asset)
		{
			JSONObject json = new JSONObject();
			putAccount(json, "account", asset.AccountId);
			json.put("name", asset.Name);
			json.put("description", asset.Description);
			json.put("decimals", asset.Decimals);
			json.put("quantityQNT", Convert.ToString(asset.QuantityQNT));
			json.put("asset", Convert.toUnsignedLong(asset.Id));
			json.put("numberOfTrades", Trade.getTradeCount(asset.Id));
			json.put("numberOfTransfers", AssetTransfer.getTransferCount(asset.Id));
			json.put("numberOfAccounts", Account.getAssetAccountsCount(asset.Id));
			return json;
		}

		internal static JSONObject accountAsset(Account.AccountAsset accountAsset)
		{
			JSONObject json = new JSONObject();
			putAccount(json, "account", accountAsset.AccountId);
			json.put("asset", Convert.toUnsignedLong(accountAsset.AssetId));
			json.put("quantityQNT", Convert.ToString(accountAsset.QuantityQNT));
			json.put("unconfirmedQuantityQNT", Convert.ToString(accountAsset.UnconfirmedQuantityQNT));
			return json;
		}

		internal static JSONObject askOrder(Order.Ask order)
		{
			JSONObject json = order(order);
			json.put("type", "ask");
			return json;
		}

		internal static JSONObject bidOrder(Order.Bid order)
		{
			JSONObject json = order(order);
			json.put("type", "bid");
			return json;
		}

		internal static JSONObject order(Order order)
		{
			JSONObject json = new JSONObject();
			json.put("order", Convert.toUnsignedLong(order.Id));
			json.put("asset", Convert.toUnsignedLong(order.AssetId));
			putAccount(json, "account", order.AccountId);
			json.put("quantityQNT", Convert.ToString(order.QuantityQNT));
			json.put("priceNQT", Convert.ToString(order.PriceNQT));
			json.put("height", order.Height);
			return json;
		}

		internal static JSONObject block(Block block, bool includeTransactions)
		{
			JSONObject json = new JSONObject();
			json.put("block", block.StringId);
			json.put("height", block.Height);
			putAccount(json, "generator", block.GeneratorId);
			json.put("generatorPublicKey", Convert.toHexString(block.GeneratorPublicKey));
			json.put("timestamp", block.Timestamp);
			json.put("numberOfTransactions", block.Transactions.size());
			json.put("totalAmountNQT", Convert.ToString(block.TotalAmountNQT));
			json.put("totalFeeNQT", Convert.ToString(block.TotalFeeNQT));
			json.put("payloadLength", block.PayloadLength);
			json.put("version", block.Version);
			json.put("baseTarget", Convert.toUnsignedLong(block.BaseTarget));
			if(block.PreviousBlockId != 0)
			{
				json.put("previousBlock", Convert.toUnsignedLong(block.PreviousBlockId));
			}
			if(block.NextBlockId != 0)
			{
				json.put("nextBlock", Convert.toUnsignedLong(block.NextBlockId));
			}
			json.put("payloadHash", Convert.toHexString(block.PayloadHash));
			json.put("generationSignature", Convert.toHexString(block.GenerationSignature));
			if(block.Version > 1)
			{
				json.put("previousBlockHash", Convert.toHexString(block.PreviousBlockHash));
			}
			json.put("blockSignature", Convert.toHexString(block.BlockSignature));
			JSONArray transactions = new JSONArray();
			foreach (Transaction transaction in block.Transactions)
			{
				transactions.add(includeTransactions ? transaction(transaction) : Convert.toUnsignedLong(transaction.Id));
			}
			json.put("transactions", transactions);
			return json;
		}

		internal static JSONObject encryptedData(EncryptedData encryptedData)
		{
			JSONObject json = new JSONObject();
			json.put("data", Convert.toHexString(encryptedData.Data));
			json.put("nonce", Convert.toHexString(encryptedData.Nonce));
			return json;
		}

		internal static JSONObject goods(DigitalGoodsStore.Goods goods)
		{
			JSONObject json = new JSONObject();
			json.put("goods", Convert.toUnsignedLong(goods.Id));
			json.put("name", goods.Name);
			json.put("description", goods.Description);
			json.put("quantity", goods.Quantity);
			json.put("priceNQT", Convert.ToString(goods.PriceNQT));
			putAccount(json, "seller", goods.SellerId);
			json.put("tags", goods.Tags);
			json.put("delisted", goods.Delisted);
			json.put("timestamp", goods.Timestamp);
			return json;
		}

		internal static JSONObject hallmark(Hallmark hallmark)
		{
			JSONObject json = new JSONObject();
			putAccount(json, "account", Account.getId(hallmark.PublicKey));
			json.put("host", hallmark.Host);
			json.put("weight", hallmark.Weight);
			string dateString = Hallmark.formatDate(hallmark.Date);
			json.put("date", dateString);
			json.put("valid", hallmark.Valid);
			return json;
		}

		internal static JSONObject token(Token token)
		{
			JSONObject json = new JSONObject();
			putAccount(json, "account", Account.getId(token.PublicKey));
			json.put("timestamp", token.Timestamp);
			json.put("valid", token.Valid);
			return json;
		}

		internal static JSONObject peer(Peer peer)
		{
			JSONObject json = new JSONObject();
			json.put("state", peer.State.ordinal());
			json.put("announcedAddress", peer.AnnouncedAddress);
			json.put("shareAddress", peer.shareAddress());
			if(peer.Hallmark != null)
			{
				json.put("hallmark", peer.Hallmark.HallmarkString);
			}
			json.put("weight", peer.Weight);
			json.put("downloadedVolume", peer.DownloadedVolume);
			json.put("uploadedVolume", peer.UploadedVolume);
			json.put("application", peer.Application);
			json.put("version", peer.Version);
			json.put("platform", peer.Platform);
			json.put("blacklisted", peer.Blacklisted);
			json.put("lastUpdated", peer.LastUpdated);
			return json;
		}

		internal static JSONObject poll(Poll poll)
		{
			JSONObject json = new JSONObject();
			json.put("name", poll.Name);
			json.put("description", poll.Description);
			JSONArray options = new JSONArray();
			Collections.addAll(options, poll.Options);
			json.put("options", options);
			json.put("minNumberOfOptions", poll.MinNumberOfOptions);
			json.put("maxNumberOfOptions", poll.MaxNumberOfOptions);
			json.put("optionsAreBinary", poll.OptionsAreBinary);
			JSONArray voters = new JSONArray();
			foreach (long? voterId in poll.Voters.Keys)
			{
				voters.add(Convert.toUnsignedLong(voterId));
			}
			json.put("voters", voters);
			return json;
		}

		internal static JSONObject purchase(DigitalGoodsStore.Purchase purchase)
		{
			JSONObject json = new JSONObject();
			json.put("purchase", Convert.toUnsignedLong(purchase.Id));
			json.put("goods", Convert.toUnsignedLong(purchase.GoodsId));
			json.put("name", purchase.Name);
			putAccount(json, "seller", purchase.SellerId);
			json.put("priceNQT", Convert.ToString(purchase.PriceNQT));
			json.put("quantity", purchase.Quantity);
			putAccount(json, "buyer", purchase.BuyerId);
			json.put("timestamp", purchase.Timestamp);
			json.put("deliveryDeadlineTimestamp", purchase.DeliveryDeadlineTimestamp);
			if(purchase.Note != null)
			{
				json.put("note", encryptedData(purchase.Note));
			}
			json.put("pending", purchase.Pending);
			if(purchase.EncryptedGoods != null)
			{
				json.put("goodsData", encryptedData(purchase.EncryptedGoods));
				json.put("goodsIsText", purchase.goodsIsText());
			}
			if(purchase.FeedbackNotes != null)
			{
				JSONArray feedbacks = new JSONArray();
				foreach (EncryptedData encryptedData in purchase.FeedbackNotes)
				{
					feedbacks.add(encryptedData(encryptedData));
				}
				json.put("feedbackNotes", feedbacks);
			}
			if(purchase.PublicFeedback != null)
			{
				JSONArray publicFeedbacks = new JSONArray();
				foreach (string publicFeedback in purchase.PublicFeedback)
				{
					publicFeedbacks.add(publicFeedback);
				}
				json.put("publicFeedbacks", publicFeedbacks);
			}
			if(purchase.RefundNote != null)
			{
				json.put("refundNote", encryptedData(purchase.RefundNote));
			}
			if(purchase.DiscountNQT > 0)
			{
				json.put("discountNQT", Convert.ToString(purchase.DiscountNQT));
			}
			if(purchase.RefundNQT > 0)
			{
				json.put("refundNQT", Convert.ToString(purchase.RefundNQT));
			}
			return json;
		}

		internal static JSONObject trade(Trade trade, bool includeAssetInfo)
		{
			JSONObject json = new JSONObject();
			json.put("timestamp", trade.Timestamp);
			json.put("quantityQNT", Convert.ToString(trade.QuantityQNT));
			json.put("priceNQT", Convert.ToString(trade.PriceNQT));
			json.put("asset", Convert.toUnsignedLong(trade.AssetId));
			json.put("askOrder", Convert.toUnsignedLong(trade.AskOrderId));
			json.put("bidOrder", Convert.toUnsignedLong(trade.BidOrderId));
			json.put("askOrderHeight", trade.AskOrderHeight);
			json.put("bidOrderHeight", trade.BidOrderHeight);
			putAccount(json, "seller", trade.SellerId);
			putAccount(json, "buyer", trade.BuyerId);
			json.put("block", Convert.toUnsignedLong(trade.BlockId));
			json.put("height", trade.Height);
			json.put("tradeType", trade.Buy ? "buy" : "sell");
			if(includeAssetInfo)
			{
				Asset asset = Asset.getAsset(trade.AssetId);
				json.put("name", asset.Name);
				json.put("decimals", asset.Decimals);
			}
			return json;
		}

		internal static JSONObject assetTransfer(AssetTransfer assetTransfer, bool includeAssetInfo)
		{
			JSONObject json = new JSONObject();
			json.put("assetTransfer", Convert.toUnsignedLong(assetTransfer.Id));
			json.put("asset", Convert.toUnsignedLong(assetTransfer.AssetId));
			putAccount(json, "sender", assetTransfer.SenderId);
			putAccount(json, "recipient", assetTransfer.RecipientId);
			json.put("quantityQNT", Convert.ToString(assetTransfer.QuantityQNT));
			json.put("height", assetTransfer.Height);
			json.put("timestamp", assetTransfer.Timestamp);
			if(includeAssetInfo)
			{
				Asset asset = Asset.getAsset(assetTransfer.AssetId);
				json.put("name", asset.Name);
				json.put("decimals", asset.Decimals);
			}
			return json;
		}

		internal static JSONObject unconfirmedTransaction(Transaction transaction)
		{
			JSONObject json = new JSONObject();
			json.put("type", transaction.Type.Type);
			json.put("subtype", transaction.Type.Subtype);
			json.put("timestamp", transaction.Timestamp);
			json.put("deadline", transaction.Deadline);
			json.put("senderPublicKey", Convert.toHexString(transaction.SenderPublicKey));
			if(transaction.RecipientId != 0)
			{
				putAccount(json, "recipient", transaction.RecipientId);
			}
			json.put("amountNQT", Convert.ToString(transaction.AmountNQT));
			json.put("feeNQT", Convert.ToString(transaction.FeeNQT));
			if(transaction.ReferencedTransactionFullHash != null)
			{
				json.put("referencedTransactionFullHash", transaction.ReferencedTransactionFullHash);
			}
			sbyte[] signature = Convert.emptyToNull(transaction.Signature);
			if(signature != null)
			{
				json.put("signature", Convert.toHexString(signature));
				json.put("signatureHash", Convert.toHexString(Crypto.sha256().digest(signature)));
				json.put("fullHash", transaction.FullHash);
				json.put("transaction", transaction.StringId);
			}
			JSONObject attachmentJSON = new JSONObject();
			foreach (Appendix appendage in transaction.Appendages)
			{
				attachmentJSON.putAll(appendage.JSONObject);
			}
			if(! attachmentJSON.Empty)
			{
				modifyAttachmentJSON(attachmentJSON);
				json.put("attachment", attachmentJSON);
			}
			putAccount(json, "sender", transaction.SenderId);
			json.put("height", transaction.Height);
			json.put("version", transaction.Version);
			if(transaction.Version > 0)
			{
				json.put("ecBlockId", Convert.toUnsignedLong(transaction.ECBlockId));
				json.put("ecBlockHeight", transaction.ECBlockHeight);
			}

			return json;
		}

		internal static JSONObject transaction(Transaction transaction)
		{
			JSONObject json = unconfirmedTransaction(transaction);
			json.put("block", Convert.toUnsignedLong(transaction.BlockId));
			json.put("confirmations", Nxt.Blockchain.Height - transaction.Height);
			json.put("blockTimestamp", transaction.BlockTimestamp);
			return json;
		}

	// ugly, hopefully temporary
		private static void modifyAttachmentJSON(JSONObject json)
		{
			long? quantityQNT = (long?) json.remove("quantityQNT");
			if(quantityQNT != null)
			{
				json.put("quantityQNT", Convert.ToString(quantityQNT));
			}
			long? priceNQT = (long?) json.remove("priceNQT");
			if(priceNQT != null)
			{
				json.put("priceNQT", Convert.ToString(priceNQT));
			}
			long? discountNQT = (long?) json.remove("discountNQT");
			if(discountNQT != null)
			{
				json.put("discountNQT", Convert.ToString(discountNQT));
			}
			long? refundNQT = (long?) json.remove("refundNQT");
			if(refundNQT != null)
			{
				json.put("refundNQT", Convert.ToString(refundNQT));
			}
		}

		internal static void putAccount(JSONObject json, string name, long accountId)
		{
			json.put(name, Convert.toUnsignedLong(accountId));
			json.put(name + "RS", Convert.rsAccount(accountId));
		}

		private JSONData() // never
		{
		}

	}

}