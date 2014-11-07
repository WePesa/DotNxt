namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using Order = nxt.Order;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAskOrders : APIServlet.APIRequestHandler
	{

		internal static readonly GetAskOrders instance = new GetAskOrders();

		private GetAskOrders() : base(new APITag[] {APITag.AE}, "asset", "firstIndex", "lastIndex")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			long assetId = ParameterParser.getAsset(req).Id;
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONArray orders = new JSONArray();
			using (DbIterator<Order.Ask> askOrders = Order.Ask.getSortedOrders(assetId, firstIndex, lastIndex))
			{
				while(askOrders.hasNext())
				{
					orders.add(JSONData.askOrder(askOrders.next()));
				}
			}

			JSONObject response = new JSONObject();
			response.put("askOrders", orders);
			return response;

		}

	}

}