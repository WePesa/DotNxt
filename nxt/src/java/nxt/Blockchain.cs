using System.Collections.Generic;

namespace nxt
{

	using DbIterator = nxt.db.DbIterator;


	public interface Blockchain
	{

		Block LastBlock {get;}

		Block getLastBlock(int timestamp);

		int Height {get;}

		Block getBlock(long blockId);

		Block getBlockAtHeight(int height);

		bool hasBlock(long blockId);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getAllBlocks();
		DbIterator<?> getAllBlocks() {get;}
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getAllBlocks();

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getBlocks(int from, int to);
		DbIterator<?> getBlocks(int from, int to); where ? : Block
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getBlocks(int from, int to);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getBlocks(Account account, int timestamp);
		DbIterator<?> getBlocks(Account account, int timestamp); where ? : Block
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getBlocks(Account account, int timestamp);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getBlocks(Account account, int timestamp, int from, int to);
		DbIterator<?> getBlocks(Account account, int timestamp, int from, int to); where ? : Block
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getBlocks(Account account, int timestamp, int from, int to);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getBlocks(Connection con, PreparedStatement pstmt);
		DbIterator<?> getBlocks(Connection con, PreparedStatement pstmt); where ? : Block
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Block> getBlocks(Connection con, PreparedStatement pstmt);

		IList<long?> getBlockIdsAfter(long blockId, int limit);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Block> getBlocksAfter(long blockId, int limit);
		IList<?> getBlocksAfter(long blockId, int limit); where ? : Block
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Block> getBlocksAfter(long blockId, int limit);

		long getBlockIdAtHeight(int height);

		Transaction getTransaction(long transactionId);

		Transaction getTransactionByFullHash(string fullHash);

		bool hasTransaction(long transactionId);

		bool hasTransactionByFullHash(string fullHash);

		int TransactionCount {get;}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getAllTransactions();
		DbIterator<?> getAllTransactions() {get;}
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getAllTransactions();

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getTransactions(Account account, byte type, byte subtype, int blockTimestamp);
		DbIterator<?> getTransactions(Account account, sbyte type, sbyte subtype, int blockTimestamp); where ? : Transaction
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getTransactions(Account account, byte type, byte subtype, int blockTimestamp);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getTransactions(Account account, int numberOfConfirmations, byte type, byte subtype, int blockTimestamp, int from, int to);
		DbIterator<?> getTransactions(Account account, int numberOfConfirmations, sbyte type, sbyte subtype, int blockTimestamp, int from, int to); where ? : Transaction
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getTransactions(Account account, int numberOfConfirmations, byte type, byte subtype, int blockTimestamp, int from, int to);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getTransactions(Connection con, PreparedStatement pstmt);
		DbIterator<?> getTransactions(Connection con, PreparedStatement pstmt); where ? : Transaction
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: DbIterator<? extends Transaction> getTransactions(Connection con, PreparedStatement pstmt);

	}

}