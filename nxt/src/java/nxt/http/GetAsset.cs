namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAsset : APIServlet.APIRequestHandler
	{

		internal static readonly GetAsset instance = new GetAsset();

		private GetAsset() : base(new APITag[] {APITag.AE}, "asset")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			return JSONData.asset(ParameterParser.getAsset(req));
		}

	}

}