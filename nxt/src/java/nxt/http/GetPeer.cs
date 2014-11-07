namespace nxt.http
{

	using Peer = nxt.peer.Peer;
	using Peers = nxt.peer.Peers;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_PEER;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_PEER;

	public sealed class GetPeer : APIServlet.APIRequestHandler
	{

		internal static readonly GetPeer instance = new GetPeer();

		private GetPeer() : base(new APITag[] {APITag.INFO}, "peer")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string peerAddress = req.getParameter("peer");
			if(peerAddress == null)
			{
				return MISSING_PEER;
			}

			Peer peer = Peers.getPeer(peerAddress);
			if(peer == null)
			{
				return UNKNOWN_PEER;
			}

			return JSONData.peer(peer);

		}

	}

}