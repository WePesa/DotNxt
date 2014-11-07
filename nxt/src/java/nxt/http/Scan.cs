using System;

namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class Scan : APIServlet.APIRequestHandler
	{

		internal static readonly Scan instance = new Scan();

		private Scan() : base(new APITag[] {APITag.DEBUG}, "numBlocks", "height", "validate")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			JSONObject response = new JSONObject();
			try
			{
				if("true".equalsIgnoreCase(req.getParameter("validate")))
				{
					Nxt.BlockchainProcessor.validateAtNextScan();
				}
				int numBlocks = 0;
				try
				{
					numBlocks = Convert.ToInt32(req.getParameter("numBlocks"));
				}
				catch(NumberFormatException e)
				{
				}
				int height = -1;
				try
				{
					height = Convert.ToInt32(req.getParameter("height"));
				}
				catch(NumberFormatException ignore)
				{
				}
				long start = System.currentTimeMillis();
				if(numBlocks > 0)
				{
					Nxt.BlockchainProcessor.scan(Nxt.Blockchain.Height - numBlocks + 1);
				}
				else if(height >= 0)
				{
					Nxt.BlockchainProcessor.scan(height);
				}
				else
				{
					response.put("error", "invalid numBlocks or height");
					return response;
				}
				long end = System.currentTimeMillis();
				response.put("done", true);
				response.put("scanTime", (end - start)/1000);
			}
			catch(Exception e)
			{
				response.put("error", e.ToString());
			}
			return response;
		}

		internal override bool requirePost()
		{
			return true;
		}

	}

}