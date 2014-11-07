using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using NxtException = nxt.NxtException;
	using EncryptedData = nxt.crypto.EncryptedData;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.DECRYPTION_FAILED;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ACCOUNT;

	public sealed class DecryptFrom : APIServlet.APIRequestHandler
	{

		internal static readonly DecryptFrom instance = new DecryptFrom();

		private DecryptFrom() : base(new APITag[] {APITag.MESSAGES}, "account", "data", "nonce", "decryptedMessageIsText", "secretPhrase")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account account = ParameterParser.getAccount(req);
			if(account.PublicKey == null)
			{
				return INCORRECT_ACCOUNT;
			}
			string secretPhrase = ParameterParser.getSecretPhrase(req);
			sbyte[] data = Convert.parseHexString(Convert.nullToEmpty(req.getParameter("data")));
			sbyte[] nonce = Convert.parseHexString(Convert.nullToEmpty(req.getParameter("nonce")));
			EncryptedData encryptedData = new EncryptedData(data, nonce);
			bool isText = !"false".equalsIgnoreCase(req.getParameter("decryptedMessageIsText"));
			try
			{
				sbyte[] decrypted = account.decryptFrom(encryptedData, secretPhrase);
				JSONObject response = new JSONObject();
				response.put("decryptedMessage", isText ? Convert.ToString(decrypted) : Convert.toHexString(decrypted));
				return response;
			}
			catch(Exception e)
			{
				Logger.logDebugMessage(e.ToString());
				return DECRYPTION_FAILED;
			}
		}

	}

}