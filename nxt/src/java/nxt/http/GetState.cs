namespace nxt.http
{

	using Account = nxt.Account;
	using Alias = nxt.Alias;
	using Asset = nxt.Asset;
	using AssetTransfer = nxt.AssetTransfer;
	using Generator = nxt.Generator;
	using Nxt = nxt.Nxt;
	using Order = nxt.Order;
	using Trade = nxt.Trade;
	using Peer = nxt.peer.Peer;
	using Peers = nxt.peer.Peers;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetState : APIServlet.APIRequestHandler
	{

		internal static readonly GetState instance = new GetState();

		private GetState() : base(new APITag[] {APITag.INFO}, "includeCounts")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			JSONObject response = new JSONObject();

			response.put("application", Nxt.APPLICATION);
			response.put("version", Nxt.VERSION);
			response.put("time", Nxt.EpochTime);
			response.put("lastBlock", Nxt.Blockchain.LastBlock.StringId);
			response.put("cumulativeDifficulty", Nxt.Blockchain.LastBlock.CumulativeDifficulty.ToString());

//        
//        long totalEffectiveBalance = 0;
//        try (DbIterator<Account> accounts = Account.getAllAccounts(0, -1)) {
//            for (Account account : accounts) {
//                long effectiveBalanceNXT = account.getEffectiveBalanceNXT();
//                if (effectiveBalanceNXT > 0) {
//                    totalEffectiveBalance += effectiveBalanceNXT;
//                }
//            }
//        }
//        response.put("totalEffectiveBalanceNXT", totalEffectiveBalance);
//        

			if(!"false".equalsIgnoreCase(req.getParameter("includeCounts")))
			{
				response.put("numberOfBlocks", Nxt.Blockchain.Height + 1);
				response.put("numberOfTransactions", Nxt.Blockchain.TransactionCount);
				response.put("numberOfAccounts", Account.Count);
				response.put("numberOfAssets", Asset.Count);
				int askCount = Order.Ask.Count;
				int bidCount = Order.Bid.Count;
				response.put("numberOfOrders", askCount + bidCount);
				response.put("numberOfAskOrders", askCount);
				response.put("numberOfBidOrders", bidCount);
				response.put("numberOfTrades", Trade.Count);
				response.put("numberOfTransfers", AssetTransfer.Count);
				response.put("numberOfAliases", Alias.Count);
			//response.put("numberOfPolls", Poll.getCount());
			//response.put("numberOfVotes", Vote.getCount());
			}

			response.put("numberOfPeers", Peers.AllPeers.size());
			response.put("numberOfUnlockedAccounts", Generator.AllGenerators.size());
			Peer lastBlockchainFeeder = Nxt.BlockchainProcessor.LastBlockchainFeeder;
			response.put("lastBlockchainFeeder", lastBlockchainFeeder == null ? null : lastBlockchainFeeder.AnnouncedAddress);
			response.put("lastBlockchainFeederHeight", Nxt.BlockchainProcessor.LastBlockchainFeederHeight);
			response.put("isScanning", Nxt.BlockchainProcessor.Scanning);
			response.put("availableProcessors", Runtime.Runtime.availableProcessors());
			response.put("maxMemory", Runtime.Runtime.maxMemory());
			response.put("totalMemory", Runtime.Runtime.totalMemory());
			response.put("freeMemory", Runtime.Runtime.freeMemory());

			return response;
		}

	}

}