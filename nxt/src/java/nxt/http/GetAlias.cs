namespace nxt.http
{

	using Alias = nxt.Alias;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAlias : APIServlet.APIRequestHandler
	{

		internal static readonly GetAlias instance = new GetAlias();

		private GetAlias() : base(new APITag[] {APITag.ALIASES}, "alias", "aliasName")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws ParameterException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			Alias alias = ParameterParser.getAlias(req);
			return JSONData.alias(alias);
		}

	}

}