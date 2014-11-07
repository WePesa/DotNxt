namespace nxt.http
{


	using Account = nxt.Account;
	using Alias = nxt.Alias;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ALIAS_LENGTH;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ALIAS_NAME;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_URI_LENGTH;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_ALIAS_NAME;

	public sealed class SetAlias : CreateTransaction
	{

		internal static readonly SetAlias instance = new SetAlias();

		private SetAlias() : base(new APITag[] {APITag.ALIASES, APITag.CREATE_TRANSACTION}, "aliasName", "aliasURI")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			string aliasName = Convert.emptyToNull(req.getParameter("aliasName"));
			string aliasURI = Convert.nullToEmpty(req.getParameter("aliasURI"));

			if(aliasName == null)
			{
				return MISSING_ALIAS_NAME;
			}

			aliasName = aliasName.Trim();
			if(aliasName.Length == 0 || aliasName.Length > Constants.MAX_ALIAS_LENGTH)
			{
				return INCORRECT_ALIAS_LENGTH;
			}

			string normalizedAlias = aliasName.ToLower();
			for(int i = 0; i < normalizedAlias.Length; i++)
			{
				if(Constants.ALPHABET.IndexOf(normalizedAlias[i]) < 0)
				{
					return INCORRECT_ALIAS_NAME;
				}
			}

			aliasURI = aliasURI.Trim();
			if(aliasURI.Length > Constants.MAX_ALIAS_URI_LENGTH)
			{
				return INCORRECT_URI_LENGTH;
			}

			Account account = ParameterParser.getSenderAccount(req);

			Alias alias = Alias.getAlias(normalizedAlias);
			if(alias != null && alias.AccountId != account.Id)
			{
				JSONObject response = new JSONObject();
				response.put("errorCode", 8);
				response.put("errorDescription", "\"" + aliasName + "\" is already used");
				return response;
			}

			Attachment attachment = new Attachment.MessagingAliasAssignment(aliasName, aliasURI);
			return createTransaction(req, account, attachment);

		}

	}

}