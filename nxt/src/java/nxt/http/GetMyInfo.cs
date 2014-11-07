namespace nxt.http
{

	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetMyInfo : APIServlet.APIRequestHandler
	{

		internal static readonly GetMyInfo instance = new GetMyInfo();

		private GetMyInfo() : base(new APITag[] {APITag.INFO})
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			JSONObject response = new JSONObject();
			response.put("host", req.RemoteHost);
			response.put("address", req.RemoteAddr);
			return response;
		}

	}

}