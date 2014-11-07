namespace nxt.http
{

	using Account = nxt.Account;
	using Crypto = nxt.crypto.Crypto;
	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SECRET_PHRASE_OR_PUBLIC_KEY;

	public sealed class GetAccountId : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountId instance = new GetAccountId();

		private GetAccountId() : base(new APITag[] {APITag.ACCOUNTS}, "secretPhrase", "publicKey")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			long accountId;
			string secretPhrase = Convert.emptyToNull(req.getParameter("secretPhrase"));
			string publicKeyString = Convert.emptyToNull(req.getParameter("publicKey"));
			if(secretPhrase != null)
			{
				sbyte[] publicKey = Crypto.getPublicKey(secretPhrase);
				accountId = Account.getId(publicKey);
				publicKeyString = Convert.toHexString(publicKey);
			}
			else if(publicKeyString != null)
			{
				accountId = Account.getId(Convert.parseHexString(publicKeyString));
			}
			else
			{
				return MISSING_SECRET_PHRASE_OR_PUBLIC_KEY;
			}

			JSONObject response = new JSONObject();
			JSONData.putAccount(response, "account", accountId);
			response.put("publicKey", publicKeyString);

			return response;
		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}