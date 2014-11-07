using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Appendix = nxt.Appendix;
	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using Crypto = nxt.crypto.Crypto;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_TRANSACTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_TRANSACTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.NO_MESSAGE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_TRANSACTION;

	public sealed class ReadMessage : APIServlet.APIRequestHandler
	{

		internal static readonly ReadMessage instance = new ReadMessage();

		private ReadMessage() : base(new APITag[] {APITag.MESSAGES}, "transaction", "secretPhrase")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws ParameterException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string transactionIdString = Convert.emptyToNull(req.getParameter("transaction"));
			if(transactionIdString == null)
			{
				return MISSING_TRANSACTION;
			}

			Transaction transaction;
			try
			{
				transaction = Nxt.Blockchain.getTransaction(Convert.parseUnsignedLong(transactionIdString));
				if(transaction == null)
				{
					return UNKNOWN_TRANSACTION;
				}
			}
			catch(Exception e)
			{
				return INCORRECT_TRANSACTION;
			}

			JSONObject response = new JSONObject();
			Account senderAccount = Account.getAccount(transaction.SenderId);
			Appendix.Message message = transaction.Message;
			Appendix.EncryptedMessage encryptedMessage = transaction.EncryptedMessage;
			Appendix.EncryptToSelfMessage encryptToSelfMessage = transaction.EncryptToSelfMessage;
			if(message == null && encryptedMessage == null && encryptToSelfMessage == null)
			{
				return NO_MESSAGE;
			}
			if(message != null)
			{
				response.put("message", message.Text ? Convert.ToString(message.Message) : Convert.toHexString(message.Message));
			}
			string secretPhrase = Convert.emptyToNull(req.getParameter("secretPhrase"));
			if(secretPhrase != null)
			{
				if(encryptedMessage != null)
				{
					long readerAccountId = Account.getId(Crypto.getPublicKey(secretPhrase));
					Account account = senderAccount.Id == readerAccountId ? Account.getAccount(transaction.RecipientId) : senderAccount;
					if(account != null)
					{
						try
						{
							sbyte[] decrypted = account.decryptFrom(encryptedMessage.EncryptedData, secretPhrase);
							response.put("decryptedMessage", encryptedMessage.Text ? Convert.ToString(decrypted) : Convert.toHexString(decrypted));
						}
						catch(Exception e)
						{
							Logger.logDebugMessage("Decryption of message to recipient failed: " + e.ToString());
						}
					}
				}
				if(encryptToSelfMessage != null)
				{
					Account account = Account.getAccount(Crypto.getPublicKey(secretPhrase));
					if(account != null)
					{
						try
						{
							sbyte[] decrypted = account.decryptFrom(encryptToSelfMessage.EncryptedData, secretPhrase);
							response.put("decryptedMessageToSelf", encryptToSelfMessage.Text ? Convert.ToString(decrypted) : Convert.toHexString(decrypted));
						}
						catch(Exception e)
						{
							Logger.logDebugMessage("Decryption of message to self failed: " + e.ToString());
						}
					}
				}
			}
			return response;
		}

	}

}