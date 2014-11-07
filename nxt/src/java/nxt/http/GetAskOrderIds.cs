namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using Order = nxt.Order;
	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAskOrderIds : APIServlet.APIRequestHandler
	{

		internal static readonly GetAskOrderIds instance = new GetAskOrderIds();

		private GetAskOrderIds() : base(new APITag[] {APITag.AE}, "asset", "firstIndex", "lastIndex")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			long assetId = ParameterParser.getAsset(req).Id;
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONArray orderIds = new JSONArray();
			using (DbIterator<Order.Ask> askOrders = Order.Ask.getSortedOrders(assetId, firstIndex, lastIndex))
			{
				while(askOrders.hasNext())
				{
					orderIds.add(Convert.toUnsignedLong(askOrders.next().Id));
				}
			}

			JSONObject response = new JSONObject();
			response.put("askOrderIds", orderIds);
			return response;

		}

	}

}