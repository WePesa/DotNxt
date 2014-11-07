namespace nxt.peer
{

	using JSON = nxt.util.JSON;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class AddPeers : PeerServlet.PeerRequestHandler
	{

		internal static readonly AddPeers instance = new AddPeers();

		private AddPeers()
		{
		}

		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{
			JSONArray peers = (JSONArray)request.get("peers");
			if(peers != null && Peers.getMorePeers)
			{
				foreach (object announcedAddress in peers)
				{
					Peers.addPeer((string) announcedAddress);
				}
			}
			return JSON.emptyJSON;
		}

	}

}