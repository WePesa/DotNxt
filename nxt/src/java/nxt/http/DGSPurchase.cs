using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DELIVERY_DEADLINE_TIMESTAMP;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_PURCHASE_PRICE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_PURCHASE_QUANTITY;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_DELIVERY_DEADLINE_TIMESTAMP;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_GOODS;

	public sealed class DGSPurchase : CreateTransaction
	{

		internal static readonly DGSPurchase instance = new DGSPurchase();

		private DGSPurchase() : base(new APITag[] {APITag.DGS, APITag.CREATE_TRANSACTION}, "goods", "priceNQT", "quantity", "deliveryDeadlineTimestamp")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			DigitalGoodsStore.Goods goods = ParameterParser.getGoods(req);
			if(goods.Delisted)
			{
				return UNKNOWN_GOODS;
			}

			int quantity = ParameterParser.getGoodsQuantity(req);
			if(quantity > goods.Quantity)
			{
				return INCORRECT_PURCHASE_QUANTITY;
			}

			long priceNQT = ParameterParser.getPriceNQT(req);
			if(priceNQT != goods.PriceNQT)
			{
				return INCORRECT_PURCHASE_PRICE;
			}

			string deliveryDeadlineString = Convert.emptyToNull(req.getParameter("deliveryDeadlineTimestamp"));
			if(deliveryDeadlineString == null)
			{
				return MISSING_DELIVERY_DEADLINE_TIMESTAMP;
			}
			int deliveryDeadline;
			try
			{
				deliveryDeadline = Convert.ToInt32(deliveryDeadlineString);
				if(deliveryDeadline <= Nxt.EpochTime)
				{
					return INCORRECT_DELIVERY_DEADLINE_TIMESTAMP;
				}
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_DELIVERY_DEADLINE_TIMESTAMP;
			}

			Account buyerAccount = ParameterParser.getSenderAccount(req);
			Account sellerAccount = Account.getAccount(goods.SellerId);

			Attachment attachment = new Attachment.DigitalGoodsPurchase(goods.Id, quantity, priceNQT, deliveryDeadline);
			return createTransaction(req, buyerAccount, sellerAccount.Id, 0, attachment);

		}

	}

}