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

	public sealed class GetAccountCurrentBidOrderIds : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountCurrentBidOrderIds instance = new GetAccountCurrentBidOrderIds();

		private GetAccountCurrentBidOrderIds() : base(new APITag[] {APITag.ACCOUNTS, APITag.AE}, "account", "asset", "firstIndex", "lastIndex")
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
			JSONArray orderIds = new JSONArray();
			try
			{
				while(bidOrders.hasNext())
				{
					orderIds.add(Convert.toUnsignedLong(bidOrders.next().Id));
				}
			}
			finally
			{
				bidOrders.close();
			}
			JSONObject response = new JSONObject();
			response.put("bidOrderIds", orderIds);
			return response;
		}

	}
}