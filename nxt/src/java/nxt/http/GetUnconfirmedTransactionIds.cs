using System;

namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ACCOUNT;

	public sealed class GetUnconfirmedTransactionIds : APIServlet.APIRequestHandler
	{

		internal static readonly GetUnconfirmedTransactionIds instance = new GetUnconfirmedTransactionIds();

		private GetUnconfirmedTransactionIds() : base(new APITag[] {APITag.TRANSACTIONS, APITag.ACCOUNTS}, "account")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string accountIdString = Convert.emptyToNull(req.getParameter("account"));
			long accountId = 0;

			if(accountIdString != null)
			{
				try
				{
					accountId = Convert.parseAccountId(accountIdString);
				}
				catch(Exception e)
				{
					return INCORRECT_ACCOUNT;
				}
			}

			JSONArray transactionIds = new JSONArray();
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Transaction> transactionsIterator = Nxt.getTransactionProcessor().getAllUnconfirmedTransactions())
			using (DbIterator<?> transactionsIterator = Nxt.TransactionProcessor.AllUnconfirmedTransactions)
			{
				while(transactionsIterator.hasNext())
				{
					Transaction transaction = transactionsIterator.next();
					if(accountId != 0 && !(accountId == transaction.SenderId || accountId == transaction.RecipientId))
					{
						continue;
					}
					transactionIds.add(transaction.StringId);
				}
			}

			JSONObject response = new JSONObject();
			response.put("unconfirmedTransactionIds", transactionIds);
			return response;
		}

	}

}