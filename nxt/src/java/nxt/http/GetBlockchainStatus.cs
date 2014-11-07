namespace nxt.http
{

	using Block = nxt.Block;
	using BlockchainProcessor = nxt.BlockchainProcessor;
	using Nxt = nxt.Nxt;
	using Peer = nxt.peer.Peer;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetBlockchainStatus : APIServlet.APIRequestHandler
	{

		internal static readonly GetBlockchainStatus instance = new GetBlockchainStatus();

		private GetBlockchainStatus() : base(new APITag[] {APITag.BLOCKS, APITag.INFO})
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			JSONObject response = new JSONObject();
			response.put("application", Nxt.APPLICATION);
			response.put("version", Nxt.VERSION);
			response.put("time", Nxt.EpochTime);
			Block lastBlock = Nxt.Blockchain.LastBlock;
			response.put("lastBlock", lastBlock.StringId);
			response.put("cumulativeDifficulty", lastBlock.CumulativeDifficulty.ToString());
			response.put("numberOfBlocks", lastBlock.Height + 1);
			BlockchainProcessor blockchainProcessor = Nxt.BlockchainProcessor;
			Peer lastBlockchainFeeder = blockchainProcessor.LastBlockchainFeeder;
			response.put("lastBlockchainFeeder", lastBlockchainFeeder == null ? null : lastBlockchainFeeder.AnnouncedAddress);
			response.put("lastBlockchainFeederHeight", blockchainProcessor.LastBlockchainFeederHeight);
			response.put("isScanning", blockchainProcessor.Scanning);
			return response;
		}

	}

}