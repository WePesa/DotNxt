using System;

namespace nxt.http
{

	using Poll = nxt.Poll;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_POLL;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_POLL;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_POLL;

	public sealed class GetPoll : APIServlet.APIRequestHandler
	{

		internal static readonly GetPoll instance = new GetPoll();

		private GetPoll() : base(new APITag[] {APITag.VS}, "poll")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string poll = req.getParameter("poll");
			if(poll == null)
			{
				return MISSING_POLL;
			}

			Poll pollData;
			try
			{
				pollData = Poll.getPoll(Convert.parseUnsignedLong(poll));
				if(pollData == null)
				{
					return UNKNOWN_POLL;
				}
			}
			catch(Exception e)
			{
				return INCORRECT_POLL;
			}

			return JSONData.poll(pollData);

		}

	}

}