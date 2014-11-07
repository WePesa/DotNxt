namespace nxt.http
{

	using Order = nxt.Order;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAllOpenBidOrders : APIServlet.APIRequestHandler
	{

		internal static readonly GetAllOpenBidOrders instance = new GetAllOpenBidOrders();

		private GetAllOpenBidOrders() : base(new APITag[] {APITag.AE}, "firstIndex", "lastIndex")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			JSONObject response = new JSONObject();
			JSONArray ordersData = new JSONArray();

			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			using (DbIterator<Order.Bid> bidOrders = Order.Bid.getAll(firstIndex, lastIndex))
			{
				while(bidOrders.hasNext())
				{
					ordersData.add(JSONData.bidOrder(bidOrders.next()));
				}
			}

			response.put("openOrders", ordersData);
			return response;
		}

	}

}