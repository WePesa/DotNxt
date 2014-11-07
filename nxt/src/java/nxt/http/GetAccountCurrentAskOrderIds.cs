using System;

namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using Order = nxt.Order;
	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAccountCurrentAskOrderIds : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountCurrentAskOrderIds instance = new GetAccountCurrentAskOrderIds();

		private GetAccountCurrentAskOrderIds() : base(new APITag[] {APITag.ACCOUNTS, APITag.AE}, "account", "asset", "firstIndex", "lastIndex")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
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

			DbIterator<Order.Ask> askOrders;
			if(assetId == 0)
			{
				askOrders = Order.Ask.getAskOrdersByAccount(accountId, firstIndex, lastIndex);
			}
			else
			{
				askOrders = Order.Ask.getAskOrdersByAccountAsset(accountId, assetId, firstIndex, lastIndex);
			}
			JSONArray orderIds = new JSONArray();
			try
			{
				while(askOrders.hasNext())
				{
					orderIds.add(Convert.toUnsignedLong(askOrders.next().Id));
				}
			}
			finally
			{
				askOrders.close();
			}
			JSONObject response = new JSONObject();
			response.put("askOrderIds", orderIds);
			return response;
		}

	}

}