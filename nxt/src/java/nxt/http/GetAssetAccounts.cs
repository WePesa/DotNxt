namespace nxt.http
{

	using Account = nxt.Account;
	using Asset = nxt.Asset;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAssetAccounts : APIServlet.APIRequestHandler
	{

		internal static readonly GetAssetAccounts instance = new GetAssetAccounts();

		private GetAssetAccounts() : base(new APITag[] {APITag.AE}, "asset", "height", "firstIndex", "lastIndex")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Asset asset = ParameterParser.getAsset(req);
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);
			int height = ParameterParser.getHeight(req);

			JSONArray accountAssets = new JSONArray();
			using (DbIterator<Account.AccountAsset> iterator = asset.getAccounts(height, firstIndex, lastIndex))
			{
				while(iterator.hasNext())
				{
					Account.AccountAsset accountAsset = iterator.next();
					accountAssets.add(JSONData.accountAsset(accountAsset));
				}
			}

			JSONObject response = new JSONObject();
			response.put("accountAssets", accountAssets);
			return response;

		}

	}

}