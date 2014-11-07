using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using NxtException = nxt.NxtException;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetGuaranteedBalance : APIServlet.APIRequestHandler
	{

		internal static readonly GetGuaranteedBalance instance = new GetGuaranteedBalance();

		private GetGuaranteedBalance() : base(new APITag[] {APITag.ACCOUNTS, APITag.FORGING}, "account", "numberOfConfirmations")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account account = ParameterParser.getAccount(req);
			int numberOfConfirmations = ParameterParser.getNumberOfConfirmations(req);

			JSONObject response = new JSONObject();
			if(account == null)
			{
				response.put("guaranteedBalanceNQT", "0");
			}
			else
			{
				response.put("guaranteedBalanceNQT", Convert.ToString(account.getGuaranteedBalanceNQT(numberOfConfirmations)));
			}

			return response;
		}

	}

}