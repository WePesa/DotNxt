namespace nxt.http
{

	using Peer = nxt.peer.Peer;
	using Peers = nxt.peer.Peers;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetPeers : APIServlet.APIRequestHandler
	{

		internal static readonly GetPeers instance = new GetPeers();

		private GetPeers() : base(new APITag[] {APITag.INFO}, "active", "state")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			bool active = "true".equalsIgnoreCase(req.getParameter("active"));
			string stateValue = Convert.emptyToNull(req.getParameter("state"));

			JSONArray peers = new JSONArray();
			foreach (Peer peer in active ? Peers.ActivePeers : stateValue != null ? Peers.getPeers(Peer.State.valueOf(stateValue)) : Peers.AllPeers)
			{
				peers.add(peer.PeerAddress);
			}

			JSONObject response = new JSONObject();
			response.put("peers", peers);
			return response;
		}

	}

}