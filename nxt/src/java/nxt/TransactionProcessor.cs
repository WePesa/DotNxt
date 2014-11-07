namespace nxt
{

	using DbIterator = nxt.db.DbIterator;
	using Observable = nxt.util.Observable;
	using JSONObject = org.json.simple.JSONObject;


	public interface TransactionProcessor : Observable<IList<JavaToDotNetGenericWildcard>, TransactionProcessor.Event> where ? : Transaction
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public interface TransactionProcessor extends Observable<List<? extends Transaction>,TransactionProcessor.Event>
	{

//JAVA TO VB & C# CONVERTER TODO TASK: Interfaces cannot contain types in .NET:
//		public static enum Event
//	{
//		REMOVED_UNCONFIRMED_TRANSACTIONS,
//		ADDED_UNCONFIRMED_TRANSACTIONS,
//		ADDED_CONFIRMED_TRANSACTIONS,
//		ADDED_DOUBLESPENDING_TRANSACTIONS
//	}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getAllUnconfirmedTransactions();
		DbIterator<?> getAllUnconfirmedTransactions() {get;}
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getAllUnconfirmedTransactions();

		Transaction getUnconfirmedTransaction(long transactionId);

		void clearUnconfirmedTransactions();

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void broadcast(Transaction transaction) throws NxtException.ValidationException;
		void broadcast(Transaction transaction);

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void processPeerTransactions(JSONObject request) throws NxtException.ValidationException;
		void processPeerTransactions(JSONObject request);

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Transaction parseTransaction(byte[] bytes) throws NxtException.ValidationException;
		Transaction parseTransaction(sbyte[] bytes);

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: Transaction parseTransaction(JSONObject json) throws NxtException.ValidationException;
		Transaction parseTransaction(JSONObject json);

		Transaction.Builder newTransactionBuilder(sbyte[] senderPublicKey, long amountNQT, long feeNQT, short deadline, Attachment attachment);

	}

}