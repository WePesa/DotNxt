using System;

namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using Transaction = nxt.Transaction;
	using Crypto = nxt.crypto.Crypto;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SECRET_PHRASE;

	public sealed class SignTransaction : APIServlet.APIRequestHandler
	{

		internal static readonly SignTransaction instance = new SignTransaction();

		private SignTransaction() : base(new APITag[] {APITag.TRANSACTIONS}, "unsignedTransactionBytes", "unsignedTransactionJSON", "secretPhrase")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string transactionBytes = Convert.emptyToNull(req.getParameter("unsignedTransactionBytes"));
			string transactionJSON = Convert.emptyToNull(req.getParameter("unsignedTransactionJSON"));
			Transaction transaction = ParameterParser.parseTransaction(transactionBytes, transactionJSON);

			string secretPhrase = Convert.emptyToNull(req.getParameter("secretPhrase"));
			if(secretPhrase == null)
			{
				return MISSING_SECRET_PHRASE;
			}

			JSONObject response = new JSONObject();
			try
			{
				transaction.validate();
				if(transaction.Signature != null)
				{
					response.put("errorCode", 4);
					response.put("errorDescription", "Incorrect unsigned transaction - already signed");
					return response;
				}
				if(! Array.Equals(Crypto.getPublicKey(secretPhrase), transaction.SenderPublicKey))
				{
					response.put("errorCode", 4);
					response.put("errorDescription", "Secret phrase doesn't match transaction sender public key");
					return response;
				}
				transaction.sign(secretPhrase);
				response.put("transaction", transaction.StringId);
				response.put("fullHash", transaction.FullHash);
				response.put("transactionBytes", Convert.toHexString(transaction.Bytes));
				response.put("signatureHash", Convert.toHexString(Crypto.sha256().digest(transaction.Signature)));
				response.put("verify", transaction.verifySignature());
			}
			catch(NxtException.ValidationException|Exception e)
			{
				Logger.logDebugMessage(e.Message, e);
				response.put("errorCode", 4);
				response.put("errorDescription", "Incorrect unsigned transaction: " + e.ToString());
				response.put("error", e.Message);
				return response;
			}
			return response;
		}

	}

}