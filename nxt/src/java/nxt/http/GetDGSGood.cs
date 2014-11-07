namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetDGSGood : APIServlet.APIRequestHandler
	{

		internal static readonly GetDGSGood instance = new GetDGSGood();

		private GetDGSGood() : base(new APITag[] {APITag.DGS}, "goods")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			return JSONData.goods(ParameterParser.getGoods(req));
		}

	}

}