namespace nxt.http
{

	using Account = nxt.Account;
	using Asset = nxt.Asset;
	using AssetTransfer = nxt.AssetTransfer;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using DbUtils = nxt.db.DbUtils;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAssetTransfers : APIServlet.APIRequestHandler
	{

		internal static readonly GetAssetTransfers instance = new GetAssetTransfers();

		private GetAssetTransfers() : base(new APITag[] {APITag.AE}, "asset", "account", "firstIndex", "lastIndex", "includeAssetInfo")
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
			JSONArray transfersData = new JSONArray();
			DbIterator<AssetTransfer> transfers = null;
			try
			{
				if(accountId == null)
				{
					Asset asset = ParameterParser.getAsset(req);
					transfers = asset.getAssetTransfers(firstIndex, lastIndex);
				}
				else if(assetId == null)
				{
					Account account = ParameterParser.getAccount(req);
					transfers = account.getAssetTransfers(firstIndex, lastIndex);
				}
				else
				{
					Asset asset = ParameterParser.getAsset(req);
					Account account = ParameterParser.getAccount(req);
					transfers = AssetTransfer.getAccountAssetTransfers(account.Id, asset.Id, firstIndex, lastIndex);
				}
				while(transfers.hasNext())
				{
					transfersData.add(JSONData.assetTransfer(transfers.next(), includeAssetInfo));
				}
			}
			finally
			{
				DbUtils.close(transfers);
			}
			response.put("transfers", transfersData);

			return response;
		}

		internal override bool startDbTransaction()
		{
			return true;
		}
	}

}