namespace nxt.http
{

	using Block = nxt.Block;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetBlocks : APIServlet.APIRequestHandler
	{

		internal static readonly GetBlocks instance = new GetBlocks();

		private GetBlocks() : base(new APITag[] {APITag.BLOCKS}, "firstIndex", "lastIndex", "includeTransactions")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);
			if(lastIndex < 0 || lastIndex - firstIndex > 99)
			{
				lastIndex = firstIndex + 99;
			}

			bool includeTransactions = "true".equalsIgnoreCase(req.getParameter("includeTransactions"));

			JSONArray blocks = new JSONArray();
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Block> iterator = Nxt.getBlockchain().getBlocks(firstIndex, lastIndex))
			using (DbIterator<?> iterator = Nxt.Blockchain.getBlocks(firstIndex, lastIndex))
			{
				while(iterator.hasNext())
				{
					Block block = iterator.next();
					blocks.add(JSONData.block(block, includeTransactions));
				}
			}

			JSONObject response = new JSONObject();
			response.put("blocks", blocks);

			return response;
		}

	}

}