namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using Order = nxt.Order;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_ORDER;

	public sealed class GetAskOrder : APIServlet.APIRequestHandler
	{

		internal static readonly GetAskOrder instance = new GetAskOrder();

		private GetAskOrder() : base(new APITag[] {APITag.AE}, "order")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			long orderId = ParameterParser.getOrderId(req);
			Order.Ask askOrder = Order.Ask.getAskOrder(orderId);
			if(askOrder == null)
			{
				return UNKNOWN_ORDER;
			}
			return JSONData.askOrder(askOrder);
		}

	}

}