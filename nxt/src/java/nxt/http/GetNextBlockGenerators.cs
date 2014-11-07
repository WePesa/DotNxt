using System;
using System.Collections.Generic;

namespace nxt.http
{

	using Block = nxt.Block;
	using Constants = nxt.Constants;
	using Hub = nxt.Hub;
	using Nxt = nxt.Nxt;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetNextBlockGenerators : APIServlet.APIRequestHandler
	{

		internal static readonly GetNextBlockGenerators instance = new GetNextBlockGenerators();

		private GetNextBlockGenerators() : base(new APITag[] {APITag.FORGING})
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

//         implement later, if needed
//        Block curBlock;
//
//        String block = req.getParameter("block");
//        if (block == null) {
//            curBlock = Nxt.getBlockchain().getLastBlock();
//        } else {
//            try {
//                curBlock = Nxt.getBlockchain().getBlock(Convert.parseUnsignedLong(block));
//                if (curBlock == null) {
//                    return UNKNOWN_BLOCK;
//                }
//            } catch (RuntimeException e) {
//                return INCORRECT_BLOCK;
//            }
//        }
//        

			Block curBlock = Nxt.Blockchain.LastBlock;
			if(curBlock.Height < Constants.TRANSPARENT_FORGING_BLOCK_7)
			{
				return JSONResponses.FEATURE_NOT_AVAILABLE;
			}


			JSONObject response = new JSONObject();
			response.put("time", Nxt.EpochTime);
			response.put("lastBlock", Convert.toUnsignedLong(curBlock.Id));
			JSONArray hubs = new JSONArray();

			int limit;
			try
			{
				limit = Convert.ToInt32(req.getParameter("limit"));
			}
			catch(Exception e)
			{
				limit = int.MaxValue;
			}

			IEnumerator<Hub.Hit> iterator = Hub.getHubHits(curBlock).GetEnumerator();
			while(iterator.MoveNext() && hubs.size() < limit)
			{
				JSONObject hub = new JSONObject();
				Hub.Hit hit = iterator.Current;
				hub.put("account", Convert.toUnsignedLong(hit.hub.AccountId));
				hub.put("minFeePerByteNQT", hit.hub.MinFeePerByteNQT);
				hub.put("time", hit.hitTime);
				JSONArray uris = new JSONArray();
				uris.addAll(hit.hub.Uris);
				hub.put("uris", uris);
				hubs.add(hub);
			}

			response.put("hubs", hubs);
			return response;
		}

	}

}