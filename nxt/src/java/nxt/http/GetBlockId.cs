using System;

namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using Convert = nxt.util.Convert;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_HEIGHT;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_HEIGHT;

	public sealed class GetBlockId : APIServlet.APIRequestHandler
	{

		internal static readonly GetBlockId instance = new GetBlockId();

		private GetBlockId() : base(new APITag[] {APITag.BLOCKS}, "height")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			int height;
			try
			{
				string heightValue = Convert.emptyToNull(req.getParameter("height"));
				if(heightValue == null)
				{
					return MISSING_HEIGHT;
				}
				height = Convert.ToInt32(heightValue);
			}
			catch(Exception e)
			{
				return INCORRECT_HEIGHT;
			}

			try
			{
				JSONObject response = new JSONObject();
				response.put("block", Convert.toUnsignedLong(Nxt.Blockchain.getBlockIdAtHeight(height)));
				return response;
			}
			catch(Exception e)
			{
				return INCORRECT_HEIGHT;
			}

		}

	}
}