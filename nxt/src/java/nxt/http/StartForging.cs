namespace nxt.http
{

	using Generator = nxt.Generator;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SECRET_PHRASE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_ACCOUNT;


	public sealed class StartForging : APIServlet.APIRequestHandler
	{

		internal static readonly StartForging instance = new StartForging();

		private StartForging() : base(new APITag[] {APITag.FORGING}, "secretPhrase")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string secretPhrase = req.getParameter("secretPhrase");
			if(secretPhrase == null)
			{
				return MISSING_SECRET_PHRASE;
			}

			Generator generator = Generator.startForging(secretPhrase);
			if(generator == null)
			{
				return UNKNOWN_ACCOUNT;
			}

			JSONObject response = new JSONObject();
			response.put("deadline", generator.Deadline);
			response.put("hitTime", generator.HitTime);
			return response;

		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}