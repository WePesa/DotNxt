namespace nxt.peer
{

	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using DbIterator = nxt.db.DbIterator;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class GetUnconfirmedTransactions : PeerServlet.PeerRequestHandler
	{

		internal static readonly GetUnconfirmedTransactions instance = new GetUnconfirmedTransactions();

		private GetUnconfirmedTransactions()
		{
		}


		internal override JSONStreamAware processRequest(JSONObject request, Peer peer)
		{

			JSONObject response = new JSONObject();

			JSONArray transactionsData = new JSONArray();
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Transaction> transacitons = Nxt.getTransactionProcessor().getAllUnconfirmedTransactions())
			using (DbIterator<?> transacitons = Nxt.TransactionProcessor.AllUnconfirmedTransactions)
			{
				while(transacitons.hasNext())
				{
					Transaction transaction = transacitons.next();
					transactionsData.add(transaction.JSONObject);
				}
			}
			response.put("unconfirmedTransactions", transactionsData);


			return response;
		}

	}

}