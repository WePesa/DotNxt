namespace nxt.peer
{

	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class GetPeers : PeerServlet.PeerRequestHandler
	{

		internal static readonly GetPeers instance = new GetPeers();

		private GetPeers()
		{
		}


		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{

			JSONObject response = new JSONObject();

			JSONArray peers = new JSONArray();
			foreach (Peer otherPeer in Peers.AllPeers)
			{

				if(! otherPeer.Blacklisted && otherPeer.AnnouncedAddress != null && otherPeer.State == Peer.State.CONNECTED && otherPeer.shareAddress())
				{

					peers.add(otherPeer.AnnouncedAddress);

				}

			}
			response.put("peers", peers);

			return response;
		}

	}

}