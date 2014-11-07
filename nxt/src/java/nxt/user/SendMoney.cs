using System;

namespace nxt.user
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Transaction = nxt.Transaction;
	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.user.JSONResponses.NOTIFY_OF_ACCEPTED_TRANSACTION;

	public sealed class SendMoney : UserServlet.UserRequestHandler
	{

		internal static readonly SendMoney instance = new SendMoney();

		private SendMoney()
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req, User user) throws NxtException.ValidationException, IOException
		internal override JSONStreamAware processRequest(HttpServletRequest req, User user)
		{
			if(user.SecretPhrase == null)
			{
				return null;
			}

			string recipientValue = req.getParameter("recipient");
			string amountValue = req.getParameter("amountNXT");
			string feeValue = req.getParameter("feeNXT");
			string deadlineValue = req.getParameter("deadline");
			string secretPhrase = req.getParameter("secretPhrase");

			long recipient;
			long amountNQT = 0;
			long feeNQT = 0;
			short deadline = 0;

			try
			{

				recipient = Convert.parseUnsignedLong(recipientValue);
				if(recipient == 0)
					throw new System.ArgumentException("invalid recipient");
				amountNQT = Convert.parseNXT(amountValue.Trim());
				feeNQT = Convert.parseNXT(feeValue.Trim());
				deadline = (short)(Convert.ToDouble(deadlineValue) * 60);

			}
			catch(Exception e)
			{

				JSONObject response = new JSONObject();
				response.put("response", "notifyOfIncorrectTransaction");
				response.put("message", "One of the fields is filled incorrectly!");
				response.put("recipient", recipientValue);
				response.put("amountNXT", amountValue);
				response.put("feeNXT", feeValue);
				response.put("deadline", deadlineValue);

				return response;
			}

			if(! user.SecretPhrase.Equals(secretPhrase))
			{

				JSONObject response = new JSONObject();
				response.put("response", "notifyOfIncorrectTransaction");
				response.put("message", "Wrong secret phrase!");
				response.put("recipient", recipientValue);
				response.put("amountNXT", amountValue);
				response.put("feeNXT", feeValue);
				response.put("deadline", deadlineValue);

				return response;

			}
			else if(amountNQT <= 0 || amountNQT > Constants.MAX_BALANCE_NQT)
			{

				JSONObject response = new JSONObject();
				response.put("response", "notifyOfIncorrectTransaction");
				response.put("message", "\"Amount\" must be greater than 0!");
				response.put("recipient", recipientValue);
				response.put("amountNXT", amountValue);
				response.put("feeNXT", feeValue);
				response.put("deadline", deadlineValue);

				return response;

			}
			else if(feeNQT < Constants.ONE_NXT || feeNQT > Constants.MAX_BALANCE_NQT)
			{

				JSONObject response = new JSONObject();
				response.put("response", "notifyOfIncorrectTransaction");
				response.put("message", "\"Fee\" must be at least 1 NXT!");
				response.put("recipient", recipientValue);
				response.put("amountNXT", amountValue);
				response.put("feeNXT", feeValue);
				response.put("deadline", deadlineValue);

				return response;

			}
			else if(deadline < 1 || deadline > 1440)
			{

				JSONObject response = new JSONObject();
				response.put("response", "notifyOfIncorrectTransaction");
				response.put("message", "\"Deadline\" must be greater or equal to 1 minute and less than 24 hours!");
				response.put("recipient", recipientValue);
				response.put("amountNXT", amountValue);
				response.put("feeNXT", feeValue);
				response.put("deadline", deadlineValue);

				return response;

			}

			Account account = Account.getAccount(user.PublicKey);
			if(account == null || Convert.safeAdd(amountNQT, feeNQT) > account.UnconfirmedBalanceNQT)
			{

				JSONObject response = new JSONObject();
				response.put("response", "notifyOfIncorrectTransaction");
				response.put("message", "Not enough funds!");
				response.put("recipient", recipientValue);
				response.put("amountNXT", amountValue);
				response.put("feeNXT", feeValue);
				response.put("deadline", deadlineValue);

				return response;

			}
			else
			{

//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Transaction transaction = Nxt.getTransactionProcessor().newTransactionBuilder(user.getPublicKey(), amountNQT, feeNQT, deadline, Attachment.ORDINARY_PAYMENT).recipientId(recipient).build();
				Transaction transaction = Nxt.TransactionProcessor.newTransactionBuilder(user.PublicKey, amountNQT, feeNQT, deadline, Attachment.ORDINARY_PAYMENT).recipientId(recipient).build();
				transaction.validate();
				transaction.sign(user.SecretPhrase);

				Nxt.TransactionProcessor.broadcast(transaction);

				return NOTIFY_OF_ACCEPTED_TRANSACTION;

			}
		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}