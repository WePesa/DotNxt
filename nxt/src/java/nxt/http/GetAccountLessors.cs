using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAccountLessors : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountLessors instance = new GetAccountLessors();

		private GetAccountLessors() : base(new APITag[] {APITag.ACCOUNTS}, "account", "height")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account account = ParameterParser.getAccount(req);
			int height = ParameterParser.getHeight(req);
			if(height < 0)
			{
				height = Nxt.Blockchain.Height;
			}

			JSONObject response = new JSONObject();
			JSONData.putAccount(response, "account", account.Id);
			response.put("height", height);
			JSONArray lessorsJSON = new JSONArray();

			using (DbIterator<Account> lessors = account.getLessors(height))
			{
				if(lessors.hasNext())
				{
					while(lessors.hasNext())
					{
						Account lessor = lessors.next();
						JSONObject lessorJSON = new JSONObject();
						JSONData.putAccount(lessorJSON, "lessor", lessor.Id);
						lessorJSON.put("guaranteedBalanceNQT", Convert.ToString(account.getGuaranteedBalanceNQT(1440, height)));
						lessorsJSON.add(lessorJSON);
					}
				}
			}
			response.put("lessors", lessorsJSON);
			return response;

		}

	}

}