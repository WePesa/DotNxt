namespace nxt.http
{

	using Block = nxt.Block;
	using EconomicClustering = nxt.EconomicClustering;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetECBlock : APIServlet.APIRequestHandler
	{

		internal static readonly GetECBlock instance = new GetECBlock();

		private GetECBlock() : base(new APITag[] {APITag.BLOCKS}, "timestamp")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			int timestamp = ParameterParser.getTimestamp(req);
			if(timestamp == 0)
			{
				timestamp = Nxt.EpochTime;
			}
			if(timestamp < Nxt.Blockchain.LastBlock.Timestamp - 15)
			{
				return JSONResponses.INCORRECT_TIMESTAMP;
			}
			Block ecBlock = EconomicClustering.getECBlock(timestamp);
			JSONObject response = new JSONObject();
			response.put("ecBlockId", ecBlock.StringId);
			response.put("ecBlockHeight", ecBlock.Height);
			response.put("timestamp", timestamp);
			return response;
		}

	}
}