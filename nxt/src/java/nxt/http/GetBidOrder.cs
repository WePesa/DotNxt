namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using Order = nxt.Order;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_ORDER;

	public sealed class GetBidOrder : APIServlet.APIRequestHandler
	{

		internal static readonly GetBidOrder instance = new GetBidOrder();

		private GetBidOrder() : base(new APITag[] {APITag.AE}, "order")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			long orderId = ParameterParser.getOrderId(req);
			Order.Bid bidOrder = Order.Bid.getBidOrder(orderId);
			if(bidOrder == null)
			{
				return UNKNOWN_ORDER;
			}
			return JSONData.bidOrder(bidOrder);
		}

	}

}