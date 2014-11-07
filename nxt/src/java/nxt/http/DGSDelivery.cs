using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using NxtException = nxt.NxtException;
	using EncryptedData = nxt.crypto.EncryptedData;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.ALREADY_DELIVERED;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DGS_DISCOUNT;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DGS_GOODS;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_PURCHASE;

	public sealed class DGSDelivery : CreateTransaction
	{

		internal static readonly DGSDelivery instance = new DGSDelivery();

		private DGSDelivery() : base(new APITag[] {APITag.DGS, APITag.CREATE_TRANSACTION}, "purchase", "discountNQT", "goodsToEncrypt", "goodsIsText", "goodsData", "goodsNonce")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account sellerAccount = ParameterParser.getSenderAccount(req);
			DigitalGoodsStore.Purchase purchase = ParameterParser.getPurchase(req);
			if(sellerAccount.Id != purchase.SellerId)
			{
				return INCORRECT_PURCHASE;
			}
			if(! purchase.Pending)
			{
				return ALREADY_DELIVERED;
			}

			string discountValueNQT = Convert.emptyToNull(req.getParameter("discountNQT"));
			long discountNQT = 0;
			try
			{
				if(discountValueNQT != null)
				{
					discountNQT = Convert.ToInt64(discountValueNQT);
				}
			}
			catch(Exception e)
			{
				return INCORRECT_DGS_DISCOUNT;
			}
			if(discountNQT < 0 || discountNQT > Constants.MAX_BALANCE_NQT || discountNQT > Convert.safeMultiply(purchase.PriceNQT, purchase.Quantity))
			{
				return INCORRECT_DGS_DISCOUNT;
			}

			Account buyerAccount = Account.getAccount(purchase.BuyerId);
			bool goodsIsText = !"false".equalsIgnoreCase(req.getParameter("goodsIsText"));
			EncryptedData encryptedGoods = ParameterParser.getEncryptedGoods(req);

			if(encryptedGoods == null)
			{
				string secretPhrase = ParameterParser.getSecretPhrase(req);
				sbyte[] goodsBytes;
				try
				{
					string plainGoods = Convert.nullToEmpty(req.getParameter("goodsToEncrypt"));
					if(plainGoods.Length == 0)
					{
						return INCORRECT_DGS_GOODS;
					}
					goodsBytes = goodsIsText ? Convert.toBytes(plainGoods) : Convert.parseHexString(plainGoods);
				}
				catch(Exception e)
				{
					return INCORRECT_DGS_GOODS;
				}
				encryptedGoods = buyerAccount.encryptTo(goodsBytes, secretPhrase);
			}

			Attachment attachment = new Attachment.DigitalGoodsDelivery(purchase.Id, encryptedGoods, goodsIsText, discountNQT);
			return createTransaction(req, sellerAccount, buyerAccount.Id, 0, attachment);

		}

	}

}