namespace nxt
{

///
/// <summary> * Economic Clustering concept (EC) solves the most critical flaw of "classical" Proof-of-Stake - the problem called
/// * "Nothing-at-Stake".
/// *
/// * I ought to respect BCNext's wish and say that this concept is inspired by Economic Majority idea of Meni Rosenfeld
/// * (http://en.wikipedia.org/wiki/User:Meni_Rosenfeld).
/// *
/// * EC is a vital part of Transparent Forging. Words "Mining in Nxt relies on cooperation of people and even forces it"
/// * (https://bitcointalk.org/index.php?topic=553205.0) were said about EC.
/// *
/// * Keep in mind that this concept has not been peer reviewed. You are very welcome to do it...
/// *
/// *                                                                              Come-from-Beyond (21.05.2014) </summary>
/// 
	public sealed class EconomicClustering
	{

		private static readonly Blockchain blockchain = BlockchainImpl.Instance;

		public static Block getECBlock(int timestamp)
		{
			Block block = blockchain.LastBlock;
			if(timestamp < block.Timestamp - 15)
			{
				throw new System.ArgumentException("Timestamp cannot be more than 15 s earlier than last block timestamp: " + block.Timestamp);
			}
			int distance = 0;
			while(block.Timestamp > timestamp - Constants.EC_RULE_TERMINATOR && distance < Constants.EC_BLOCK_DISTANCE_LIMIT)
			{
				block = blockchain.getBlock(block.PreviousBlockId);
				distance += 1;
			}
			return block;
		}

		public static bool verifyFork(Transaction transaction)
		{
			if(blockchain.Height < Constants.DIGITAL_GOODS_STORE_BLOCK)
			{
				return true;
			}
			if(transaction.ReferencedTransactionFullHash != null)
			{
				return true;
			}
			if(blockchain.Height - transaction.ECBlockHeight > Constants.EC_BLOCK_DISTANCE_LIMIT)
			{
				return false;
			}
			Block ecBlock = blockchain.getBlock(transaction.ECBlockId);
			return ecBlock != null && ecBlock.Height == transaction.ECBlockHeight;
		}

	}

}