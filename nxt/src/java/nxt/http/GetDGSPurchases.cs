namespace nxt.http
{

	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using FilteringIterator = nxt.db.FilteringIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetDGSPurchases : APIServlet.APIRequestHandler
	{

		internal static readonly GetDGSPurchases instance = new GetDGSPurchases();

		private GetDGSPurchases() : base(new APITag[] {APITag.DGS}, "seller", "buyer", "firstIndex", "lastIndex", "completed")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			long sellerId = ParameterParser.getSellerId(req);
			long buyerId = ParameterParser.getBuyerId(req);
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean completed = "true".equalsIgnoreCase(req.getParameter("completed"));
			bool completed = "true".equalsIgnoreCase(req.getParameter("completed"));


			JSONObject response = new JSONObject();
			JSONArray purchasesJSON = new JSONArray();
			response.put("purchases", purchasesJSON);

			if(sellerId == 0 && buyerId == 0)
			{
				using (FilteringIterator<DigitalGoodsStore.Purchase> purchaseIterator = new FilteringIterator<>(DigitalGoodsStore.getAllPurchases(0, -1), new FilteringIterator.Filter<DigitalGoodsStore.Purchase>() { public bool ok(DigitalGoodsStore.Purchase purchase) { return ! (completed && purchase.Pending); } }, firstIndex, lastIndex))
				{
					while(purchaseIterator.hasNext())
					{
						purchasesJSON.add(JSONData.purchase(purchaseIterator.next()));
					}
				}
				return response;
			}

			DbIterator<DigitalGoodsStore.Purchase> purchases;
			if(sellerId != 0 && buyerId == 0)
			{
				purchases = DigitalGoodsStore.getSellerPurchases(sellerId, 0, -1);
			}
			else if(sellerId == 0)
			{
				purchases = DigitalGoodsStore.getBuyerPurchases(buyerId, 0, -1);
			}
			else
			{
				purchases = DigitalGoodsStore.getSellerBuyerPurchases(sellerId, buyerId, 0, -1);
			}
			using (FilteringIterator<DigitalGoodsStore.Purchase> purchaseIterator = new FilteringIterator<>(purchases, new FilteringIterator.Filter<DigitalGoodsStore.Purchase>() { public bool ok(DigitalGoodsStore.Purchase purchase) { return ! (completed && purchase.Pending); } }, firstIndex, lastIndex))
			{
				while(purchaseIterator.hasNext())
				{
					purchasesJSON.add(JSONData.purchase(purchaseIterator.next()));
				}
			}
			return response;
		}

	}

}