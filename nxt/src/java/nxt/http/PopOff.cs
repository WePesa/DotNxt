using System;
using System.Collections.Generic;

namespace nxt.http
{

	using Block = nxt.Block;
	using Nxt = nxt.Nxt;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class PopOff : APIServlet.APIRequestHandler
	{

		internal static readonly PopOff instance = new PopOff();

		private PopOff() : base(new APITag[] {APITag.DEBUG}, "numBlocks", "height")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			JSONObject response = new JSONObject();
			int numBlocks = 0;
			try
			{
				numBlocks = Convert.ToInt32(req.getParameter("numBlocks"));
			}
			catch(NumberFormatException e)
			{
			}
			int height = 0;
			try
			{
				height = Convert.ToInt32(req.getParameter("height"));
			}
			catch(NumberFormatException e)
			{
			}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Block> blocks;
			IList<?> blocks;
			JSONArray blocksJSON = new JSONArray();
			if(numBlocks > 0)
			{
				blocks = Nxt.BlockchainProcessor.popOffTo(Nxt.Blockchain.Height - numBlocks);
			}
			else if(height > 0)
			{
				blocks = Nxt.BlockchainProcessor.popOffTo(height);
			}
			else
			{
				response.put("error", "invalid numBlocks or height");
				return response;
			}
			foreach (Block block in blocks)
			{
				blocksJSON.add(JSONData.block(block, true));
			}
			response.put("blocks", blocksJSON);
			return response;
		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}