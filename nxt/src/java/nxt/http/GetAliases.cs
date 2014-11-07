namespace nxt.http
{

	using Alias = nxt.Alias;
	using NxtException = nxt.NxtException;
	using FilteringIterator = nxt.db.FilteringIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAliases : APIServlet.APIRequestHandler
	{

		internal static readonly GetAliases instance = new GetAliases();

		private GetAliases() : base(new APITag[] {APITag.ALIASES}, "timestamp", "account", "firstIndex", "lastIndex")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int timestamp = ParameterParser.getTimestamp(req);
			int timestamp = ParameterParser.getTimestamp(req);
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long accountId = ParameterParser.getAccount(req).getId();
			long accountId = ParameterParser.getAccount(req).Id;
			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONArray aliases = new JSONArray();
			using (FilteringIterator<Alias> aliasIterator = new FilteringIterator<>(Alias.getAliasesByOwner(accountId, 0, -1), new FilteringIterator.Filter<Alias>() { public bool ok(Alias alias) { return alias.Timestamp >= timestamp; } }, firstIndex, lastIndex))
			{
				while(aliasIterator.hasNext())
				{
					aliases.add(JSONData.alias(aliasIterator.next()));
				}
			}

			JSONObject response = new JSONObject();
			response.put("aliases", aliases);
			return response;
		}

	}

}