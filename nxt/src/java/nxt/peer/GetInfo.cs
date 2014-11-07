namespace nxt.peer
{

	using Nxt = nxt.Nxt;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class GetInfo : PeerServlet.PeerRequestHandler
	{

		internal static readonly GetInfo instance = new GetInfo();

		private GetInfo()
		{
		}


		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{
			PeerImpl peerImpl = (PeerImpl)peer;
			string announcedAddress = (string)request.get("announcedAddress");
			if(announcedAddress != null && (announcedAddress = announcedAddress.Trim()).Length > 0)
			{
				if(peerImpl.AnnouncedAddress != null && ! announcedAddress.Equals(peerImpl.AnnouncedAddress))
				{
				// force verification of changed announced address
					peerImpl.State = Peer.State.NON_CONNECTED;
				}
				peerImpl.AnnouncedAddress = announcedAddress;
			}
			string application = (string)request.get("application");
			if(application == null)
			{
				application = "?";
			}
			peerImpl.Application = application.Trim();

			string version = (string)request.get("version");
			if(version == null)
			{
				version = "?";
			}
			peerImpl.Version = version.Trim();

			string platform = (string)request.get("platform");
			if(platform == null)
			{
				platform = "?";
			}
			peerImpl.Platform = platform.Trim();

			peerImpl.ShareAddress = bool.TRUE.Equals(request.get("shareAddress"));
			peerImpl.LastUpdated = Nxt.EpochTime;

		//peerImpl.setState(Peer.State.CONNECTED);
			Peers.notifyListeners(peerImpl, Peers.Event.ADDED_ACTIVE_PEER);

			return Peers.myPeerInfoResponse;

		}

	}

}