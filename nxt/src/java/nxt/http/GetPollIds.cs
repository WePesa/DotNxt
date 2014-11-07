namespace nxt.http
{

	using Poll = nxt.Poll;
	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetPollIds : APIServlet.APIRequestHandler
	{

		internal static readonly GetPollIds instance = new GetPollIds();

		private GetPollIds() : base(new APITag[] {APITag.VS}, "firstIndex", "lastIndex")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			int firstIndex = ParameterParser.getFirstIndex(req);
			int lastIndex = ParameterParser.getLastIndex(req);

			JSONArray pollIds = new JSONArray();
			using (DbIterator<Poll> polls = Poll.getAllPolls(firstIndex, lastIndex))
			{
				while(polls.hasNext())
				{
					pollIds.add(Convert.toUnsignedLong(polls.next().Id));
				}
			}
			JSONObject response = new JSONObject();
			response.put("pollIds", pollIds);
			return response;

		}

	}

}