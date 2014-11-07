using System;

namespace nxt.peer
{

	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using JSON = nxt.util.JSON;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class ProcessTransactions : PeerServlet.PeerRequestHandler
	{

		internal static readonly ProcessTransactions instance = new ProcessTransactions();

		private ProcessTransactions()
		{
		}


		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{

			try
			{
				Nxt.TransactionProcessor.processPeerTransactions(request);
				return JSON.emptyJSON;
			}
			catch(Exception | NxtException.ValidationException e)
			{
			//Logger.logDebugMessage("Failed to parse peer transactions: " + request.toJSONString());
				peer.blacklist(e);
				JSONObject response = new JSONObject();
				response.put("error", e.ToString());
				return response;
			}

		}

	}

}