using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Appendix = nxt.Appendix;
	using Attachment = nxt.Attachment;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Transaction = nxt.Transaction;
	using Crypto = nxt.crypto.Crypto;
	using EncryptedData = nxt.crypto.EncryptedData;
	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.FEATURE_NOT_AVAILABLE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ARBITRARY_MESSAGE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DEADLINE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_REFERENCED_TRANSACTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_DEADLINE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SECRET_PHRASE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.NOT_ENOUGH_FUNDS;

	internal abstract class CreateTransaction : APIServlet.APIRequestHandler
	{

		private static readonly string[] commonParameters = new string[] {"secretPhrase", "publicKey", "feeNQT", "deadline", "referencedTransactionFullHash", "broadcast", "message", "messageIsText", "messageToEncrypt", "messageToEncryptIsText", "encryptedMessageData", "encryptedMessageNonce", "messageToEncryptToSelf", "messageToEncryptToSelfIsText", "encryptToSelfMessageData", "encryptToSelfMessageNonce", "recipientPublicKey"};

		private static string[] addCommonParameters(string[] parameters)
		{
			string[] result = new string[parameters.length + commonParameters.length];
			Array.Copy(parameters, result, parameters.Length + commonParameters.Length);
			Array.Copy(commonParameters, 0, result, parameters.Length, commonParameters.Length);
			return result;
		}

		internal CreateTransaction(APITag[] apiTags, params string[] parameters) : base(apiTags, addCommonParameters(parameters))
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: final JSONStreamAware createTransaction(HttpServletRequest req, Account senderAccount, Attachment attachment) throws NxtException
		internal JSONStreamAware createTransaction(HttpServletRequest req, Account senderAccount, Attachment attachment)
		{
			return createTransaction(req, senderAccount, 0, 0, attachment);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: final JSONStreamAware createTransaction(HttpServletRequest req, Account senderAccount, long recipientId, long amountNQT) throws NxtException
		internal JSONStreamAware createTransaction(HttpServletRequest req, Account senderAccount, long recipientId, long amountNQT)
		{
			return createTransaction(req, senderAccount, recipientId, amountNQT, Attachment.ORDINARY_PAYMENT);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: final JSONStreamAware createTransaction(HttpServletRequest req, Account senderAccount, long recipientId, long amountNQT, Attachment attachment) throws NxtException
		internal JSONStreamAware createTransaction(HttpServletRequest req, Account senderAccount, long recipientId, long amountNQT, Attachment attachment)
		{
			string deadlineValue = req.getParameter("deadline");
			string referencedTransactionFullHash = Convert.emptyToNull(req.getParameter("referencedTransactionFullHash"));
			string referencedTransactionId = Convert.emptyToNull(req.getParameter("referencedTransaction"));
			string secretPhrase = Convert.emptyToNull(req.getParameter("secretPhrase"));
			string publicKeyValue = Convert.emptyToNull(req.getParameter("publicKey"));
			bool broadcast = !"false".equalsIgnoreCase(req.getParameter("broadcast"));
			Appendix.EncryptedMessage encryptedMessage = null;
			if(attachment.TransactionType.hasRecipient())
			{
				EncryptedData encryptedData = ParameterParser.getEncryptedMessage(req, Account.getAccount(recipientId));
				if(encryptedData != null)
				{
					encryptedMessage = new Appendix.EncryptedMessage(encryptedData, !"false".equalsIgnoreCase(req.getParameter("messageToEncryptIsText")));
				}
			}
			Appendix.EncryptToSelfMessage encryptToSelfMessage = null;
			EncryptedData encryptedToSelfData = ParameterParser.getEncryptToSelfMessage(req);
			if(encryptedToSelfData != null)
			{
				encryptToSelfMessage = new Appendix.EncryptToSelfMessage(encryptedToSelfData, !"false".equalsIgnoreCase(req.getParameter("messageToEncryptToSelfIsText")));
			}
			Appendix.Message message = null;
			string messageValue = Convert.emptyToNull(req.getParameter("message"));
			if(messageValue != null)
			{
				bool messageIsText = !"false".equalsIgnoreCase(req.getParameter("messageIsText"));
				try
				{
					message = messageIsText ? new Appendix.Message(messageValue) : new Appendix.Message(Convert.parseHexString(messageValue));
				}
				catch(Exception e)
				{
					throw new ParameterException(INCORRECT_ARBITRARY_MESSAGE);
				}
			}
			Appendix.PublicKeyAnnouncement publicKeyAnnouncement = null;
			string recipientPublicKey = Convert.emptyToNull(req.getParameter("recipientPublicKey"));
			if(recipientPublicKey != null)
			{
				publicKeyAnnouncement = new Appendix.PublicKeyAnnouncement(Convert.parseHexString(recipientPublicKey));
			}

			if(secretPhrase == null && publicKeyValue == null)
			{
				return MISSING_SECRET_PHRASE;
			}
			else if(deadlineValue == null)
			{
				return MISSING_DEADLINE;
			}

			short deadline;
			try
			{
				deadline = Convert.ToInt16(deadlineValue);
				if(deadline < 1 || deadline > 1440)
				{
					return INCORRECT_DEADLINE;
				}
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_DEADLINE;
			}

			long feeNQT = ParameterParser.getFeeNQT(req);
			if(referencedTransactionId != null)
			{
				return INCORRECT_REFERENCED_TRANSACTION;
			}

			JSONObject response = new JSONObject();

		// shouldn't try to get publicKey from senderAccount as it may have not been set yet
			sbyte[] publicKey = secretPhrase != null ? Crypto.getPublicKey(secretPhrase) : Convert.parseHexString(publicKeyValue);

			try
			{
				Transaction.Builder builder = Nxt.TransactionProcessor.newTransactionBuilder(publicKey, amountNQT, feeNQT, deadline, attachment).referencedTransactionFullHash(referencedTransactionFullHash);
				if(attachment.TransactionType.hasRecipient())
				{
					builder.recipientId(recipientId);
				}
				if(encryptedMessage != null)
				{
					builder.encryptedMessage(encryptedMessage);
				}
				if(message != null)
				{
					builder.message(message);
				}
				if(publicKeyAnnouncement != null)
				{
					builder.publicKeyAnnouncement(publicKeyAnnouncement);
				}
				if(encryptToSelfMessage != null)
				{
					builder.encryptToSelfMessage(encryptToSelfMessage);
				}
				Transaction transaction = builder.build();
				transaction.validate();
				try
				{
					if(Convert.safeAdd(amountNQT, transaction.FeeNQT) > senderAccount.UnconfirmedBalanceNQT)
					{
						return NOT_ENOUGH_FUNDS;
					}
				}
				catch(ArithmeticException e)
				{
					return NOT_ENOUGH_FUNDS;
				}
				if(secretPhrase != null)
				{
					transaction.sign(secretPhrase);
					response.put("transaction", transaction.StringId);
					response.put("fullHash", transaction.FullHash);
					response.put("transactionBytes", Convert.toHexString(transaction.Bytes));
					response.put("signatureHash", Convert.toHexString(Crypto.sha256().digest(transaction.Signature)));
					if(broadcast)
					{
						Nxt.TransactionProcessor.broadcast(transaction);
						response.put("broadcasted", true);
					}
					else
					{
						response.put("broadcasted", false);
					}
				}
				else
				{
					response.put("broadcasted", false);
				}
				response.put("unsignedTransactionBytes", Convert.toHexString(transaction.UnsignedBytes));
				response.put("transactionJSON", JSONData.unconfirmedTransaction(transaction));

			}
			catch(NxtException.NotYetEnabledException e)
			{
				return FEATURE_NOT_AVAILABLE;
			}
			catch(NxtException.ValidationException e)
			{
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