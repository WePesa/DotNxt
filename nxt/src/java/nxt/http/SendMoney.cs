namespace nxt.http
{

	using Account = nxt.Account;
	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class SendMoney : CreateTransaction
	{

		internal static readonly SendMoney instance = new SendMoney();

		private SendMoney() : base(new APITag[] {APITag.ACCOUNTS, APITag.CREATE_TRANSACTION}, "recipient", "amountNQT")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			long recipient = ParameterParser.getRecipientId(req);
			long amountNQT = ParameterParser.getAmountNQT(req);
			Account account = ParameterParser.getSenderAccount(req);
			return createTransaction(req, account, recipient, amountNQT);
		}

	}

}