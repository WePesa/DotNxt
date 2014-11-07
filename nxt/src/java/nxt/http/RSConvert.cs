using System;

namespace nxt.http
{

	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ACCOUNT;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_ACCOUNT;

	public sealed class RSConvert : APIServlet.APIRequestHandler
	{

		internal static readonly RSConvert instance = new RSConvert();

		private RSConvert() : base(new APITag[] {APITag.ACCOUNTS, APITag.UTILS}, "account")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			string accountValue = Convert.emptyToNull(req.getParameter("account"));
			if(accountValue == null)
			{
				return MISSING_ACCOUNT;
			}
			try
			{
				long accountId = Convert.parseAccountId(accountValue);
				if(accountId == 0)
				{
					return INCORRECT_ACCOUNT;
				}
				JSONObject response = new JSONObject();
				JSONData.putAccount(response, "account", accountId);
				return response;
			}
			catch(Exception e)
			{
				return INCORRECT_ACCOUNT;
			}
		}

	}

}