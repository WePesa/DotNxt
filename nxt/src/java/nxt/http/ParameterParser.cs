using System;
using System.Collections.Generic;

namespace nxt.http
{

	using Account = nxt.Account;
	using Alias = nxt.Alias;
	using Asset = nxt.Asset;
	using Constants = nxt.Constants;
	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Transaction = nxt.Transaction;
	using Crypto = nxt.crypto.Crypto;
	using EncryptedData = nxt.crypto.EncryptedData;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONValue = org.json.simple.JSONValue;
	using ParseException = org.json.simple.parser.ParseException;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.*;

	internal sealed class ParameterParser
	{

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Alias getAlias(HttpServletRequest req) throws ParameterException
		internal static Alias getAlias(HttpServletRequest req)
		{
			long aliasId;
			try
			{
				aliasId = Convert.parseUnsignedLong(Convert.emptyToNull(req.getParameter("alias")));
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_ALIAS);
			}
			string aliasName = Convert.emptyToNull(req.getParameter("aliasName"));
			Alias alias;
			if(aliasId != 0)
			{
				alias = Alias.getAlias(aliasId);
			}
			else if(aliasName != null)
			{
				alias = Alias.getAlias(aliasName);
			}
			else
			{
				throw new ParameterException(MISSING_ALIAS_OR_ALIAS_NAME);
			}
			if(alias == null)
			{
				throw new ParameterException(UNKNOWN_ALIAS);
			}
			return alias;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static long getAmountNQT(HttpServletRequest req) throws ParameterException
		internal static long getAmountNQT(HttpServletRequest req)
		{
			string amountValueNQT = Convert.emptyToNull(req.getParameter("amountNQT"));
			if(amountValueNQT == null)
			{
				throw new ParameterException(MISSING_AMOUNT);
			}
			long amountNQT;
			try
			{
				amountNQT = Convert.ToInt64(amountValueNQT);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_AMOUNT);
			}
			if(amountNQT <= 0 || amountNQT >= Constants.MAX_BALANCE_NQT)
			{
				throw new ParameterException(INCORRECT_AMOUNT);
			}
			return amountNQT;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static long getFeeNQT(HttpServletRequest req) throws ParameterException
		internal static long getFeeNQT(HttpServletRequest req)
		{
			string feeValueNQT = Convert.emptyToNull(req.getParameter("feeNQT"));
			if(feeValueNQT == null)
			{
				throw new ParameterException(MISSING_FEE);
			}
			long feeNQT;
			try
			{
				feeNQT = Convert.ToInt64(feeValueNQT);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_FEE);
			}
			if(feeNQT < 0 || feeNQT >= Constants.MAX_BALANCE_NQT)
			{
				throw new ParameterException(INCORRECT_FEE);
			}
			return feeNQT;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static long getPriceNQT(HttpServletRequest req) throws ParameterException
		internal static long getPriceNQT(HttpServletRequest req)
		{
			string priceValueNQT = Convert.emptyToNull(req.getParameter("priceNQT"));
			if(priceValueNQT == null)
			{
				throw new ParameterException(MISSING_PRICE);
			}
			long priceNQT;
			try
			{
				priceNQT = Convert.ToInt64(priceValueNQT);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_PRICE);
			}
			if(priceNQT <= 0 || priceNQT > Constants.MAX_BALANCE_NQT)
			{
				throw new ParameterException(INCORRECT_PRICE);
			}
			return priceNQT;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Asset getAsset(HttpServletRequest req) throws ParameterException
		internal static Asset getAsset(HttpServletRequest req)
		{
			string assetValue = Convert.emptyToNull(req.getParameter("asset"));
			if(assetValue == null)
			{
				throw new ParameterException(MISSING_ASSET);
			}
			Asset asset;
			try
			{
				long assetId = Convert.parseUnsignedLong(assetValue);
				asset = Asset.getAsset(assetId);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_ASSET);
			}
			if(asset == null)
			{
				throw new ParameterException(UNKNOWN_ASSET);
			}
			return asset;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static long getQuantityQNT(HttpServletRequest req) throws ParameterException
		internal static long getQuantityQNT(HttpServletRequest req)
		{
			string quantityValueQNT = Convert.emptyToNull(req.getParameter("quantityQNT"));
			if(quantityValueQNT == null)
			{
				throw new ParameterException(MISSING_QUANTITY);
			}
			long quantityQNT;
			try
			{
				quantityQNT = Convert.ToInt64(quantityValueQNT);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_QUANTITY);
			}
			if(quantityQNT <= 0 || quantityQNT > Constants.MAX_ASSET_QUANTITY_QNT)
			{
				throw new ParameterException(INCORRECT_ASSET_QUANTITY);
			}
			return quantityQNT;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static long getOrderId(HttpServletRequest req) throws ParameterException
		internal static long getOrderId(HttpServletRequest req)
		{
			string orderValue = Convert.emptyToNull(req.getParameter("order"));
			if(orderValue == null)
			{
				throw new ParameterException(MISSING_ORDER);
			}
			try
			{
				return Convert.parseUnsignedLong(orderValue);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_ORDER);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static DigitalGoodsStore.Goods getGoods(HttpServletRequest req) throws ParameterException
		internal static DigitalGoodsStore.Goods getGoods(HttpServletRequest req)
		{
			string goodsValue = Convert.emptyToNull(req.getParameter("goods"));
			if(goodsValue == null)
			{
				throw new ParameterException(MISSING_GOODS);
			}
			DigitalGoodsStore.Goods goods;
			try
			{
				long goodsId = Convert.parseUnsignedLong(goodsValue);
				goods = DigitalGoodsStore.getGoods(goodsId);
				if(goods == null)
				{
					throw new ParameterException(UNKNOWN_GOODS);
				}
				return goods;
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_GOODS);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static int getGoodsQuantity(HttpServletRequest req) throws ParameterException
		internal static int getGoodsQuantity(HttpServletRequest req)
		{
			string quantityString = Convert.emptyToNull(req.getParameter("quantity"));
			try
			{
				int quantity = Convert.ToInt32(quantityString);
				if(quantity < 0 || quantity > Constants.MAX_DGS_LISTING_QUANTITY)
				{
					throw new ParameterException(INCORRECT_QUANTITY);
				}
				return quantity;
			}
			catch(NumberFormatException e)
			{
				throw new ParameterException(INCORRECT_QUANTITY);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static EncryptedData getEncryptedMessage(HttpServletRequest req, Account recipientAccount) throws ParameterException
		internal static EncryptedData getEncryptedMessage(HttpServletRequest req, Account recipientAccount)
		{
			string data = Convert.emptyToNull(req.getParameter("encryptedMessageData"));
			string nonce = Convert.emptyToNull(req.getParameter("encryptedMessageNonce"));
			if(data != null && nonce != null)
			{
				try
				{
					return new EncryptedData(Convert.parseHexString(data), Convert.parseHexString(nonce));
				}
				catch(Exception e)
				{
					throw new ParameterException(INCORRECT_ENCRYPTED_MESSAGE);
				}
			}
			string plainMessage = Convert.emptyToNull(req.getParameter("messageToEncrypt"));
			if(plainMessage == null)
			{
				return null;
			}
			if(recipientAccount == null)
			{
				throw new ParameterException(INCORRECT_RECIPIENT);
			}
			string secretPhrase = getSecretPhrase(req);
			bool isText = !"false".equalsIgnoreCase(req.getParameter("messageToEncryptIsText"));
			try
			{
				sbyte[] plainMessageBytes = isText ? Convert.toBytes(plainMessage) : Convert.parseHexString(plainMessage);
				return recipientAccount.encryptTo(plainMessageBytes, secretPhrase);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_PLAIN_MESSAGE);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static EncryptedData getEncryptToSelfMessage(HttpServletRequest req) throws ParameterException
		internal static EncryptedData getEncryptToSelfMessage(HttpServletRequest req)
		{
			string data = Convert.emptyToNull(req.getParameter("encryptToSelfMessageData"));
			string nonce = Convert.emptyToNull(req.getParameter("encryptToSelfMessageNonce"));
			if(data != null && nonce != null)
			{
				try
				{
					return new EncryptedData(Convert.parseHexString(data), Convert.parseHexString(nonce));
				}
				catch(Exception e)
				{
					throw new ParameterException(INCORRECT_ENCRYPTED_MESSAGE);
				}
			}
			string plainMessage = Convert.emptyToNull(req.getParameter("messageToEncryptToSelf"));
			if(plainMessage == null)
			{
				return null;
			}
			string secretPhrase = getSecretPhrase(req);
			Account senderAccount = Account.getAccount(Crypto.getPublicKey(secretPhrase));
			bool isText = !"false".equalsIgnoreCase(req.getParameter("messageToEncryptToSelfIsText"));
			try
			{
				sbyte[] plainMessageBytes = isText ? Convert.toBytes(plainMessage) : Convert.parseHexString(plainMessage);
				return senderAccount.encryptTo(plainMessageBytes, secretPhrase);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_PLAIN_MESSAGE);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static EncryptedData getEncryptedGoods(HttpServletRequest req) throws ParameterException
		internal static EncryptedData getEncryptedGoods(HttpServletRequest req)
		{
			string data = Convert.emptyToNull(req.getParameter("goodsData"));
			string nonce = Convert.emptyToNull(req.getParameter("goodsNonce"));
			if(data != null && nonce != null)
			{
				try
				{
					return new EncryptedData(Convert.parseHexString(data), Convert.parseHexString(nonce));
				}
				catch(Exception e)
				{
					throw new ParameterException(INCORRECT_DGS_ENCRYPTED_GOODS);
				}
			}
			return null;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static DigitalGoodsStore.Purchase getPurchase(HttpServletRequest req) throws ParameterException
		internal static DigitalGoodsStore.Purchase getPurchase(HttpServletRequest req)
		{
			string purchaseIdString = Convert.emptyToNull(req.getParameter("purchase"));
			if(purchaseIdString == null)
			{
				throw new ParameterException(MISSING_PURCHASE);
			}
			try
			{
				DigitalGoodsStore.Purchase purchase = DigitalGoodsStore.getPurchase(Convert.parseUnsignedLong(purchaseIdString));
				if(purchase == null)
				{
					throw new ParameterException(INCORRECT_PURCHASE);
				}
				return purchase;
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_PURCHASE);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static String getSecretPhrase(HttpServletRequest req) throws ParameterException
		internal static string getSecretPhrase(HttpServletRequest req)
		{
			string secretPhrase = Convert.emptyToNull(req.getParameter("secretPhrase"));
			if(secretPhrase == null)
			{
				throw new ParameterException(MISSING_SECRET_PHRASE);
			}
			return secretPhrase;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Account getSenderAccount(HttpServletRequest req) throws ParameterException
		internal static Account getSenderAccount(HttpServletRequest req)
		{
			Account account;
			string secretPhrase = Convert.emptyToNull(req.getParameter("secretPhrase"));
			string publicKeyString = Convert.emptyToNull(req.getParameter("publicKey"));
			if(secretPhrase != null)
			{
				account = Account.getAccount(Crypto.getPublicKey(secretPhrase));
			}
			else if(publicKeyString != null)
			{
				try
				{
					account = Account.getAccount(Convert.parseHexString(publicKeyString));
				}
				catch(Exception e)
				{
					throw new ParameterException(INCORRECT_PUBLIC_KEY);
				}
			}
			else
			{
				throw new ParameterException(MISSING_SECRET_PHRASE_OR_PUBLIC_KEY);
			}
			if(account == null)
			{
				throw new ParameterException(UNKNOWN_ACCOUNT);
			}
			return account;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Account getAccount(HttpServletRequest req) throws ParameterException
		internal static Account getAccount(HttpServletRequest req)
		{
			string accountValue = Convert.emptyToNull(req.getParameter("account"));
			if(accountValue == null)
			{
				throw new ParameterException(MISSING_ACCOUNT);
			}
			try
			{
				Account account = Account.getAccount(Convert.parseAccountId(accountValue));
				if(account == null)
				{
					throw new ParameterException(UNKNOWN_ACCOUNT);
				}
				return account;
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_ACCOUNT);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static List<Account> getAccounts(HttpServletRequest req) throws ParameterException
		internal static IList<Account> getAccounts(HttpServletRequest req)
		{
			string[] accountValues = req.getParameterValues("account");
			if(accountValues == null || accountValues.Length == 0)
			{
				throw new ParameterException(MISSING_ACCOUNT);
			}
			IList<Account> result = new List<>();
			foreach (string accountValue in accountValues)
			{
				if(accountValue == null || accountValue.Equals(""))
				{
					continue;
				}
				try
				{
					Account account = Account.getAccount(Convert.parseAccountId(accountValue));
					if(account == null)
					{
						throw new ParameterException(UNKNOWN_ACCOUNT);
					}
					result.Add(account);
				}
				catch(Exception e)
				{
					throw new ParameterException(INCORRECT_ACCOUNT);
				}
			}
			return result;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static int getTimestamp(HttpServletRequest req) throws ParameterException
		internal static int getTimestamp(HttpServletRequest req)
		{
			string timestampValue = Convert.emptyToNull(req.getParameter("timestamp"));
			if(timestampValue == null)
			{
				return 0;
			}
			int timestamp;
			try
			{
				timestamp = Convert.ToInt32(timestampValue);
			}
			catch(NumberFormatException e)
			{
				throw new ParameterException(INCORRECT_TIMESTAMP);
			}
			if(timestamp < 0)
			{
				throw new ParameterException(INCORRECT_TIMESTAMP);
			}
			return timestamp;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static long getRecipientId(HttpServletRequest req) throws ParameterException
		internal static long getRecipientId(HttpServletRequest req)
		{
			string recipientValue = Convert.emptyToNull(req.getParameter("recipient"));
			if(recipientValue == null || "0".Equals(recipientValue))
			{
				throw new ParameterException(MISSING_RECIPIENT);
			}
			long recipientId;
			try
			{
				recipientId = Convert.parseAccountId(recipientValue);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_RECIPIENT);
			}
			if(recipientId == 0)
			{
				throw new ParameterException(INCORRECT_RECIPIENT);
			}
			return recipientId;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static long getSellerId(HttpServletRequest req) throws ParameterException
		internal static long getSellerId(HttpServletRequest req)
		{
			string sellerIdValue = Convert.emptyToNull(req.getParameter("seller"));
			try
			{
				return Convert.parseAccountId(sellerIdValue);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_RECIPIENT);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static long getBuyerId(HttpServletRequest req) throws ParameterException
		internal static long getBuyerId(HttpServletRequest req)
		{
			string buyerIdValue = Convert.emptyToNull(req.getParameter("buyer"));
			try
			{
				return Convert.parseAccountId(buyerIdValue);
			}
			catch(Exception e)
			{
				throw new ParameterException(INCORRECT_RECIPIENT);
			}
		}

		internal static int getFirstIndex(HttpServletRequest req)
		{
			int firstIndex;
			try
			{
				firstIndex = Convert.ToInt32(req.getParameter("firstIndex"));
				if(firstIndex < 0)
				{
					return 0;
				}
			}
			catch(NumberFormatException e)
			{
				return 0;
			}
			return firstIndex;
		}

		internal static int getLastIndex(HttpServletRequest req)
		{
			int lastIndex;
			try
			{
				lastIndex = Convert.ToInt32(req.getParameter("lastIndex"));
				if(lastIndex < 0)
				{
					return int.MaxValue;
				}
			}
			catch(NumberFormatException e)
			{
				return int.MaxValue;
			}
			return lastIndex;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static int getNumberOfConfirmations(HttpServletRequest req) throws ParameterException
		internal static int getNumberOfConfirmations(HttpServletRequest req)
		{
			string numberOfConfirmationsValue = Convert.emptyToNull(req.getParameter("numberOfConfirmations"));
			if(numberOfConfirmationsValue != null)
			{
				try
				{
					int numberOfConfirmations = Convert.ToInt32(numberOfConfirmationsValue);
					if(numberOfConfirmations <= Nxt.Blockchain.Height)
					{
						return numberOfConfirmations;
					}
					throw new ParameterException(INCORRECT_NUMBER_OF_CONFIRMATIONS);
				}
				catch(NumberFormatException e)
				{
					throw new ParameterException(INCORRECT_NUMBER_OF_CONFIRMATIONS);
				}
			}
			return 0;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static int getHeight(HttpServletRequest req) throws ParameterException
		internal static int getHeight(HttpServletRequest req)
		{
			string heightValue = Convert.emptyToNull(req.getParameter("height"));
			if(heightValue != null)
			{
				try
				{
					int height = Convert.ToInt32(heightValue);
					if(height < 0 || height > Nxt.Blockchain.Height)
					{
						throw new ParameterException(INCORRECT_HEIGHT);
					}
					if(height < Nxt.BlockchainProcessor.MinRollbackHeight)
					{
						throw new ParameterException(HEIGHT_NOT_AVAILABLE);
					}
					return height;
				}
				catch(NumberFormatException e)
				{
					throw new ParameterException(INCORRECT_HEIGHT);
				}
			}
			return -1;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static Transaction parseTransaction(String transactionBytes, String transactionJSON) throws ParameterException
		internal static Transaction parseTransaction(string transactionBytes, string transactionJSON)
		{
			if(transactionBytes == null && transactionJSON == null)
			{
				throw new ParameterException(MISSING_TRANSACTION_BYTES_OR_JSON);
			}
			if(transactionBytes != null)
			{
				try
				{
					sbyte[] bytes = Convert.parseHexString(transactionBytes);
					return Nxt.TransactionProcessor.parseTransaction(bytes);
				}
				catch(NxtException.ValidationException|Exception e)
				{
					Logger.logDebugMessage(e.Message, e);
					JSONObject response = new JSONObject();
					response.put("errorCode", 4);
					response.put("errorDescription", "Incorrect transactionBytes: " + e.ToString());
					throw new ParameterException(response);
				}
			}
			else
			{
				try
				{
					JSONObject json = (JSONObject) JSONValue.parseWithException(transactionJSON);
					return Nxt.TransactionProcessor.parseTransaction(json);
				}
				catch(NxtException.ValidationException | Exception | ParseException e)
				{
					Logger.logDebugMessage(e.Message, e);
					JSONObject response = new JSONObject();
					response.put("errorCode", 4);
					response.put("errorDescription", "Incorrect transactionJSON: " + e.ToString());
					throw new ParameterException(response);
				}
			}
		}


		private ParameterParser() // never
		{
		}

	}

}