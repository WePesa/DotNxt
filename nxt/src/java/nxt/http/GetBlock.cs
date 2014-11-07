using System;

namespace nxt.http
{

	using Block = nxt.Block;
	using Nxt = nxt.Nxt;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_BLOCK;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_HEIGHT;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_TIMESTAMP;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_BLOCK;

	public sealed class GetBlock : APIServlet.APIRequestHandler
	{

		internal static readonly GetBlock instance = new GetBlock();

		private GetBlock() : base(new APITag[] {APITag.BLOCKS}, "block", "height", "timestamp", "includeTransactions")
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Block blockData;
			string blockValue = Convert.emptyToNull(req.getParameter("block"));
			string heightValue = Convert.emptyToNull(req.getParameter("height"));
			string timestampValue = Convert.emptyToNull(req.getParameter("timestamp"));
			if(blockValue != null)
			{
				try
				{
					blockData = Nxt.Blockchain.getBlock(Convert.parseUnsignedLong(blockValue));
				}
				catch(Exception e)
				{
					return INCORRECT_BLOCK;
				}
			}
			else if(heightValue != null)
			{
				try
				{
					int height = Convert.ToInt32(heightValue);
					if(height < 0 || height > Nxt.Blockchain.Height)
					{
						return INCORRECT_HEIGHT;
					}
					blockData = Nxt.Blockchain.getBlockAtHeight(height);
				}
				catch(Exception e)
				{
					return INCORRECT_HEIGHT;
				}
			}
			else if(timestampValue != null)
			{
				try
				{
					int timestamp = Convert.ToInt32(timestampValue);
					if(timestamp < 0)
					{
						return INCORRECT_TIMESTAMP;
					}
					blockData = Nxt.Blockchain.getLastBlock(timestamp);
				}
				catch(Exception e)
				{
					return INCORRECT_TIMESTAMP;
				}
			}
			else
			{
				blockData = Nxt.Blockchain.LastBlock;
			}

			if(blockData == null)
			{
				return UNKNOWN_BLOCK;
			}

			bool includeTransactions = "true".equalsIgnoreCase(req.getParameter("includeTransactions"));

			return JSONData.block(blockData, includeTransactions);

		}

	}
}