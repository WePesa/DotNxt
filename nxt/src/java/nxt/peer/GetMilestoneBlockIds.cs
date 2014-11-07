using System;

namespace nxt.peer
{

	using Block = nxt.Block;
	using Nxt = nxt.Nxt;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class GetMilestoneBlockIds : PeerServlet.PeerRequestHandler
	{

		internal static readonly GetMilestoneBlockIds instance = new GetMilestoneBlockIds();

		private GetMilestoneBlockIds()
		{
		}


		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{

			JSONObject response = new JSONObject();
			try
			{

				JSONArray milestoneBlockIds = new JSONArray();

				string lastBlockIdString = (string) request.get("lastBlockId");
				if(lastBlockIdString != null)
				{
					long lastBlockId = Convert.parseUnsignedLong(lastBlockIdString);
					long myLastBlockId = Nxt.Blockchain.LastBlock.Id;
					if(myLastBlockId == lastBlockId || Nxt.Blockchain.hasBlock(lastBlockId))
					{
						milestoneBlockIds.add(lastBlockIdString);
						response.put("milestoneBlockIds", milestoneBlockIds);
						if(myLastBlockId == lastBlockId)
						{
							response.put("last", bool.TRUE);
						}
						return response;
					}
				}

				long blockId;
				int height;
				int jump;
				int limit = 10;
				int blockchainHeight = Nxt.Blockchain.Height;
				string lastMilestoneBlockIdString = (string) request.get("lastMilestoneBlockId");
				if(lastMilestoneBlockIdString != null)
				{
					Block lastMilestoneBlock = Nxt.Blockchain.getBlock(Convert.parseUnsignedLong(lastMilestoneBlockIdString));
					if(lastMilestoneBlock == null)
					{
						throw new IllegalStateException("Don't have block " + lastMilestoneBlockIdString);
					}
					height = lastMilestoneBlock.Height;
					jump = Math.Min(1440, Math.Max(blockchainHeight - height, 1));
					height = Math.Max(height - jump, 0);
				}
				else if(lastBlockIdString != null)
				{
					height = blockchainHeight;
					jump = 10;
				}
				else
				{
					peer.blacklist();
					response.put("error", "Old getMilestoneBlockIds protocol not supported, please upgrade");
					return response;
				}
				blockId = Nxt.Blockchain.getBlockIdAtHeight(height);

				while(height > 0 && limit-- > 0)
				{
					milestoneBlockIds.add(Convert.toUnsignedLong(blockId));
					blockId = Nxt.Blockchain.getBlockIdAtHeight(height);
					height = height - jump;
				}
				response.put("milestoneBlockIds", milestoneBlockIds);

			}
			catch(Exception e)
			{
				Logger.logDebugMessage(e.ToString());
				response.put("error", e.ToString());
			}

			return response;
		}

	}

}