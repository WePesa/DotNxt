namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ACCOUNT_DESCRIPTION_LENGTH;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ACCOUNT_NAME_LENGTH;

	public sealed class SetAccountInfo : CreateTransaction
	{

		internal static readonly SetAccountInfo instance = new SetAccountInfo();

		private SetAccountInfo() : base(new APITag[] {APITag.ACCOUNTS, APITag.CREATE_TRANSACTION}, "name", "description")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string name = Convert.nullToEmpty(req.getParameter("name")).Trim();
			string description = Convert.nullToEmpty(req.getParameter("description")).Trim();

			if(name.Length > Constants.MAX_ACCOUNT_NAME_LENGTH)
			{
				return INCORRECT_ACCOUNT_NAME_LENGTH;
			}

			if(description.Length > Constants.MAX_ACCOUNT_DESCRIPTION_LENGTH)
			{
				return INCORRECT_ACCOUNT_DESCRIPTION_LENGTH;
			}

			Account account = ParameterParser.getSenderAccount(req);
			Attachment attachment = new Attachment.MessagingAccountInfo(name, description);
			return createTransaction(req, account, attachment);

		}

	}

}