using System;

namespace nxt.http
{

	using Order = nxt.Order;
	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAccountCurrentBidOrders : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountCurrentBidOrders instance = new GetAccountCurrentBidOrders();

		private GetAccountCurrentBidOrders() : base(new APITag[] {APITag.ACCOUNTS, APITag.AE}, "account", "asset", "firstIndex", "lastIndex")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws ParameterException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			long accountId = ParameterParser.getAccount(req).Id;
			long assetId = 0;
			try
			{
				assetId = Convert.parseUnsignedLong(req.getParameter("asset"));
			}
			catch(Exception e)
			{
			// ignore
			}
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			DbIterator<Order.Bid> bidOrders;
			if(assetId == 0)
			{
				bidOrders = Order.Bid.getBidOrdersByAccount(accountId, firstIndex, lastIndex);
			}
			else
			{
				bidOrders = Order.Bid.getBidOrdersByAccountAsset(accountId, assetId, firstIndex, lastIndex);
			}
			JSONArray orders = new JSONArray();
			try
			{
				while(bidOrders.hasNext())
				{
					orders.add(JSONData.bidOrder(bidOrders.next()));
				}
			}
			finally
			{
				bidOrders.close();
			}
			JSONObject response = new JSONObject();
			response.put("bidOrders", orders);
			return response;
		}

	}
}