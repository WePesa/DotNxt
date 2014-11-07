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

	public sealed class GetAccountBlockIds : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccountBlockIds instance = new GetAccountBlockIds();

		private GetAccountBlockIds() : base(new APITag[] {APITag.ACCOUNTS}, "account", "timestamp", "firstIndex", "lastIndex")
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

			JSONArray blockIds = new JSONArray();
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Block> iterator = Nxt.getBlockchain().getBlocks(account, timestamp, firstIndex, lastIndex))
			using (DbIterator<?> iterator = Nxt.Blockchain.getBlocks(account, timestamp, firstIndex, lastIndex))
			{
				while(iterator.hasNext())
				{
					Block block = iterator.next();
					blockIds.add(block.StringId);
				}
			}

			JSONObject response = new JSONObject();
			response.put("blockIds", blockIds);

			return response;
		}

	}

}