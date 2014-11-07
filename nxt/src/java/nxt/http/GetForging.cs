using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Generator = nxt.Generator;
	using Nxt = nxt.Nxt;
	using Crypto = nxt.crypto.Crypto;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SECRET_PHRASE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.NOT_FORGING;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_ACCOUNT;


	public sealed class GetForging : APIServlet.APIRequestHandler
	{

		internal static readonly GetForging instance = new GetForging();

		private GetForging() : base(new APITag[] {APITag.FORGING}, "secretPhrase")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string secretPhrase = req.getParameter("secretPhrase");
			if(secretPhrase == null)
			{
				return MISSING_SECRET_PHRASE;
			}
			Account account = Account.getAccount(Crypto.getPublicKey(secretPhrase));
			if(account == null)
			{
				return UNKNOWN_ACCOUNT;
			}

			Generator generator = Generator.getGenerator(secretPhrase);
			if(generator == null)
			{
				return NOT_FORGING;
			}

			JSONObject response = new JSONObject();
			long deadline = generator.Deadline;
			response.put("deadline", deadline);
			response.put("hitTime", generator.HitTime);
			int elapsedTime = Nxt.EpochTime - Nxt.Blockchain.LastBlock.Timestamp;
			response.put("remaining", Math.Max(deadline - elapsedTime, 0));
			return response;

		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}