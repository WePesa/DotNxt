namespace nxt.user
{

	using Block = nxt.Block;
	using Constants = nxt.Constants;
	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using DbIterator = nxt.db.DbIterator;
	using Peer = nxt.peer.Peer;
	using Peers = nxt.peer.Peers;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetInitialData : UserServlet.UserRequestHandler
	{

		internal static readonly GetInitialData instance = new GetInitialData();

		private GetInitialData()
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req, User user) throws IOException
		internal override JSONStreamAware processRequest(HttpServletRequest req, User user)
		{

			JSONArray unconfirmedTransactions = new JSONArray();
			JSONArray activePeers = new JSONArray(), knownPeers = new JSONArray(), blacklistedPeers = new JSONArray();
			JSONArray recentBlocks = new JSONArray();

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Transaction> transactions = Nxt.getTransactionProcessor().getAllUnconfirmedTransactions())
			using (DbIterator<?> transactions = Nxt.TransactionProcessor.AllUnconfirmedTransactions)
			{
				while(transactions.hasNext())
				{
					Transaction transaction = transactions.next();

					JSONObject unconfirmedTransaction = new JSONObject();
					unconfirmedTransaction.put("index", Users.getIndex(transaction));
					unconfirmedTransaction.put("timestamp", transaction.Timestamp);
					unconfirmedTransaction.put("deadline", transaction.Deadline);
					unconfirmedTransaction.put("recipient", Convert.toUnsignedLong(transaction.RecipientId));
					unconfirmedTransaction.put("amountNQT", transaction.AmountNQT);
					unconfirmedTransaction.put("feeNQT", transaction.FeeNQT);
					unconfirmedTransaction.put("sender", Convert.toUnsignedLong(transaction.SenderId));
					unconfirmedTransaction.put("id", transaction.StringId);

					unconfirmedTransactions.add(unconfirmedTransaction);
				}
			}

			foreach (Peer peer in Peers.AllPeers)
			{

				if(peer.Blacklisted)
				{

					JSONObject blacklistedPeer = new JSONObject();
					blacklistedPeer.put("index", Users.getIndex(peer));
					blacklistedPeer.put("address", peer.PeerAddress);
					blacklistedPeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true));
					blacklistedPeer.put("software", peer.Software);
					if(peer.WellKnown)
					{
						blacklistedPeer.put("wellKnown", true);
					}
					blacklistedPeers.add(blacklistedPeer);

				}
				else if(peer.State == Peer.State.NON_CONNECTED)
				{

					JSONObject knownPeer = new JSONObject();
					knownPeer.put("index", Users.getIndex(peer));
					knownPeer.put("address", peer.PeerAddress);
					knownPeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true));
					knownPeer.put("software", peer.Software);
					if(peer.WellKnown)
					{
						knownPeer.put("wellKnown", true);
					}
					knownPeers.add(knownPeer);

				}
				else
				{

					JSONObject activePeer = new JSONObject();
					activePeer.put("index", Users.getIndex(peer));
					if(peer.State == Peer.State.DISCONNECTED)
					{
						activePeer.put("disconnected", true);
					}
					activePeer.put("address", peer.PeerAddress);
					activePeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true));
					activePeer.put("weight", peer.Weight);
					activePeer.put("downloaded", peer.DownloadedVolume);
					activePeer.put("uploaded", peer.UploadedVolume);
					activePeer.put("software", peer.Software);
					if(peer.WellKnown)
					{
						activePeer.put("wellKnown", true);
					}
					activePeers.add(activePeer);
				}
			}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Block> lastBlocks = Nxt.getBlockchain().getBlocks(0, 59))
			using (DbIterator<?> lastBlocks = Nxt.Blockchain.getBlocks(0, 59))
			{
				foreach (Block block in lastBlocks)
				{
					JSONObject recentBlock = new JSONObject();
					recentBlock.put("index", Users.getIndex(block));
					recentBlock.put("timestamp", block.Timestamp);
					recentBlock.put("numberOfTransactions", block.Transactions.size());
					recentBlock.put("totalAmountNQT", block.TotalAmountNQT);
					recentBlock.put("totalFeeNQT", block.TotalFeeNQT);
					recentBlock.put("payloadLength", block.PayloadLength);
					recentBlock.put("generator", Convert.toUnsignedLong(block.GeneratorId));
					recentBlock.put("height", block.Height);
					recentBlock.put("version", block.Version);
					recentBlock.put("block", block.StringId);
					recentBlock.put("baseTarget", BigInteger.valueOf(block.BaseTarget).multiply(BigInteger.valueOf(100000)).divide(BigInteger.valueOf(Constants.INITIAL_BASE_TARGET)));

					recentBlocks.add(recentBlock);
				}
			}

			JSONObject response = new JSONObject();
			response.put("response", "processInitialData");
			response.put("version", Nxt.VERSION);
			if(unconfirmedTransactions.size() > 0)
			{
				response.put("unconfirmedTransactions", unconfirmedTransactions);
			}
			if(activePeers.size() > 0)
			{
				response.put("activePeers", activePeers);
			}
			if(knownPeers.size() > 0)
			{
				response.put("knownPeers", knownPeers);
			}
			if(blacklistedPeers.size() > 0)
			{
				response.put("blacklistedPeers", blacklistedPeers);
			}
			if(recentBlocks.size() > 0)
			{
				response.put("recentBlocks", recentBlocks);
			}

			return response;
		}
	}

}