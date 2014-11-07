namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetDGSPurchase : APIServlet.APIRequestHandler
	{

		internal static readonly GetDGSPurchase instance = new GetDGSPurchase();

		private GetDGSPurchase() : base(new APITag[] {APITag.DGS}, "purchase")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			return JSONData.purchase(ParameterParser.getPurchase(req));
		}

	}

}