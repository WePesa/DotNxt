namespace nxt.http
{

	using Asset = nxt.Asset;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAllAssets : APIServlet.APIRequestHandler
	{

		internal static readonly GetAllAssets instance = new GetAllAssets();

		private GetAllAssets() : base(new APITag[] {APITag.AE}, "firstIndex", "lastIndex")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONObject response = new JSONObject();
			JSONArray assetsJSONArray = new JSONArray();
			response.put("assets", assetsJSONArray);
			using (DbIterator<Asset> assets = Asset.getAllAssets(firstIndex, lastIndex))
			{
				while(assets.hasNext())
				{
					assetsJSONArray.add(JSONData.asset(assets.next()));
				}
			}
			return response;
		}

	}

}