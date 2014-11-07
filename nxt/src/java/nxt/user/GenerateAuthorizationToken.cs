namespace nxt.user
{

	using Token = nxt.Token;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.user.JSONResponses.INVALID_SECRET_PHRASE;

	public sealed class GenerateAuthorizationToken : UserServlet.UserRequestHandler
	{

		internal static readonly GenerateAuthorizationToken instance = new GenerateAuthorizationToken();

		private GenerateAuthorizationToken()
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req, User user) throws IOException
		internal override JSONStreamAware processRequest(HttpServletRequest req, User user)
		{
			string secretPhrase = req.getParameter("secretPhrase");
			if(! user.SecretPhrase.Equals(secretPhrase))
			{
				return INVALID_SECRET_PHRASE;
			}

			string tokenString = Token.generateToken(secretPhrase, req.getParameter("website").Trim());

			JSONObject response = new JSONObject();
			response.put("response", "showAuthorizationToken");
			response.put("token", tokenString);

			return response;
		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}