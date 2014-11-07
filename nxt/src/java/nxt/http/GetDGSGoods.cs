namespace nxt.http
{

	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using DbUtils = nxt.db.DbUtils;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetDGSGoods : APIServlet.APIRequestHandler
	{

		internal static readonly GetDGSGoods instance = new GetDGSGoods();

		private GetDGSGoods() : base(new APITag[] {APITag.DGS}, "seller", "firstIndex", "lastIndex", "inStockOnly")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			long sellerId = ParameterParser.getSellerId(req);
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);
			bool inStockOnly = !"false".equalsIgnoreCase(req.getParameter("inStockOnly"));

			JSONObject response = new JSONObject();
			JSONArray goodsJSON = new JSONArray();
			response.put("goods", goodsJSON);

			DbIterator<DigitalGoodsStore.Goods> goods = null;
			try
			{
				if(sellerId == 0)
				{
					if(inStockOnly)
					{
						goods = DigitalGoodsStore.getGoodsInStock(firstIndex, lastIndex);
					}
					else
					{
						goods = DigitalGoodsStore.getAllGoods(firstIndex, lastIndex);
					}
				}
				else
				{
					goods = DigitalGoodsStore.getSellerGoods(sellerId, inStockOnly, firstIndex, lastIndex);
				}
				while(goods.hasNext())
				{
					DigitalGoodsStore.Goods good = goods.next();
					goodsJSON.add(JSONData.goods(good));
				}
			}
			finally
			{
				DbUtils.close(goods);
			}

			return response;
		}

	}

}