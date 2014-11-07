using System;

namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_TRANSACTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_TRANSACTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_TRANSACTION;

	public sealed class GetTransaction : APIServlet.APIRequestHandler
	{

		internal static readonly GetTransaction instance = new GetTransaction();

		private GetTransaction() : base(new APITag[] {APITag.TRANSACTIONS}, "transaction", "fullHash")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string transactionIdString = Convert.emptyToNull(req.getParameter("transaction"));
			string transactionFullHash = Convert.emptyToNull(req.getParameter("fullHash"));
			if(transactionIdString == null && transactionFullHash == null)
			{
				return MISSING_TRANSACTION;
			}

			long transactionId = 0;
			Transaction transaction;
			try
			{
				if(transactionIdString != null)
				{
					transactionId = Convert.parseUnsignedLong(transactionIdString);
					transaction = Nxt.Blockchain.getTransaction(transactionId);
				}
				else
				{
					transaction = Nxt.Blockchain.getTransactionByFullHash(transactionFullHash);
					if(transaction == null)
					{
						return UNKNOWN_TRANSACTION;
					}
				}
			}
			catch(Exception e)
			{
				return INCORRECT_TRANSACTION;
			}

			if(transaction == null)
			{
				transaction = Nxt.TransactionProcessor.getUnconfirmedTransaction(transactionId);
				if(transaction == null)
				{
					return UNKNOWN_TRANSACTION;
				}
				return JSONData.unconfirmedTransaction(transaction);
			}
			else
			{
				return JSONData.transaction(transaction);
			}

		}

	}

}