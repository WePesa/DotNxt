using System.Collections.Generic;

namespace nxt.http
{

	using Account = nxt.Account;
	using Asset = nxt.Asset;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAssetsByIssuer : APIServlet.APIRequestHandler
	{

		internal static readonly GetAssetsByIssuer instance = new GetAssetsByIssuer();

		private GetAssetsByIssuer() : base(new APITag[] {APITag.AE, APITag.ACCOUNTS}, "account", "account", "account", "firstIndex", "lastIndex")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws ParameterException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			IList<Account> accounts = ParameterParser.getAccounts(req);
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONObject response = new JSONObject();
			JSONArray accountsJSONArray = new JSONArray();
			response.put("assets", accountsJSONArray);
			foreach (Account account in accounts)
			{
				JSONArray assetsJSONArray = new JSONArray();
				using (DbIterator<Asset> assets = Asset.getAssetsIssuedBy(account.Id, firstIndex, lastIndex))
				{
					while(assets.hasNext())
					{
						assetsJSONArray.add(JSONData.asset(assets.next()));
					}
				}
				accountsJSONArray.add(assetsJSONArray);
			}
			return response;
		}

	}

}