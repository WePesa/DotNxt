using System;

namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Transaction = nxt.Transaction;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class BroadcastTransaction : APIServlet.APIRequestHandler
	{

		internal static readonly BroadcastTransaction instance = new BroadcastTransaction();

		private BroadcastTransaction() : base(new APITag[] {APITag.TRANSACTIONS}, "transactionBytes", "transactionJSON")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string transactionBytes = Convert.emptyToNull(req.getParameter("transactionBytes"));
			string transactionJSON = Convert.emptyToNull(req.getParameter("transactionJSON"));
			Transaction transaction = ParameterParser.parseTransaction(transactionBytes, transactionJSON);
			JSONObject response = new JSONObject();
			try
			{
				transaction.validate();
				Nxt.TransactionProcessor.broadcast(transaction);
				response.put("transaction", transaction.StringId);
				response.put("fullHash", transaction.FullHash);
			}
			catch(NxtException.ValidationException|Exception e)
			{
				Logger.logDebugMessage(e.Message, e);
				response.put("errorCode", 4);
				response.put("errorDescription", "Incorrect transaction: " + e.ToString());
				response.put("error", e.Message);
			}
			return response;

		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}