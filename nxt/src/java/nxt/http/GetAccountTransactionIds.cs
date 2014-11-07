using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Transaction = nxt.Transaction;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAccountTransactionIds : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountTransactionIds instance = new GetAccountTransactionIds();

		private GetAccountTransactionIds() : base(new APITag[] {APITag.ACCOUNTS}, "account", "timestamp", "type", "subtype", "firstIndex", "lastIndex", "numberOfConfirmations")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account account = ParameterParser.getAccount(req);
			int timestamp = ParameterParser.getTimestamp(req);
			int numberOfConfirmations = ParameterParser.getNumberOfConfirmations(req);

			sbyte type;
			sbyte subtype;
			try
			{
				type = Convert.ToByte(req.getParameter("type"));
			}
			catch(NumberFormatException e)
			{
				type = -1;
			}
			try
			{
				subtype = Convert.ToByte(req.getParameter("subtype"));
			}
			catch(NumberFormatException e)
			{
				subtype = -1;
			}

			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONArray transactionIds = new JSONArray();
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Transaction> iterator = Nxt.getBlockchain().getTransactions(account, numberOfConfirmations, type, subtype, timestamp, firstIndex, lastIndex))
			using (DbIterator<?> iterator = Nxt.Blockchain.getTransactions(account, numberOfConfirmations, type, subtype, timestamp, firstIndex, lastIndex))
			{
				while(iterator.hasNext())
				{
					Transaction transaction = iterator.next();
					transactionIds.add(transaction.StringId);
				}
			}

			JSONObject response = new JSONObject();
			response.put("transactionIds", transactionIds);
			return response;

		}

	}

}