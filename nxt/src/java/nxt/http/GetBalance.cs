namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetBalance : APIServlet.APIRequestHandler
	{

		internal static readonly GetBalance instance = new GetBalance();

		private GetBalance() : base(new APITag[] {APITag.ACCOUNTS}, "account")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			return JSONData.accountBalance(ParameterParser.getAccount(req));
		}

	}

}