namespace nxt.http
{

	using Crypto = nxt.crypto.Crypto;
	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_SIGNATURE_HASH;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_UNSIGNED_BYTES;

	public sealed class CalculateFullHash : APIServlet.APIRequestHandler
	{

		internal static readonly CalculateFullHash instance = new CalculateFullHash();

		private CalculateFullHash() : base(new APITag[] {APITag.TRANSACTIONS}, "unsignedTransactionBytes", "signatureHash")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string unsignedBytesString = Convert.emptyToNull(req.getParameter("unsignedTransactionBytes"));
			string signatureHashString = Convert.emptyToNull(req.getParameter("signatureHash"));

			if(unsignedBytesString == null)
			{
				return MISSING_UNSIGNED_BYTES;
			}
			else if(signatureHashString == null)
			{
				return MISSING_SIGNATURE_HASH;
			}

			MessageDigest digest = Crypto.sha256();
			digest.update(Convert.parseHexString(unsignedBytesString));
			sbyte[] fullHash = digest.digest(Convert.parseHexString(signatureHashString));
			JSONObject response = new JSONObject();
			response.put("fullHash", Convert.toHexString(fullHash));

			return response;

		}

	}

}