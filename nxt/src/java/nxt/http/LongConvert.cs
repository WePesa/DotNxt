using System;

namespace nxt.http
{

	using Convert = nxt.util.Convert;
	using JSON = nxt.util.JSON;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class LongConvert : APIServlet.APIRequestHandler
	{

		internal static readonly LongConvert instance = new LongConvert();

		private LongConvert() : base(new APITag[] {APITag.UTILS}, "id")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			string id = Convert.emptyToNull(req.getParameter("id"));
			if(id == null)
			{
				return JSON.emptyJSON;
			}
			JSONObject response = new JSONObject();
			BigInteger bigInteger = new BigInteger(id);
			if(bigInteger.signum() < 0)
			{
				if(bigInteger.negate().CompareTo(Convert.two64) > 0)
				{
					response.put("error", "overflow");
				}
				else
				{
					response.put("stringId", bigInteger.add(Convert.two64).ToString());
					response.put("longId", Convert.ToString((long)bigInteger));
				}
			}
			else
			{
				if(bigInteger.CompareTo(Convert.two64) >= 0)
				{
					response.put("error", "overflow");
				}
				else
				{
					response.put("stringId", bigInteger.ToString());
					response.put("longId", Convert.ToString((long)bigInteger));
				}
			}
			return response;
		}

	}

}