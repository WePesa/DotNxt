using System;

namespace nxt.http
{

	using Asset = nxt.Asset;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ASSET;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_ASSET;

	public sealed class GetAssets : APIServlet.APIRequestHandler
	{

		internal static readonly GetAssets instance = new GetAssets();

		private GetAssets() : base(new APITag[] {APITag.AE}, "assets", "assets", "assets") // limit to 3 for testing
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string[] assets = req.getParameterValues("assets");

			JSONObject response = new JSONObject();
			JSONArray assetsJSONArray = new JSONArray();
			response.put("assets", assetsJSONArray);
			foreach (string assetIdString in assets)
			{
				if(assetIdString == null || assetIdString.Equals(""))
				{
					continue;
				}
				try
				{
					Asset asset = Asset.getAsset(Convert.parseUnsignedLong(assetIdString));
					if(asset == null)
					{
						return UNKNOWN_ASSET;
					}
					assetsJSONArray.add(JSONData.asset(asset));
				}
				catch(Exception e)
				{
					return INCORRECT_ASSET;
				}
			}
			return response;
		}

	}

}