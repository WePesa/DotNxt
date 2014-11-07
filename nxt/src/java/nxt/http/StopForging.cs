namespace nxt.http
{

	using Generator = nxt.Generator;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SECRET_PHRASE;


	public sealed class StopForging : APIServlet.APIRequestHandler
	{

		internal static readonly StopForging instance = new StopForging();

		private StopForging() : base(new APITag[] {APITag.FORGING}, "secretPhrase")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string secretPhrase = req.getParameter("secretPhrase");
			if(secretPhrase == null)
			{
				return MISSING_SECRET_PHRASE;
			}

			Generator generator = Generator.stopForging(secretPhrase);

			JSONObject response = new JSONObject();
			response.put("foundAndStopped", generator != null);
			return response;

		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}