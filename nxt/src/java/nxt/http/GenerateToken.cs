using System;

namespace nxt.http
{

	using Token = nxt.Token;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_WEBSITE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SECRET_PHRASE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_WEBSITE;


	public sealed class GenerateToken : APIServlet.APIRequestHandler
	{

		internal static readonly GenerateToken instance = new GenerateToken();

		private GenerateToken() : base(new APITag[] {APITag.TOKENS}, "website", "secretPhrase")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string secretPhrase = req.getParameter("secretPhrase");
			string website = req.getParameter("website");
			if(secretPhrase == null)
			{
				return MISSING_SECRET_PHRASE;
			}
			else if(website == null)
			{
				return MISSING_WEBSITE;
			}

			try
			{

				string tokenString = Token.generateToken(secretPhrase, website.Trim());

				JSONObject response = new JSONObject();
				response.put("token", tokenString);

				return response;

			}
			catch(Exception e)
			{
				return INCORRECT_WEBSITE;
			}

		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}