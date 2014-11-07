namespace nxt.http
{

	using Account = nxt.Account;
	using NxtException = nxt.NxtException;
	using EncryptedData = nxt.crypto.EncryptedData;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_RECIPIENT;

	public sealed class EncryptTo : APIServlet.APIRequestHandler
	{

		internal static readonly EncryptTo instance = new EncryptTo();

		private EncryptTo() : base(new APITag[] {APITag.MESSAGES}, "recipient", "messageToEncrypt", "messageToEncryptIsText", "secretPhrase")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			long recipientId = ParameterParser.getRecipientId(req);
			Account recipientAccount = Account.getAccount(recipientId);
			if(recipientAccount == null || recipientAccount.PublicKey == null)
			{
				return INCORRECT_RECIPIENT;
			}

			EncryptedData encryptedData = ParameterParser.getEncryptedMessage(req, recipientAccount);
			return JSONData.encryptedData(encryptedData);

		}

	}

}