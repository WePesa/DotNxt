namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class SendMessage : CreateTransaction
	{

		internal static readonly SendMessage instance = new SendMessage();

		private SendMessage() : base(new APITag[] {APITag.MESSAGES, APITag.CREATE_TRANSACTION}, "recipient")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			long recipient = ParameterParser.getRecipientId(req);
			Account account = ParameterParser.getSenderAccount(req);
			return createTransaction(req, account, recipient, 0, Attachment.ARBITRARY_MESSAGE);
		}

	}

}