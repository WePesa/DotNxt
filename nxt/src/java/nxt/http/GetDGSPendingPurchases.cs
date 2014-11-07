namespace nxt.http
{

	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SELLER;

	public sealed class GetDGSPendingPurchases : APIServlet.APIRequestHandler
	{

		internal static readonly GetDGSPendingPurchases instance = new GetDGSPendingPurchases();

		private GetDGSPendingPurchases() : base(new APITag[] {APITag.DGS}, "seller", "firstIndex", "lastIndex")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			long sellerId = ParameterParser.getSellerId(req);
			if(sellerId == 0)
			{
				return MISSING_SELLER;
			}
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONObject response = new JSONObject();
			JSONArray purchasesJSON = new JSONArray();

			using (DbIterator<DigitalGoodsStore.Purchase> purchases = DigitalGoodsStore.getPendingSellerPurchases(sellerId, firstIndex, lastIndex))
			{
				while(purchases.hasNext())
				{
					purchasesJSON.add(JSONData.purchase(purchases.next()));
				}
			}

			response.put("purchases", purchasesJSON);
			return response;
		}

	}

}