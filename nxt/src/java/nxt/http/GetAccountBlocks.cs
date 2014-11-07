namespace nxt.http
{

	using Account = nxt.Account;
	using Block = nxt.Block;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAccountBlocks : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountBlocks instance = new GetAccountBlocks();

		private GetAccountBlocks() : base(new APITag[] {APITag.ACCOUNTS}, "account", "timestamp", "firstIndex", "lastIndex", "includeTransactions")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account account = ParameterParser.getAccount(req);
			int timestamp = ParameterParser.getTimestamp(req);
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			bool includeTransactions = "true".equalsIgnoreCase(req.getParameter("includeTransactions"));

			JSONArray blocks = new JSONArray();
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Block> iterator = Nxt.getBlockchain().getBlocks(account, timestamp, firstIndex, lastIndex))
			using (DbIterator<?> iterator = Nxt.Blockchain.getBlocks(account, timestamp, firstIndex, lastIndex))
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