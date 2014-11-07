using System;

namespace nxt.peer
{

	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using JSON = nxt.util.JSON;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class ProcessBlock : PeerServlet.PeerRequestHandler
	{

		internal static readonly ProcessBlock instance = new ProcessBlock();

		private ProcessBlock()
		{
		}

		private static readonly JSONStreamAware ACCEPTED;
		static ProcessBlock()
		{
			JSONObject response = new JSONObject();
			response.put("accepted", true);
			ACCEPTED = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("accepted", false);
			NOT_ACCEPTED = JSON.prepare(response);
		}

		private static readonly JSONStreamAware NOT_ACCEPTED;

		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{

			try
			{

				if(! Nxt.Blockchain.LastBlock.StringId.Equals(request.get("previousBlock")))
				{
				// do this check first to avoid validation failures of future blocks and transactions
				// when loading blockchain from scratch
					return NOT_ACCEPTED;
				}
				Nxt.BlockchainProcessor.processPeerBlock(request);
				return ACCEPTED;

			}
			catch(NxtException|Exception e)
			{
				if(peer != null)
				{
					peer.blacklist(e);
				}
				return NOT_ACCEPTED;
			}

		}

	}

}