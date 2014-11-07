using System;

namespace nxt.http
{

	using Constants = nxt.Constants;
	using Hallmark = nxt.peer.Hallmark;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DATE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_HOST;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_WEIGHT;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_DATE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_HOST;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SECRET_PHRASE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_WEIGHT;


	public sealed class MarkHost : APIServlet.APIRequestHandler
	{

		internal static readonly MarkHost instance = new MarkHost();

		private MarkHost() : base(new APITag[] {APITag.TOKENS}, "secretPhrase", "host", "weight", "date")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string secretPhrase = req.getParameter("secretPhrase");
			string host = req.getParameter("host");
			string weightValue = req.getParameter("weight");
			string dateValue = req.getParameter("date");
			if(secretPhrase == null)
			{
				return MISSING_SECRET_PHRASE;
			}
			else if(host == null)
			{
				return MISSING_HOST;
			}
			else if(weightValue == null)
			{
				return MISSING_WEIGHT;
			}
			else if(dateValue == null)
			{
				return MISSING_DATE;
			}

			if(host.Length > 100)
			{
				return INCORRECT_HOST;
			}

			int weight;
			try
			{
				weight = Convert.ToInt32(weightValue);
				if(weight <= 0 || weight > Constants.MAX_BALANCE_NXT)
				{
					return INCORRECT_WEIGHT;
				}
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_WEIGHT;
			}

			try
			{

				string hallmark = Hallmark.generateHallmark(secretPhrase, host, weight, Hallmark.parseDate(dateValue));

				JSONObject response = new JSONObject();
				response.put("hallmark", hallmark);
				return response;

			}
			catch(Exception e)
			{
				return INCORRECT_DATE;
			}

		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}