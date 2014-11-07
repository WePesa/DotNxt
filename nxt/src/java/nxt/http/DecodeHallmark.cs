using System;

namespace nxt.http
{

	using Hallmark = nxt.peer.Hallmark;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_HALLMARK;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_HALLMARK;

	public sealed class DecodeHallmark : APIServlet.APIRequestHandler
	{

		internal static readonly DecodeHallmark instance = new DecodeHallmark();

		private DecodeHallmark() : base(new APITag[] {APITag.TOKENS}, "hallmark")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string hallmarkValue = req.getParameter("hallmark");
			if(hallmarkValue == null)
			{
				return MISSING_HALLMARK;
			}

			try
			{

				Hallmark hallmark = Hallmark.parseHallmark(hallmarkValue);

				return JSONData.hallmark(hallmark);

			}
			catch(Exception e)
			{
				return INCORRECT_HALLMARK;
			}
		}

	}

}