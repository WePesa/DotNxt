using System.Collections.Generic;

namespace nxt
{

	using DerivedDbTable = nxt.db.DerivedDbTable;
	using Peer = nxt.peer.Peer;
	using Observable = nxt.util.Observable;
	using JSONObject = org.json.simple.JSONObject;


	public interface BlockchainProcessor : Observable<Block, BlockchainProcessor.Event>
	{

//JAVA TO VB & C# CONVERTER TODO TASK: Interfaces cannot contain types in .NET:
//		public static enum Event
//	{
//		BLOCK_PUSHED, BLOCK_POPPED, BLOCK_GENERATED, BLOCK_SCANNED,
//		RESCAN_BEGIN, RESCAN_END,
//		BEFORE_BLOCK_ACCEPT,
//		BEFORE_BLOCK_APPLY, AFTER_BLOCK_APPLY
//	}

		Peer LastBlockchainFeeder {get;}

		int LastBlockchainFeederHeight {get;}

		bool isScanning() {get;}

		int MinRollbackHeight {get;}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void processPeerBlock(JSONObject request) throws NxtException;
		void processPeerBlock(JSONObject request);

		void fullReset();

		void scan(int height);

		void forceScanAtStart();

		void validateAtNextScan();

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Block> popOffTo(int height);
		IList<?> popOffTo(int height); where ? : Block
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Block> popOffTo(int height);

		void registerDerivedTable(DerivedDbTable table);


//JAVA TO VB & C# CONVERTER TODO TASK: Interfaces cannot contain types in .NET:
//		public static class BlockNotAcceptedException extends NxtException
//	{
//
//		BlockNotAcceptedException(String message)
//		{
//			base(message);
//		}
//
//	}

//JAVA TO VB & C# CONVERTER TODO TASK: Interfaces cannot contain types in .NET:
//		public static class TransactionNotAcceptedException extends BlockNotAcceptedException
//	{
//
//		private final TransactionImpl transaction;
//
//		TransactionNotAcceptedException(String message, TransactionImpl transaction)
//		{
//			base(message + " transaction: " + transaction.getJSONObject().toJSONString());
//			this.transaction = transaction;
//		}
//
//		public Transaction getTransaction()
//		{
//			return transaction;
//		}
//
//	}

//JAVA TO VB & C# CONVERTER TODO TASK: Interfaces cannot contain types in .NET:
//		public static class BlockOutOfOrderException extends BlockNotAcceptedException
//	{
//
//		BlockOutOfOrderException(String message)
//		{
//			base(message);
//		}
//
//	}

	}

}