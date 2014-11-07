namespace nxt.http
{

	using Order = nxt.Order;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAllOpenAskOrders : APIServlet.APIRequestHandler
	{

		internal static readonly GetAllOpenAskOrders instance = new GetAllOpenAskOrders();

		private GetAllOpenAskOrders() : base(new APITag[] {APITag.AE}, "firstIndex", "lastIndex")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			JSONObject response = new JSONObject();
			JSONArray ordersData = new JSONArray();

			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			using (DbIterator<Order.Ask> askOrders = Order.Ask.getAll(firstIndex, lastIndex))
			{
				while(askOrders.hasNext())
				{
					ordersData.add(JSONData.askOrder(askOrders.next()));
				}
			}

			response.put("openOrders", ordersData);
			return response;
		}

	}

}