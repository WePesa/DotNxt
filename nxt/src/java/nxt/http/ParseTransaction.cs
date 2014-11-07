using System;

namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using Transaction = nxt.Transaction;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class ParseTransaction : APIServlet.APIRequestHandler
	{

		internal static readonly ParseTransaction instance = new ParseTransaction();

		private ParseTransaction() : base(new APITag[] {APITag.TRANSACTIONS}, "transactionBytes", "transactionJSON")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string transactionBytes = Convert.emptyToNull(req.getParameter("transactionBytes"));
			string transactionJSON = Convert.emptyToNull(req.getParameter("transactionJSON"));
			Transaction transaction = ParameterParser.parseTransaction(transactionBytes, transactionJSON);
			JSONObject response = JSONData.unconfirmedTransaction(transaction);
			try
			{
				transaction.validate();
			}
			catch(NxtException.ValidationException|Exception e)
			{
				Logger.logDebugMessage(e.Message, e);
				response.put("validate", false);
				response.put("errorCode", 4);
				response.put("errorDescription", "Invalid transaction: " + e.ToString());
				response.put("error", e.Message);
			}
			response.put("verify", transaction.verifySignature());
			return response;
		}

	}

}