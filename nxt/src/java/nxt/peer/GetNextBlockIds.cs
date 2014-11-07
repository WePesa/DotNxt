using System.Collections.Generic;

namespace nxt.peer
{

	using Nxt = nxt.Nxt;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;


	internal sealed class GetNextBlockIds : PeerServlet.PeerRequestHandler
	{

		internal static readonly GetNextBlockIds instance = new GetNextBlockIds();

		private GetNextBlockIds()
		{
		}


		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{

			JSONObject response = new JSONObject();

			JSONArray nextBlockIds = new JSONArray();
			long blockId = Convert.parseUnsignedLong((string) request.get("blockId"));
			IList<long?> ids = Nxt.Blockchain.getBlockIdsAfter(blockId, 1440);

			foreach (long? id in ids)
			{
				nextBlockIds.add(Convert.toUnsignedLong(id));
			}

			response.put("nextBlockIds", nextBlockIds);

			return response;
		}

	}

}