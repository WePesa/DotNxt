using System.Collections.Generic;

namespace nxt.peer
{

	using Block = nxt.Block;
	using Constants = nxt.Constants;
	using Nxt = nxt.Nxt;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;


	internal sealed class GetNextBlocks : PeerServlet.PeerRequestHandler
	{

		internal static readonly GetNextBlocks instance = new GetNextBlocks();

		private GetNextBlocks()
		{
		}


		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{

			JSONObject response = new JSONObject();

			IList<Block> nextBlocks = new List<>();
			int totalLength = 0;
			long blockId = Convert.parseUnsignedLong((string) request.get("blockId"));
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Block> blocks = Nxt.getBlockchain().getBlocksAfter(blockId, 1440);
			IList<?> blocks = Nxt.Blockchain.getBlocksAfter(blockId, 1440);

			foreach (Block block in blocks)
			{
				int length = Constants.BLOCK_HEADER_LENGTH + block.PayloadLength;
				if(totalLength + length > 1048576)
				{
					break;
				}
				nextBlocks.Add(block);
				totalLength += length;
			}

			JSONArray nextBlocksArray = new JSONArray();
			foreach (Block nextBlock in nextBlocks)
			{
				nextBlocksArray.add(nextBlock.JSONObject);
			}
			response.put("nextBlocks", nextBlocksArray);

			return response;
		}

	}

}