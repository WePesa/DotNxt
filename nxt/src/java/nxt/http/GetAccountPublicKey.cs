namespace nxt.http
{

	using Account = nxt.Account;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSON = nxt.util.JSON;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAccountPublicKey : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountPublicKey instance = new GetAccountPublicKey();

		private GetAccountPublicKey() : base(new APITag[] {APITag.ACCOUNTS}, "account")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account account = ParameterParser.getAccount(req);

			if(account.PublicKey != null)
			{
				JSONObject response = new JSONObject();
				response.put("publicKey", Convert.toHexString(account.PublicKey));
				return response;
			}
			else
			{
				return JSON.emptyJSON;
			}
		}

	}

}