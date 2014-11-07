namespace nxt.peer
{

	using Block = nxt.Block;
	using Nxt = nxt.Nxt;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class GetCumulativeDifficulty : PeerServlet.PeerRequestHandler
	{

		internal static readonly GetCumulativeDifficulty instance = new GetCumulativeDifficulty();

		private GetCumulativeDifficulty()
		{
		}


		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{

			JSONObject response = new JSONObject();

			Block lastBlock = Nxt.Blockchain.LastBlock;
			response.put("cumulativeDifficulty", lastBlock.CumulativeDifficulty.ToString());
			response.put("blockchainHeight", lastBlock.Height);
			return response;
		}

	}

}