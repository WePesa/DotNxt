namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetTime : APIServlet.APIRequestHandler
	{

		internal static readonly GetTime instance = new GetTime();

		private GetTime() : base(new APITag[] {APITag.INFO})
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			JSONObject response = new JSONObject();
			response.put("time", Nxt.EpochTime);

			return response;
		}

	}

}