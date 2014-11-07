using System;

namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_TRANSACTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_TRANSACTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_TRANSACTION;

	public sealed class GetTransactionBytes : APIServlet.APIRequestHandler
	{

		internal static readonly GetTransactionBytes instance = new GetTransactionBytes();

		private GetTransactionBytes() : base(new APITag[] {APITag.TRANSACTIONS}, "transaction")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string transactionValue = req.getParameter("transaction");
			if(transactionValue == null)
			{
				return MISSING_TRANSACTION;
			}

			long transactionId;
			Transaction transaction;
			try
			{
				transactionId = Convert.parseUnsignedLong(transactionValue);
			}
			catch(Exception e)
			{
				return INCORRECT_TRANSACTION;
			}

			transaction = Nxt.Blockchain.getTransaction(transactionId);
			JSONObject response = new JSONObject();
			if(transaction == null)
			{
				transaction = Nxt.TransactionProcessor.getUnconfirmedTransaction(transactionId);
				if(transaction == null)
				{
					return UNKNOWN_TRANSACTION;
				}
			}
			else
			{
				response.put("confirmations", Nxt.Blockchain.Height - transaction.Height);
			}
			response.put("transactionBytes", Convert.toHexString(transaction.Bytes));
			response.put("unsignedTransactionBytes", Convert.toHexString(transaction.UnsignedBytes));
			return response;

		}

	}

}