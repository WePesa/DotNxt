namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using NxtException = nxt.NxtException;
	using Order = nxt.Order;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_ORDER;

	public sealed class CancelBidOrder : CreateTransaction
	{

		internal static readonly CancelBidOrder instance = new CancelBidOrder();

		private CancelBidOrder() : base(new APITag[] {APITag.AE, APITag.CREATE_TRANSACTION}, "order")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			long orderId = ParameterParser.getOrderId(req);
			Account account = ParameterParser.getSenderAccount(req);
			Order.Bid orderData = Order.Bid.getBidOrder(orderId);
			if(orderData == null || orderData.AccountId != account.Id)
			{
				return UNKNOWN_ORDER;
			}
			Attachment attachment = new Attachment.ColoredCoinsBidOrderCancellation(orderId);
			return createTransaction(req, account, attachment);
		}

	}

}