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

	public sealed class CancelAskOrder : CreateTransaction
	{

		internal static readonly CancelAskOrder instance = new CancelAskOrder();

		private CancelAskOrder() : base(new APITag[] {APITag.AE, APITag.CREATE_TRANSACTION}, "order")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			long orderId = ParameterParser.getOrderId(req);
			Account account = ParameterParser.getSenderAccount(req);
			Order.Ask orderData = Order.Ask.getAskOrder(orderId);
			if(orderData == null || orderData.AccountId != account.Id)
			{
				return UNKNOWN_ORDER;
			}
			Attachment attachment = new Attachment.ColoredCoinsAskOrderCancellation(orderId);
			return createTransaction(req, account, attachment);
		}

	}

}