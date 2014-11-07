using System;

namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class ClearUnconfirmedTransactions : APIServlet.APIRequestHandler
	{

		internal static readonly ClearUnconfirmedTransactions instance = new ClearUnconfirmedTransactions();

		private ClearUnconfirmedTransactions() : base(new APITag[] {APITag.DEBUG})
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			JSONObject response = new JSONObject();
			try
			{
				Nxt.TransactionProcessor.clearUnconfirmedTransactions();
				response.put("done", true);
			}
			catch(Exception e)
			{
				response.put("error", e.ToString());
			}
			return response;
		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}