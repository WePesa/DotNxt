using System;
using System.Collections.Generic;

namespace nxt
{

	using Crypto = nxt.crypto.Crypto;
	using Db = nxt.db.Db;
	using DbIterator = nxt.db.DbIterator;
	using DerivedDbTable = nxt.db.DerivedDbTable;
	using FilteringIterator = nxt.db.FilteringIterator;
	using Peer = nxt.peer.Peer;
	using Peers = nxt.peer.Peers;
	using Convert = nxt.util.Convert;
	using JSON = nxt.util.JSON;
	using Listener = nxt.util.Listener;
	using Listeners = nxt.util.Listeners;
	using Logger = nxt.util.Logger;
	using ThreadPool = nxt.util.ThreadPool;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;
	using JSONValue = org.json.simple.JSONValue;


	internal sealed class BlockchainProcessorImpl : BlockchainProcessor
	{

		private static readonly sbyte[] CHECKSUM_TRANSPARENT_FORGING = new sbyte[]{27, -54, -59, -98, 49, -42, 48, -68, -112, 49, 41, 94, -41, 78, -84, 27, -87, -22, -28, 36, -34, -90, 112, -50, -9, 5, 89, -35, 80, -121, -128, 112};
		private static readonly sbyte[] CHECKSUM_NQT_BLOCK = Constants.isTestnet ? new sbyte[]{-126, -117, -94, -16, 125, -94, 38, 10, 11, 37, -33, 4, -70, -8, -40, -80, 18, -21, -54, -126, 109, -73, 63, -56, 67, 59, -30, 83, -6, -91, -24, 34} : new sbyte[]{-125, 17, 63, -20, 90, -98, 52, 114, 7, -100, -20, -103, -50, 76, 46, -38, -29, -43, -43, 45, 81, 12, -30, 100, -67, -50, -112, -15, 22, -57, 84, -106};

		private static readonly BlockchainProcessorImpl instance = new BlockchainProcessorImpl();

		static BlockchainProcessorImpl Instance
		{
			get
			{
				return instance;
			}
		}

		private readonly BlockchainImpl blockchain = BlockchainImpl.Instance;

		private readonly IList<DerivedDbTable> derivedTables = new CopyOnWriteArrayList<>();
		private readonly bool trimDerivedTables = Nxt.getBooleanProperty("nxt.trimDerivedTables");
		private volatile int lastTrimHeight;

		private readonly Listeners<Block, Event> blockListeners = new Listeners<>();
		private volatile Peer lastBlockchainFeeder;
		private volatile int lastBlockchainFeederHeight;
		private volatile bool getMoreBlocks = true;

		private volatile bool isScanning;
		private volatile bool forceScan = Nxt.getBooleanProperty("nxt.forceScan");
		private volatile bool validateAtScan = Nxt.getBooleanProperty("nxt.forceValidate");

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private final Runnable getMoreBlocksThread = new Runnable()
//	{
//
//		private final JSONStreamAware getCumulativeDifficultyRequest;
//
//		{
//			JSONObject request = new JSONObject();
//			request.put("requestType", "getCumulativeDifficulty");
//			getCumulativeDifficultyRequest = JSON.prepareRequest(request);
//		}
//
//		private boolean peerHasMore;
//
//		@Override public void run()
//		{
//
//			try
//			{
//				try
//				{
//					if (!getMoreBlocks)
//					{
//						return;
//					}
//					peerHasMore = true;
//					Peer peer = Peers.getAnyPeer(Peer.State.CONNECTED, true);
//					if (peer == null)
//					{
//						return;
//					}
//					JSONObject response = peer.send(getCumulativeDifficultyRequest);
//					if (response == null)
//					{
//						return;
//					}
//					BigInteger curCumulativeDifficulty = blockchain.getLastBlock().getCumulativeDifficulty();
//					String peerCumulativeDifficulty = (String) response.get("cumulativeDifficulty");
//					if (peerCumulativeDifficulty == null)
//					{
//						return;
//					}
//					BigInteger betterCumulativeDifficulty = new BigInteger(peerCumulativeDifficulty);
//					if (betterCumulativeDifficulty.compareTo(curCumulativeDifficulty) < 0)
//					{
//						return;
//					}
//					if (response.get("blockchainHeight") != null)
//					{
//						lastBlockchainFeeder = peer;
//						lastBlockchainFeederHeight = ((Long) response.get("blockchainHeight")).intValue();
//					}
//					if (betterCumulativeDifficulty.equals(curCumulativeDifficulty))
//					{
//						return;
//					}
//
//					long commonBlockId = Genesis.GENESIS_BLOCK_ID;
//
//					if (blockchain.getLastBlock().getId() != Genesis.GENESIS_BLOCK_ID)
//					{
//						commonBlockId = getCommonMilestoneBlockId(peer);
//					}
//					if (commonBlockId == 0 || !peerHasMore)
//					{
//						return;
//					}
//
//					commonBlockId = getCommonBlockId(peer, commonBlockId);
//					if (commonBlockId == 0 || !peerHasMore)
//					{
//						return;
//					}
//
//					final Block commonBlock = BlockDb.findBlock(commonBlockId);
//					if (commonBlock == null || blockchain.getHeight() - commonBlock.getHeight() >= 720)
//					{
//						return;
//					}
//
//					long currentBlockId = commonBlockId;
//					List<BlockImpl> forkBlocks = new ArrayList<>();
//
//					boolean processedAll = true;
//					int requestCount = 0;
//					outer:
//					while (forkBlocks.size() < 1440 && requestCount++ < 10)
//					{
//						JSONArray nextBlocks = getNextBlocks(peer, currentBlockId);
//						if (nextBlocks == null || nextBlocks.size() == 0)
//						{
//							break;
//						}
//
//						synchronized (blockchain)
//						{
//
//							for (Object o : nextBlocks)
//							{
//								JSONObject blockData = (JSONObject) o;
//								BlockImpl block;
//								try
//								{
//									block = BlockImpl.parseBlock(blockData);
//								}
//								catch (NxtException.NotCurrentlyValidException e)
//								{
//									Logger.logDebugMessage("Cannot validate block: " + e.toString() + ", will try again later", e);
//									processedAll = false;
//									break outer;
//								}
//								catch (RuntimeException | NxtException.ValidationException e)
//								{
//									Logger.logDebugMessage("Failed to parse block: " + e.toString(), e);
//									peer.blacklist(e);
//									return;
//								}
//								currentBlockId = block.getId();
//
//								if (blockchain.getLastBlock().getId() == block.getPreviousBlockId())
//								{
//									try
//									{
//										pushBlock(block);
//									}
//									catch (BlockNotAcceptedException e)
//									{
//										peer.blacklist(e);
//										return;
//									}
//								}
//								else if (!BlockDb.hasBlock(block.getId()))
//								{
//									forkBlocks.add(block);
//								}
//
//							}
//
//						} //synchronized
//
//					}
//
//					if (forkBlocks.size() > 0)
//					{
//						processedAll = false;
//					}
//
//					if (!processedAll && blockchain.getHeight() - commonBlock.getHeight() < 720)
//					{
//						processFork(peer, forkBlocks, commonBlock);
//					}
//
//				}
//				catch (NxtException.StopException e)
//				{
//					Logger.logMessage("Blockchain download stopped: " + e.getMessage());
//				}
//				catch (Exception e)
//				{
//					Logger.logDebugMessage("Error in blockchain download thread", e);
//				}
//			}
//			catch (Throwable t)
//			{
//				Logger.logMessage("CRITICAL ERROR. PLEASE REPORT TO THE DEVELOPERS.\n" + t.toString());
//				t.printStackTrace();
//				System.exit(1);
//			}
//
//		}
//
//		private long getCommonMilestoneBlockId(Peer peer)
//		{
//
//			String lastMilestoneBlockId = null;
//
//			while (true)
//			{
//				JSONObject milestoneBlockIdsRequest = new JSONObject();
//				milestoneBlockIdsRequest.put("requestType", "getMilestoneBlockIds");
//				if (lastMilestoneBlockId == null)
//				{
//					milestoneBlockIdsRequest.put("lastBlockId", blockchain.getLastBlock().getStringId());
//				}
//				else
//				{
//					milestoneBlockIdsRequest.put("lastMilestoneBlockId", lastMilestoneBlockId);
//				}
//
//				JSONObject response = peer.send(JSON.prepareRequest(milestoneBlockIdsRequest));
//				if (response == null)
//				{
//					return 0;
//				}
//				JSONArray milestoneBlockIds = (JSONArray) response.get("milestoneBlockIds");
//				if (milestoneBlockIds == null)
//				{
//					return 0;
//				}
//				if (milestoneBlockIds.isEmpty())
//				{
//					return Genesis.GENESIS_BLOCK_ID;
//				}
//				// prevent overloading with blockIds
//				if (milestoneBlockIds.size() > 20)
//				{
//					Logger.logDebugMessage("Obsolete or rogue peer " + peer.getPeerAddress() + " sends too many milestoneBlockIds, blacklisting");
//					peer.blacklist();
//					return 0;
//				}
//				if (Boolean.TRUE.equals(response.get("last")))
//				{
//					peerHasMore = false;
//				}
//				for (Object milestoneBlockId : milestoneBlockIds)
//				{
//					long blockId = Convert.parseUnsignedLong((String) milestoneBlockId);
//					if (BlockDb.hasBlock(blockId))
//					{
//						if (lastMilestoneBlockId == null && milestoneBlockIds.size() > 1)
//						{
//							peerHasMore = false;
//						}
//						return blockId;
//					}
//					lastMilestoneBlockId = (String) milestoneBlockId;
//				}
//			}
//
//		}
//
//		private long getCommonBlockId(Peer peer, long commonBlockId)
//		{
//
//			while (true)
//			{
//				JSONObject request = new JSONObject();
//				request.put("requestType", "getNextBlockIds");
//				request.put("blockId", Convert.toUnsignedLong(commonBlockId));
//				JSONObject response = peer.send(JSON.prepareRequest(request));
//				if (response == null)
//				{
//					return 0;
//				}
//				JSONArray nextBlockIds = (JSONArray) response.get("nextBlockIds");
//				if (nextBlockIds == null || nextBlockIds.size() == 0)
//				{
//					return 0;
//				}
//				// prevent overloading with blockIds
//				if (nextBlockIds.size() > 1440)
//				{
//					Logger.logDebugMessage("Obsolete or rogue peer " + peer.getPeerAddress() + " sends too many nextBlockIds, blacklisting");
//					peer.blacklist();
//					return 0;
//				}
//
//				for (Object nextBlockId : nextBlockIds)
//				{
//					long blockId = Convert.parseUnsignedLong((String) nextBlockId);
//					if (! BlockDb.hasBlock(blockId))
//					{
//						return commonBlockId;
//					}
//					commonBlockId = blockId;
//				}
//			}
//
//		}
//
//		private JSONArray getNextBlocks(Peer peer, long curBlockId)
//		{
//
//			JSONObject request = new JSONObject();
//			request.put("requestType", "getNextBlocks");
//			request.put("blockId", Convert.toUnsignedLong(curBlockId));
//			JSONObject response = peer.send(JSON.prepareRequest(request));
//			if (response == null)
//			{
//				return null;
//			}
//
//			JSONArray nextBlocks = (JSONArray) response.get("nextBlocks");
//			if (nextBlocks == null)
//			{
//				return null;
//			}
//			// prevent overloading with blocks
//			if (nextBlocks.size() > 1440)
//			{
//				Logger.logDebugMessage("Obsolete or rogue peer " + peer.getPeerAddress() + " sends too many nextBlocks, blacklisting");
//				peer.blacklist();
//				return null;
//			}
//
//			return nextBlocks;
//
//		}
//
//		private void processFork(Peer peer, final List<BlockImpl> forkBlocks, final Block commonBlock)
//		{
//
//			synchronized (blockchain)
//			{
//				BigInteger curCumulativeDifficulty = blockchain.getLastBlock().getCumulativeDifficulty();
//
//				List<BlockImpl> myPoppedOffBlocks = popOffTo(commonBlock);
//
//				int pushedForkBlocks = 0;
//				if (blockchain.getLastBlock().getId() == commonBlock.getId())
//				{
//					for (BlockImpl block : forkBlocks)
//					{
//						if (blockchain.getLastBlock().getId() == block.getPreviousBlockId())
//						{
//							try
//							{
//								pushBlock(block);
//								pushedForkBlocks += 1;
//							}
//							catch (BlockNotAcceptedException e)
//							{
//								peer.blacklist(e);
//								break;
//							}
//						}
//					}
//				}
//
//				if (pushedForkBlocks > 0 && blockchain.getLastBlock().getCumulativeDifficulty().compareTo(curCumulativeDifficulty) < 0)
//				{
//					Logger.logDebugMessage("Pop off caused by peer " + peer.getPeerAddress() + ", blacklisting");
//					peer.blacklist();
//					List<BlockImpl> peerPoppedOffBlocks = popOffTo(commonBlock);
//					pushedForkBlocks = 0;
//					for (BlockImpl block : peerPoppedOffBlocks)
//					{
//						TransactionProcessorImpl.getInstance().processLater(block.getTransactions());
//					}
//				}
//
//				if (pushedForkBlocks == 0)
//				{
//					for (int i = myPoppedOffBlocks.size() - 1; i >= 0; i--)
//					{
//						BlockImpl block = myPoppedOffBlocks.remove(i);
//						try
//						{
//							pushBlock(block);
//						}
//						catch (BlockNotAcceptedException e)
//						{
//							Logger.logErrorMessage("Popped off block no longer acceptable: " + block.getJSONObject().toJSONString(), e);
//							break;
//						}
//					}
//				}
//				else
//				{
//					for (BlockImpl block : myPoppedOffBlocks)
//					{
//						TransactionProcessorImpl.getInstance().processLater(block.getTransactions());
//					}
//				}
//
//			} // synchronized
//
//		}
//
//	};

		private BlockchainProcessorImpl()
		{

			blockListeners.addListener(new Listener<Block>() { public void notify(Block block) { if(block.Height % 5000 == 0) { Logger.logMessage("processed block " + block.Height); } } }, Event.BLOCK_SCANNED);

			blockListeners.addListener(new Listener<Block>() { public void notify(Block block) { if(block.Height % 5000 == 0) { Logger.logMessage("received block " + block.Height); Db.analyzeTables(); } } }, Event.BLOCK_PUSHED);

			if(trimDerivedTables)
			{
				blockListeners.addListener(new Listener<Block>() { public void notify(Block block) { if(block.Height % 1440 == 0) { lastTrimHeight = Math.Max(block.Height - Constants.MAX_ROLLBACK, 0); if(lastTrimHeight > 0) { for(DerivedDbTable table : derivedTables) { table.Trim(lastTrimHeight); } } } } }, Event.AFTER_BLOCK_APPLY);
			}

			blockListeners.addListener(new Listener<Block>() { public void notify(Block block) { Db.analyzeTables(); } }, Event.RESCAN_END);

			ThreadPool.runBeforeStart(new Runnable() { public void run() { addGenesisBlock(); if(forceScan) { scan(0); } } }, false);

			ThreadPool.scheduleThread("GetMoreBlocks", getMoreBlocksThread, 1);

		}

		public override bool addListener(Listener<Block> listener, BlockchainProcessor.Event eventType)
		{
			return blockListeners.addListener(listener, eventType);
		}

		public override bool removeListener(Listener<Block> listener, Event eventType)
		{
			return blockListeners.removeListener(listener, eventType);
		}

		public override void registerDerivedTable(DerivedDbTable table)
		{
			derivedTables.Add(table);
		}

		public override Peer LastBlockchainFeeder
		{
			get
			{
				return lastBlockchainFeeder;
			}
		}

		public override int LastBlockchainFeederHeight
		{
			get
			{
				return lastBlockchainFeederHeight;
			}
		}

		public override bool isScanning()
		{
			get
			{
				return isScanning;
			}
		}

		public override int MinRollbackHeight
		{
			get
			{
				return trimDerivedTables ? (lastTrimHeight > 0 ? lastTrimHeight : Math.Max(blockchain.Height - Constants.MAX_ROLLBACK, 0)) : 0;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processPeerBlock(JSONObject request) throws NxtException
		public override void processPeerBlock(JSONObject request)
		{
			BlockImpl block = BlockImpl.parseBlock(request);
			pushBlock(block);
		}

		public override IList<BlockImpl> popOffTo(int height)
		{
			return popOffTo(blockchain.getBlockAtHeight(height));
		}

		public override void fullReset()
		{
			lock (blockchain)
			{
			//BlockDb.deleteBlock(Genesis.GENESIS_BLOCK_ID); // fails with stack overflow in H2
				BlockDb.deleteAll();
				addGenesisBlock();
				scan(0);
			}
		}

		public override void forceScanAtStart()
		{
			forceScan = true;
		}

		public override void validateAtNextScan()
		{
			validateAtScan = true;
		}

		internal bool GetMoreBlocks
		{
			set
			{
				this.getMoreBlocks = value;
			}
		}

		private void addBlock(BlockImpl block)
		{
			using (Connection con = Db.Connection)
			{
				BlockDb.saveBlock(con, block);
				blockchain.LastBlock = block;
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		private void addGenesisBlock()
		{
			if(BlockDb.hasBlock(Genesis.GENESIS_BLOCK_ID))
			{
				Logger.logMessage("Genesis block already in database");
				BlockImpl lastBlock = BlockDb.findLastBlock();
				blockchain.LastBlock = lastBlock;
				Logger.logMessage("Last block height: " + lastBlock.Height);
				return;
			}
			Logger.logMessage("Genesis block not in database, starting from scratch");
			try
			{
				IList<TransactionImpl> transactions = new List<>();
				for(int i = 0; i < Genesis.GENESIS_RECIPIENTS.Length; i++)
				{
					TransactionImpl transaction = new TransactionImpl.BuilderImpl((sbyte) 0, Genesis.CREATOR_PUBLIC_KEY, Genesis.GENESIS_AMOUNTS[i] * Constants.ONE_NXT, 0, 0, (short) 0, Attachment.ORDINARY_PAYMENT).recipientId(Genesis.GENESIS_RECIPIENTS[i]).signature(Genesis.GENESIS_SIGNATURES[i]).height(0).build();
					transactions.Add(transaction);
				}
				Collections.sort(transactions);
				MessageDigest digest = Crypto.sha256();
				foreach (Transaction transaction in transactions)
				{
					digest.update(transaction.Bytes);
				}
				BlockImpl genesisBlock = new BlockImpl(-1, 0, 0, Constants.MAX_BALANCE_NQT, 0, transactions.Count * 128, digest.digest(), Genesis.CREATOR_PUBLIC_KEY, new sbyte[64], Genesis.GENESIS_BLOCK_SIGNATURE, null, transactions);
				genesisBlock.Previous = null;
				addBlock(genesisBlock);
			}
			catch(NxtException.ValidationException e)
			{
				Logger.logMessage(e.Message);
				throw new Exception(e.ToString(), e);
			}
		}

		private sbyte[] calculateTransactionsChecksum()
		{
			MessageDigest digest = Crypto.sha256();
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM transaction ORDER BY id ASC, timestamp ASC"), DbIterator<TransactionImpl> iterator = blockchain.getTransactions(con, pstmt))
			{
				while(iterator.hasNext())
				{
					digest.update(iterator.next().Bytes);
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			return digest.digest();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void pushBlock(final BlockImpl block) throws BlockNotAcceptedException
//JAVA TO VB & C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
		private void pushBlock(BlockImpl block)
		{

			int curTime = Nxt.EpochTime;

			lock (blockchain)
			{
				TransactionProcessorImpl transactionProcessor = TransactionProcessorImpl.Instance;
				BlockImpl previousLastBlock = null;
				try
				{
					Db.beginTransaction();
					previousLastBlock = blockchain.LastBlock;

					if(previousLastBlock.Id != block.PreviousBlockId)
					{
						throw new BlockOutOfOrderException("Previous block id doesn't match");
					}

					if(block.Version != getBlockVersion(previousLastBlock.Height))
					{
						throw new BlockNotAcceptedException("Invalid version " + block.Version);
					}

					if(previousLastBlock.Height == Constants.TRANSPARENT_FORGING_BLOCK)
					{
						sbyte[] checksum = calculateTransactionsChecksum();
						if(CHECKSUM_TRANSPARENT_FORGING == null)
						{
							Logger.logMessage("Checksum calculated:\n" + Arrays.ToString(checksum));
						}
						else if(!Array.Equals(checksum, CHECKSUM_TRANSPARENT_FORGING))
						{
							Logger.logMessage("Checksum failed at block " + Constants.TRANSPARENT_FORGING_BLOCK);
							throw new BlockNotAcceptedException("Checksum failed");
						}
						else
						{
							Logger.logMessage("Checksum passed at block " + Constants.TRANSPARENT_FORGING_BLOCK);
						}
					}

					if(previousLastBlock.Height == Constants.NQT_BLOCK)
					{
						sbyte[] checksum = calculateTransactionsChecksum();
						if(CHECKSUM_NQT_BLOCK == null)
						{
							Logger.logMessage("Checksum calculated:\n" + Arrays.ToString(checksum));
						}
						else if(!Array.Equals(checksum, CHECKSUM_NQT_BLOCK))
						{
							Logger.logMessage("Checksum failed at block " + Constants.NQT_BLOCK);
							throw new BlockNotAcceptedException("Checksum failed");
						}
						else
						{
							Logger.logMessage("Checksum passed at block " + Constants.NQT_BLOCK);
						}
					}

					if(block.Version != 1 && !Array.Equals(Crypto.sha256().digest(previousLastBlock.Bytes), block.PreviousBlockHash))
					{
						throw new BlockNotAcceptedException("Previous block hash doesn't match");
					}
					if(block.Timestamp > curTime + 15 || block.Timestamp <= previousLastBlock.Timestamp)
					{
						throw new BlockOutOfOrderException("Invalid timestamp: " + block.Timestamp + " current time is " + curTime + ", previous block timestamp is " + previousLastBlock.Timestamp);
					}
					if(block.Id == 0L || BlockDb.hasBlock(block.Id))
					{
						throw new BlockNotAcceptedException("Duplicate block or invalid id");
					}
					if(!block.verifyGenerationSignature() && !Generator.allowsFakeForging(block.GeneratorPublicKey))
					{
						throw new BlockNotAcceptedException("Generation signature verification failed");
					}
					if(!block.verifyBlockSignature())
					{
						throw new BlockNotAcceptedException("Block signature verification failed");
					}

					IDictionary<TransactionType, Set<string>> duplicates = new Dictionary<>();
					long calculatedTotalAmount = 0;
					long calculatedTotalFee = 0;
					MessageDigest digest = Crypto.sha256();

					foreach (TransactionImpl transaction in block.Transactions)
					{

						if(transaction.Timestamp > curTime + 15)
						{
							throw new BlockOutOfOrderException("Invalid transaction timestamp: " + transaction.Timestamp + ", current time is " + curTime);
						}
					// cfb: Block 303 contains a transaction which expired before the block timestamp
						if(transaction.Timestamp > block.Timestamp + 15 || (transaction.Expiration < block.Timestamp && previousLastBlock.Height != 303))
						{
							throw new TransactionNotAcceptedException("Invalid transaction timestamp " + transaction.Timestamp + " for transaction " + transaction.StringId + ", current time is " + curTime + ", block timestamp is " + block.Timestamp, transaction);
						}
						if(TransactionDb.hasTransaction(transaction.Id))
						{
							throw new TransactionNotAcceptedException("Transaction " + transaction.StringId + " is already in the blockchain", transaction);
						}
						if(transaction.ReferencedTransactionFullHash != null)
						{
							if((previousLastBlock.Height < Constants.REFERENCED_TRANSACTION_FULL_HASH_BLOCK && !TransactionDb.hasTransaction(Convert.fullHashToId(transaction.ReferencedTransactionFullHash))) || (previousLastBlock.Height >= Constants.REFERENCED_TRANSACTION_FULL_HASH_BLOCK && !hasAllReferencedTransactions(transaction, transaction.Timestamp, 0)))
							{
								throw new TransactionNotAcceptedException("Missing or invalid referenced transaction " + transaction.ReferencedTransactionFullHash + " for transaction " + transaction.StringId, transaction);
							}
						}
						if(transaction.Version != transactionProcessor.getTransactionVersion(previousLastBlock.Height))
						{
							throw new TransactionNotAcceptedException("Invalid transaction version " + transaction.Version + " at height " + previousLastBlock.Height, transaction);
						}
						if(!transaction.verifySignature())
						{
							throw new TransactionNotAcceptedException("Signature verification failed for transaction " + transaction.StringId + " at height " + previousLastBlock.Height, transaction);
						}
//                    
//                    if (!EconomicClustering.verifyFork(transaction)) {
//                        Logger.logDebugMessage("Block " + block.getStringId() + " height " + (previousLastBlock.getHeight() + 1)
//                                + " contains transaction that was generated on a fork: "
//                                + transaction.getStringId() + " ecBlockHeight " + transaction.getECBlockHeight() + " ecBlockId "
//                                + Convert.toUnsignedLong(transaction.getECBlockId()));
//                        //throw new TransactionNotAcceptedException("Transaction belongs to a different fork", transaction);
//                    }
//                    
						if(transaction.Id == 0L)
						{
							throw new TransactionNotAcceptedException("Invalid transaction id", transaction);
						}
						if(transaction.isDuplicate(duplicates))
						{
							throw new TransactionNotAcceptedException("Transaction is a duplicate: " + transaction.StringId, transaction);
						}
						try
						{
							transaction.validate();
						}
						catch(NxtException.ValidationException e)
						{
							throw new TransactionNotAcceptedException(e.Message, transaction);
						}

						calculatedTotalAmount += transaction.AmountNQT;

						calculatedTotalFee += transaction.FeeNQT;

						digest.update(transaction.Bytes);

					}

					if(calculatedTotalAmount != block.TotalAmountNQT || calculatedTotalFee != block.TotalFeeNQT)
					{
						throw new BlockNotAcceptedException("Total amount or fee don't match transaction totals");
					}
					if(!Array.Equals(digest.digest(), block.PayloadHash))
					{
						throw new BlockNotAcceptedException("Payload hash doesn't match");
					}

					block.Previous = previousLastBlock;
					blockListeners.notify(block, Event.BEFORE_BLOCK_ACCEPT);
					transactionProcessor.requeueAllUnconfirmedTransactions();
					addBlock(block);
					accept(block);

					Db.commitTransaction();
				}
				catch(Exception e)
				{
					Db.rollbackTransaction();
					blockchain.LastBlock = previousLastBlock;
					throw e;
				}
				finally
				{
					Db.endTransaction();
				}
			} // synchronized

			blockListeners.notify(block, Event.BLOCK_PUSHED);

			if(block.Timestamp >= Nxt.EpochTime - 15)
			{
				Peers.sendToSomePeers(block);
			}

		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void accept(BlockImpl block) throws TransactionNotAcceptedException
		private void accept(BlockImpl block)
		{
			TransactionProcessorImpl transactionProcessor = TransactionProcessorImpl.Instance;
			foreach (TransactionImpl transaction in block.Transactions)
			{
				if(! transaction.applyUnconfirmed())
				{
					throw new TransactionNotAcceptedException("Double spending transaction: " + transaction.StringId, transaction);
				}
			}
			blockListeners.notify(block, Event.BEFORE_BLOCK_APPLY);
			block.apply();
			blockListeners.notify(block, Event.AFTER_BLOCK_APPLY);
			if(block.Transactions.Count > 0)
			{
				transactionProcessor.notifyListeners(block.Transactions, TransactionProcessor.Event.ADDED_CONFIRMED_TRANSACTIONS);
			}
		}

		private IList<BlockImpl> popOffTo(Block commonBlock)
		{
			lock (blockchain)
			{
				if(commonBlock.Height < MinRollbackHeight)
				{
					throw new System.ArgumentException("Rollback to height " + commonBlock.Height + " not suppported, " + "current height " + Nxt.Blockchain.Height);
				}
				if(! blockchain.hasBlock(commonBlock.Id))
				{
					Logger.logDebugMessage("Block " + commonBlock.StringId + " not found in blockchain, nothing to pop off");
					return Collections.emptyList();
				}
				IList<BlockImpl> poppedOffBlocks = new List<>();
				try
				{
					Db.beginTransaction();
					BlockImpl block = blockchain.LastBlock;
					Logger.logDebugMessage("Rollback from " + block.Height + " to " + commonBlock.Height);
					while(block.Id != commonBlock.Id && block.Id != Genesis.GENESIS_BLOCK_ID)
					{
						poppedOffBlocks.Add(block);
						block = popLastBlock();
					}
					foreach (DerivedDbTable table in derivedTables)
					{
						table.rollback(commonBlock.Height);
					}
					Db.commitTransaction();
				}
				catch(Exception e)
				{
					Db.rollbackTransaction();
					Logger.logDebugMessage("Error popping off to " + commonBlock.Height, e);
					throw e;
				}
				finally
				{
					Db.endTransaction();
				}
				return poppedOffBlocks;
			} // synchronized
		}

		private BlockImpl popLastBlock()
		{
			BlockImpl block = blockchain.LastBlock;
			if(block.Id == Genesis.GENESIS_BLOCK_ID)
			{
				throw new Exception("Cannot pop off genesis block");
			}
			BlockImpl previousBlock = BlockDb.findBlock(block.PreviousBlockId);
			blockchain.setLastBlock(block, previousBlock);
			foreach (TransactionImpl transaction in block.Transactions)
			{
				transaction.unsetBlock();
			}
			BlockDb.deleteBlocksFrom(block.Id);
			blockListeners.notify(block, Event.BLOCK_POPPED);
			return previousBlock;
		}

		internal int getBlockVersion(int previousBlockHeight)
		{
			return previousBlockHeight < Constants.TRANSPARENT_FORGING_BLOCK ? 1 : previousBlockHeight < Constants.NQT_BLOCK ? 2 : 3;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void generateBlock(String secretPhrase, int blockTimestamp) throws BlockNotAcceptedException
		internal void generateBlock(string secretPhrase, int blockTimestamp)
		{

			TransactionProcessorImpl transactionProcessor = TransactionProcessorImpl.Instance;
			IList<TransactionImpl> orderedUnconfirmedTransactions = new List<>();
			using (FilteringIterator<TransactionImpl> transactions = new FilteringIterator<>(transactionProcessor.AllUnconfirmedTransactions, new FilteringIterator.Filter<TransactionImpl>() { public bool ok(TransactionImpl transaction) { return hasAllReferencedTransactions(transaction, transaction.Timestamp, 0); } }))
			{
				foreach (TransactionImpl transaction in transactions)
				{
					orderedUnconfirmedTransactions.Add(transaction);
				}
			}

			BlockImpl previousBlock = blockchain.LastBlock;

			SortedSet<TransactionImpl> blockTransactions = new TreeSet<>();

			IDictionary<TransactionType, Set<string>> duplicates = new Dictionary<>();

			long totalAmountNQT = 0;
			long totalFeeNQT = 0;
			int payloadLength = 0;

			while(payloadLength <= Constants.MAX_PAYLOAD_LENGTH && blockTransactions.size() <= Constants.MAX_NUMBER_OF_TRANSACTIONS)
			{

				int prevNumberOfNewTransactions = blockTransactions.size();

				foreach (TransactionImpl transaction in orderedUnconfirmedTransactions)
				{

					int transactionLength = transaction.Size;
					if(blockTransactions.contains(transaction) || payloadLength + transactionLength > Constants.MAX_PAYLOAD_LENGTH)
					{
						continue;
					}

					if(transaction.Version != transactionProcessor.getTransactionVersion(previousBlock.Height))
					{
						continue;
					}

					if(transaction.Timestamp > blockTimestamp + 15 || transaction.Expiration < blockTimestamp)
					{
						continue;
					}

					if(transaction.isDuplicate(duplicates))
					{
						continue;
					}

					try
					{
						transaction.validate();
					}
					catch(NxtException.NotCurrentlyValidException e)
					{
						continue;
					}
					catch(NxtException.ValidationException e)
					{
						transactionProcessor.removeUnconfirmedTransaction(transaction);
						continue;
					}

//                
//                if (!EconomicClustering.verifyFork(transaction)) {
//                    Logger.logDebugMessage("Including transaction that was generated on a fork: " + transaction.getStringId()
//                            + " ecBlockHeight " + transaction.getECBlockHeight() + " ecBlockId " + Convert.toUnsignedLong(transaction.getECBlockId()));
//                    //continue;
//                }
//                

					blockTransactions.add(transaction);
					payloadLength += transactionLength;
					totalAmountNQT += transaction.AmountNQT;
					totalFeeNQT += transaction.FeeNQT;

				}

				if(blockTransactions.size() == prevNumberOfNewTransactions)
				{
					break;
				}
			}

//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] publicKey = Crypto.getPublicKey(secretPhrase);
			sbyte[] publicKey = Crypto.getPublicKey(secretPhrase);

			MessageDigest digest = Crypto.sha256();
			foreach (Transaction transaction in blockTransactions)
			{
				digest.update(transaction.Bytes);
			}

			sbyte[] payloadHash = digest.digest();

			digest.update(previousBlock.GenerationSignature);
			sbyte[] generationSignature = digest.digest(publicKey);

			BlockImpl block;
			sbyte[] previousBlockHash = Crypto.sha256().digest(previousBlock.Bytes);

			try
			{

				block = new BlockImpl(getBlockVersion(previousBlock.Height), blockTimestamp, previousBlock.Id, totalAmountNQT, totalFeeNQT, payloadLength, payloadHash, publicKey, generationSignature, null, previousBlockHash, new List<>(blockTransactions));

			}
			catch(NxtException.ValidationException e)
			{
			// shouldn't happen because all transactions are already validated
				Logger.logMessage("Error generating block", e);
				return;
			}

			block.sign(secretPhrase);

			block.Previous = previousBlock;

			try
			{
				pushBlock(block);
				blockListeners.notify(block, Event.BLOCK_GENERATED);
				Logger.logDebugMessage("Account " + Convert.toUnsignedLong(block.GeneratorId) + " generated block " + block.StringId + " at height " + block.Height);
			}
			catch(TransactionNotAcceptedException e)
			{
				Logger.logDebugMessage("Generate block failed: " + e.Message);
				Transaction transaction = e.Transaction;
				Logger.logDebugMessage("Removing invalid transaction: " + transaction.StringId);
				transactionProcessor.removeUnconfirmedTransaction((TransactionImpl) transaction);
				throw e;
			}
			catch(BlockNotAcceptedException e)
			{
				Logger.logDebugMessage("Generate block failed: " + e.Message);
				throw e;
			}
		}

		private bool hasAllReferencedTransactions(Transaction transaction, int timestamp, int count)
		{
			if(transaction.ReferencedTransactionFullHash == null)
			{
				return timestamp - transaction.Timestamp < 60 * 1440 * 60 && count < 10;
			}
			transaction = TransactionDb.findTransactionByFullHash(transaction.ReferencedTransactionFullHash);
			return transaction != null && hasAllReferencedTransactions(transaction, timestamp, count + 1);
		}

		public override void scan(int height)
		{
			lock (blockchain)
			{
				TransactionProcessorImpl transactionProcessor = TransactionProcessorImpl.Instance;
				int blockchainHeight = Nxt.Blockchain.Height;
				if(height > blockchainHeight + 1)
				{
					throw new System.ArgumentException("Rollback height " + (height - 1) + " exceeds current blockchain height of " + blockchainHeight);
				}
				if(height > 0 && height < MinRollbackHeight)
				{
					Logger.logMessage("Rollback of more than " + Constants.MAX_ROLLBACK + " blocks not supported, will do a full scan");
					height = 0;
				}
				if(height < 0)
				{
					height = 0;
				}
				isScanning = true;
				Logger.logMessage("Scanning blockchain starting from height " + height + "...");
				if(validateAtScan)
				{
					Logger.logDebugMessage("Also verifying signatures and validating transactions...");
				}
				using (Connection con = Db.beginTransaction(), PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block WHERE height >= ? ORDER BY db_id ASC"))
				{
					transactionProcessor.requeueAllUnconfirmedTransactions();
					foreach (DerivedDbTable table in derivedTables)
					{
						if(height == 0)
						{
							table.truncate();
						}
						else
						{
							table.rollback(height - 1);
						}
					}
					pstmt.setInt(1, height);
					using (ResultSet rs = pstmt.executeQuery())
					{
						BlockImpl currentBlock = BlockDb.findBlockAtHeight(height);
						blockListeners.notify(currentBlock, Event.RESCAN_BEGIN);
						long currentBlockId = currentBlock.Id;
						if(height == 0)
						{
							blockchain.LastBlock = currentBlock; // special case to avoid no last block
							Account.addOrGetAccount(Genesis.CREATOR_ID).apply(Genesis.CREATOR_PUBLIC_KEY, 0);
						}
						else
						{
							blockchain.LastBlock = BlockDb.findBlockAtHeight(height - 1);
						}
						while(rs.next())
						{
							try
							{
								currentBlock = BlockDb.loadBlock(con, rs);
								if(currentBlock.Id != currentBlockId)
								{
									throw new NxtException.NotValidException("Database blocks in the wrong order!");
								}
								if(validateAtScan && currentBlockId != Genesis.GENESIS_BLOCK_ID)
								{
									if(!currentBlock.verifyBlockSignature())
									{
										throw new NxtException.NotValidException("Invalid block signature");
									}
									if(!currentBlock.verifyGenerationSignature() && !Generator.allowsFakeForging(currentBlock.GeneratorPublicKey))
									{
										throw new NxtException.NotValidException("Invalid block generation signature");
									}
									if(currentBlock.Version != getBlockVersion(blockchain.Height))
									{
										throw new NxtException.NotValidException("Invalid block version");
									}
									sbyte[] blockBytes = currentBlock.Bytes;
									JSONObject blockJSON = (JSONObject) JSONValue.parse(currentBlock.JSONObject.toJSONString());
									if(!Array.Equals(blockBytes, BlockImpl.parseBlock(blockJSON).Bytes))
									{
										throw new NxtException.NotValidException("Block JSON cannot be parsed back to the same block");
									}
									foreach (TransactionImpl transaction in currentBlock.Transactions)
									{
										if(!transaction.verifySignature())
										{
											throw new NxtException.NotValidException("Invalid transaction signature");
										}
										if(transaction.Version != transactionProcessor.getTransactionVersion(blockchain.Height))
										{
											throw new NxtException.NotValidException("Invalid transaction version");
										}
//                                    
//                                    if (!EconomicClustering.verifyFork(transaction)) {
//                                        Logger.logDebugMessage("Found transaction that was generated on a fork: " + transaction.getStringId()
//                                                + " in block " + currentBlock.getStringId() + " at height " + currentBlock.getHeight()
//                                                + " ecBlockHeight " + transaction.getECBlockHeight() + " ecBlockId " + Convert.toUnsignedLong(transaction.getECBlockId()));
//                                        //throw new NxtException.NotValidException("Invalid transaction fork");
//                                    }
//                                    
										transaction.validate();
										sbyte[] transactionBytes = transaction.Bytes;
										if(currentBlock.Height > Constants.NQT_BLOCK && !Array.Equals(transactionBytes, transactionProcessor.parseTransaction(transactionBytes).Bytes))
										{
											throw new NxtException.NotValidException("Transaction bytes cannot be parsed back to the same transaction");
										}
										JSONObject transactionJSON = (JSONObject) JSONValue.parse(transaction.JSONObject.toJSONString());
										if(!Array.Equals(transactionBytes, transactionProcessor.parseTransaction(transactionJSON).Bytes))
										{
											throw new NxtException.NotValidException("Transaction JSON cannot be parsed back to the same transaction");
										}
									}
								}
								blockListeners.notify(currentBlock, Event.BEFORE_BLOCK_ACCEPT);
								blockchain.LastBlock = currentBlock;
								accept(currentBlock);
								currentBlockId = currentBlock.NextBlockId;
								Db.commitTransaction();
							}
							catch(NxtException | Exception e)
							{
								Db.rollbackTransaction();
								Logger.logDebugMessage(e.ToString(), e);
								Logger.logDebugMessage("Applying block " + Convert.toUnsignedLong(currentBlockId) + " at height " + (currentBlock == null ? 0 : currentBlock.Height) + " failed, deleting from database");
								if(currentBlock != null)
								{
									transactionProcessor.processLater(currentBlock.Transactions);
								}
								while(rs.next())
								{
									try
									{
										currentBlock = BlockDb.loadBlock(con, rs);
										transactionProcessor.processLater(currentBlock.Transactions);
									}
									catch(NxtException.ValidationException ignore)
									{
									}
								}
								BlockDb.deleteBlocksFrom(currentBlockId);
								blockchain.LastBlock = BlockDb.findLastBlock();
							}
							blockListeners.notify(currentBlock, Event.BLOCK_SCANNED);
						}
						Db.endTransaction();
						blockListeners.notify(currentBlock, Event.RESCAN_END);
					}
				}
				catch(SQLException e)
				{
					throw new Exception(e.ToString(), e);
				}
				validateAtScan = false;
				Logger.logMessage("...done at height " + Nxt.Blockchain.Height);
				isScanning = false;
			} // synchronized
		}

	}

}