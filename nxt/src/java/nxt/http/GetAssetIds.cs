namespace nxt.http
{

	using Asset = nxt.Asset;
	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAssetIds : APIServlet.APIRequestHandler
	{

		internal static readonly GetAssetIds instance = new GetAssetIds();

		private GetAssetIds() : base(new APITag[] {APITag.AE}, "firstIndex", "lastIndex")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONArray assetIds = new JSONArray();
			using (DbIterator<Asset> assets = Asset.getAllAssets(firstIndex, lastIndex))
			{
				while(assets.hasNext())
				{
					assetIds.add(Convert.toUnsignedLong(assets.next().Id));
				}
			}
			JSONObject response = new JSONObject();
			response.put("assetIds", assetIds);
			return response;
		}

	}

}