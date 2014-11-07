using System;

namespace nxt.http
{

	using Token = nxt.Token;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_WEBSITE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_TOKEN;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_WEBSITE;

	public sealed class DecodeToken : APIServlet.APIRequestHandler
	{

		internal static readonly DecodeToken instance = new DecodeToken();

		private DecodeToken() : base(new APITag[] {APITag.TOKENS}, "website", "token")
		{
		}

		public override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string website = req.getParameter("website");
			string tokenString = req.getParameter("token");
			if(website == null)
			{
				return MISSING_WEBSITE;
			}
			else if(tokenString == null)
			{
				return MISSING_TOKEN;
			}

			try
			{

				Token token = Token.parseToken(tokenString, website.Trim());

				return JSONData.token(token);

			}
			catch(Exception e)
			{
				return INCORRECT_WEBSITE;
			}
		}

	}

}