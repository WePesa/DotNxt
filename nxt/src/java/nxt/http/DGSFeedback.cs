namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.GOODS_NOT_DELIVERED;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_PURCHASE;

	public sealed class DGSFeedback : CreateTransaction
	{

		internal static readonly DGSFeedback instance = new DGSFeedback();

		private DGSFeedback() : base(new APITag[] {APITag.DGS, APITag.CREATE_TRANSACTION}, "purchase")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			DigitalGoodsStore.Purchase purchase = ParameterParser.getPurchase(req);

			Account buyerAccount = ParameterParser.getSenderAccount(req);
			if(buyerAccount.Id != purchase.BuyerId)
			{
				return INCORRECT_PURCHASE;
			}
			if(purchase.EncryptedGoods == null)
			{
				return GOODS_NOT_DELIVERED;
			}

			Account sellerAccount = Account.getAccount(purchase.SellerId);
			Attachment attachment = new Attachment.DigitalGoodsFeedback(purchase.Id);
			return createTransaction(req, buyerAccount, sellerAccount.Id, 0, attachment);
		}

	}

}