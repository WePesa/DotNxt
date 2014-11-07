namespace nxt.http
{

	using Account = nxt.Account;
	using Asset = nxt.Asset;
	using NxtException = nxt.NxtException;
	using Trade = nxt.Trade;
	using DbIterator = nxt.db.DbIterator;
	using DbUtils = nxt.db.DbUtils;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetTrades : APIServlet.APIRequestHandler
	{

		internal static readonly GetTrades instance = new GetTrades();

		private GetTrades() : base(new APITag[] {APITag.AE}, "asset", "account", "firstIndex", "lastIndex", "includeAssetInfo")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string assetId = Convert.emptyToNull(req.getParameter("asset"));
			string accountId = Convert.emptyToNull(req.getParameter("account"));

			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);
			bool includeAssetInfo = !"false".equalsIgnoreCase(req.getParameter("includeAssetInfo"));

			JSONObject response = new JSONObject();
			JSONArray tradesData = new JSONArray();
			DbIterator<Trade> trades = null;
			try
			{
				if(accountId == null)
				{
					Asset asset = ParameterParser.getAsset(req);
					trades = asset.getTrades(firstIndex, lastIndex);
				}
				else if(assetId == null)
				{
					Account account = ParameterParser.getAccount(req);
					trades = account.getTrades(firstIndex, lastIndex);
				}
				else
				{
					Asset asset = ParameterParser.getAsset(req);
					Account account = ParameterParser.getAccount(req);
					trades = Trade.getAccountAssetTrades(account.Id, asset.Id, firstIndex, lastIndex);
				}
				while(trades.hasNext())
				{
					tradesData.add(JSONData.trade(trades.next(), includeAssetInfo));
				}
			}
			finally
			{
				DbUtils.close(trades);
			}
			response.put("trades", tradesData);

			return response;
		}

		internal override bool startDbTransaction()
		{
			return true;
		}

	}

}