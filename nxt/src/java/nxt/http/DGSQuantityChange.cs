using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DELTA_QUANTITY;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_DELTA_QUANTITY;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_GOODS;

	public sealed class DGSQuantityChange : CreateTransaction
	{

		internal static readonly DGSQuantityChange instance = new DGSQuantityChange();

		private DGSQuantityChange() : base(new APITag[] {APITag.DGS, APITag.CREATE_TRANSACTION}, "goods", "deltaQuantity")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account account = ParameterParser.getSenderAccount(req);
			DigitalGoodsStore.Goods goods = ParameterParser.getGoods(req);
			if(goods.Delisted || goods.SellerId != account.Id)
			{
				return UNKNOWN_GOODS;
			}

			int deltaQuantity;
			try
			{
				string deltaQuantityString = Convert.emptyToNull(req.getParameter("deltaQuantity"));
				if(deltaQuantityString == null)
				{
					return MISSING_DELTA_QUANTITY;
				}
				deltaQuantity = Convert.ToInt32(deltaQuantityString);
				if(deltaQuantity > Constants.MAX_DGS_LISTING_QUANTITY || deltaQuantity < -Constants.MAX_DGS_LISTING_QUANTITY)
				{
					return INCORRECT_DELTA_QUANTITY;
				}
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_DELTA_QUANTITY;
			}

			Attachment attachment = new Attachment.DigitalGoodsQuantityChange(goods.Id, deltaQuantity);
			return createTransaction(req, account, attachment);

		}

	}

}