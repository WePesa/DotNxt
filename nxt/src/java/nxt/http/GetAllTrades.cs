namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using Trade = nxt.Trade;
	using FilteringIterator = nxt.db.FilteringIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAllTrades : APIServlet.APIRequestHandler
	{

		internal static readonly GetAllTrades instance = new GetAllTrades();

		private GetAllTrades() : base(new APITag[] {APITag.AE}, "timestamp", "firstIndex", "lastIndex", "includeAssetInfo")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int timestamp = ParameterParser.getTimestamp(req);
			int timestamp = ParameterParser.getTimestamp(req);
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);
			bool includeAssetInfo = !"false".equalsIgnoreCase(req.getParameter("includeAssetInfo"));

			JSONObject response = new JSONObject();
			JSONArray trades = new JSONArray();
			using (FilteringIterator<Trade> tradeIterator = new FilteringIterator<>(Trade.getAllTrades(0, -1), new FilteringIterator.Filter<Trade>() { public bool ok(Trade trade) { return trade.Timestamp >= timestamp; } }, firstIndex, lastIndex))
			{
				while(tradeIterator.hasNext())
				{
					trades.add(JSONData.trade(tradeIterator.next(), includeAssetInfo));
				}
			}
			response.put("trades", trades);
			return response;
		}

	}

}