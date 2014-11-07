using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_PERIOD;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_PERIOD;

	public sealed class LeaseBalance : CreateTransaction
	{

		internal static readonly LeaseBalance instance = new LeaseBalance();

		private LeaseBalance() : base(new APITag[] {APITag.FORGING}, "period", "recipient")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string periodString = Convert.emptyToNull(req.getParameter("period"));
			if(periodString == null)
			{
				return MISSING_PERIOD;
			}
			short period;
			try
			{
				period = Convert.ToInt16(periodString);
				if(period < 1440)
				{
					return INCORRECT_PERIOD;
				}
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_PERIOD;
			}

			Account account = ParameterParser.getSenderAccount(req);
			long recipient = ParameterParser.getRecipientId(req);
			Account recipientAccount = Account.getAccount(recipient);
			if(recipientAccount == null || recipientAccount.PublicKey == null)
			{
				JSONObject response = new JSONObject();
				response.put("errorCode", 8);
				response.put("errorDescription", "recipient account does not have public key");
				return response;
			}
			Attachment attachment = new Attachment.AccountControlEffectiveBalanceLeasing(period);
			return createTransaction(req, account, recipient, 0, attachment);

		}

	}

}