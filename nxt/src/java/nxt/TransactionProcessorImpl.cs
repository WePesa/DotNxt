using System;
using System.Collections.Generic;

namespace nxt
{

	using Db = nxt.db.Db;
	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using EntityDbTable = nxt.db.EntityDbTable;
	using Peer = nxt.peer.Peer;
	using Peers = nxt.peer.Peers;
	using JSON = nxt.util.JSON;
	using Listener = nxt.util.Listener;
	using Listeners = nxt.util.Listeners;
	using Logger = nxt.util.Logger;
	using ThreadPool = nxt.util.ThreadPool;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;


	internal sealed class TransactionProcessorImpl : TransactionProcessor
	{

		private const bool enableTransactionRebroadcasting = Nxt.getBooleanProperty("nxt.enableTransactionRebroadcasting");
		private const bool testUnconfirmedTransactions = Nxt.getBooleanProperty("nxt.testUnconfirmedTransactions");

		private static readonly TransactionProcessorImpl instance = new TransactionProcessorImpl();

		static TransactionProcessorImpl Instance
		{
			get
			{
				return instance;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final DbKey.LongKeyFactory<TransactionImpl> unconfirmedTransactionDbKeyFactory = new DbKey.LongKeyFactory<TransactionImpl>("id")
		DbKey.LongKeyFactory<TransactionImpl> unconfirmedTransactionDbKeyFactory = new DbKey.LongKeyFactory<TransactionImpl>("id");
		{

			public DbKey newKey(TransactionImpl transaction)
			{
				return transaction.DbKey;
			}

		}

		private final EntityDbTable<TransactionImpl> unconfirmedTransactionTable = new EntityDbTable<TransactionImpl>("unconfirmed_transaction", unconfirmedTransactionDbKeyFactory)
		{

			protected TransactionImpl load(Connection con, ResultSet rs) throws SQLException
			{
				sbyte[] transactionBytes = rs.getBytes("transaction_bytes");
				try
				{
					TransactionImpl transaction = TransactionImpl.parseTransaction(transactionBytes);
					transaction.Height = rs.getInt("transaction_height");
					return transaction;
				}
				catch(NxtException.ValidationException e)
				{
					throw new Exception(e.ToString(), e);
				}
			}

			protected void save(Connection con, TransactionImpl transaction) throws SQLException
			{
				using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO unconfirmed_transaction (id, transaction_height, " + "fee_per_byte, timestamp, expiration, transaction_bytes, height) " + "VALUES (?, ?, ?, ?, ?, ?, ?)"))
				{
					int i = 0;
					pstmt.setLong(++i, transaction.Id);
					pstmt.setInt(++i, transaction.Height);
					pstmt.setLong(++i, transaction.FeeNQT / transaction.Size);
					pstmt.setInt(++i, transaction.Timestamp);
					pstmt.setInt(++i, transaction.Expiration);
					pstmt.setBytes(++i, transaction.Bytes);
					pstmt.setInt(++i, Nxt.Blockchain.Height);
					pstmt.executeUpdate();
				}
			}

			public void rollback(int height)
			{
				IList<TransactionImpl> transactions = new List<>();
				using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM unconfirmed_transaction WHERE height > ?"))
				{
					pstmt.setInt(1, height);
					using (ResultSet rs = pstmt.executeQuery())
					{
						while(rs.next())
						{
							transactions.Add(load(con, rs));
						}
					}
				}
				catch(SQLException e)
				{
					throw new Exception(e.ToString(), e);
				}
				base.rollback(height);
				processLater(transactions);
			}

			protected string defaultSort()
			{
				return " ORDER BY transaction_height ASC, fee_per_byte DESC, timestamp ASC, id ASC ";
			}

		}

		private final Set<TransactionImpl> nonBroadcastedTransactions = Collections.newSetFromMap(new ConcurrentHashMap<TransactionImpl, bool?>());
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: private final Listeners<List<? extends Transaction>,Event> transactionListeners = new Listeners<>();
		private final Listeners<IList<?>, Event> transactionListeners = new Listeners<>();
		private final Set<TransactionImpl> lostTransactions = new HashSet<>();

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private final Runnable removeUnconfirmedTransactionsThread = new Runnable()
//	{
//
//		private final DbClause expiredClause = new DbClause(" expiration < ? ")
//		{
//			@Override protected int set(PreparedStatement pstmt, int index) throws SQLException
//			{
//				pstmt.setInt(index, Nxt.getEpochTime());
//				return index + 1;
//			}
//		};
//
//		@Override public void run()
//		{
//
//			try
//			{
//				try
//				{
//					List<TransactionImpl> expiredTransactions = new ArrayList<>();
//					try (DbIterator<TransactionImpl> iterator = unconfirmedTransactionTable.getManyBy(expiredClause, 0, -1, ""))
//					{
//						while (iterator.hasNext())
//						{
//							expiredTransactions.add(iterator.next());
//						}
//					}
//					if (expiredTransactions.size() > 0)
//					{
//						synchronized (BlockchainImpl.getInstance())
//						{
//							try
//							{
//								Db.beginTransaction();
//								for (TransactionImpl transaction : expiredTransactions)
//								{
//									removeUnconfirmedTransaction(transaction);
//								}
//								Db.commitTransaction();
//							}
//							catch (Exception e)
//							{
//								Logger.logErrorMessage(e.toString(), e);
//								Db.rollbackTransaction();
//								throw e;
//							}
//							finally
//							{
//								Db.endTransaction();
//							}
//						} // synchronized
//					}
//				}
//				catch (Exception e)
//				{
//					Logger.logDebugMessage("Error removing unconfirmed transactions", e);
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
//	};

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private final Runnable rebroadcastTransactionsThread = new Runnable()
//	{
//
//		@Override public void run()
//		{
//
//			try
//			{
//				try
//				{
//					List<Transaction> transactionList = new ArrayList<>();
//					int curTime = Nxt.getEpochTime();
//					for (TransactionImpl transaction : nonBroadcastedTransactions)
//					{
//						if (TransactionDb.hasTransaction(transaction.getId()) || transaction.getExpiration() < curTime)
//						{
//							nonBroadcastedTransactions.remove(transaction);
//						}
//						else if (transaction.getTimestamp() < curTime - 30)
//						{
//							transactionList.add(transaction);
//						}
//					}
//
//					if (transactionList.size() > 0)
//					{
//						Peers.sendToSomePeers(transactionList);
//					}
//
//				}
//				catch (Exception e)
//				{
//					Logger.logDebugMessage("Error in transaction re-broadcasting thread", e);
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
//	};

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private final Runnable processTransactionsThread = new Runnable()
//	{
//
//		private final JSONStreamAware getUnconfirmedTransactionsRequest;
//		{
//			JSONObject request = new JSONObject();
//			request.put("requestType", "getUnconfirmedTransactions");
//			getUnconfirmedTransactionsRequest = JSON.prepareRequest(request);
//		}
//
//		@Override public void run()
//		{
//			try
//			{
//				try
//				{
//					synchronized (BlockchainImpl.getInstance())
//					{
//						processTransactions(lostTransactions, false);
//						lostTransactions.clear();
//					}
//					Peer peer = Peers.getAnyPeer(Peer.State.CONNECTED, true);
//					if (peer == null)
//					{
//						return;
//					}
//					JSONObject response = peer.send(getUnconfirmedTransactionsRequest);
//					if (response == null)
//					{
//						return;
//					}
//					JSONArray transactionsData = (JSONArray)response.get("unconfirmedTransactions");
//					if (transactionsData == null || transactionsData.size() == 0)
//					{
//						return;
//					}
//					try
//					{
//						processPeerTransactions(transactionsData);
//					}
//					catch (NxtException.ValidationException|RuntimeException e)
//					{
//						peer.blacklist(e);
//					}
//				}
//				catch (Exception e)
//				{
//					Logger.logDebugMessage("Error processing unconfirmed transactions", e);
//				}
//			}
//			catch (Throwable t)
//			{
//				Logger.logMessage("CRITICAL ERROR. PLEASE REPORT TO THE DEVELOPERS.\n" + t.toString());
//				t.printStackTrace();
//				System.exit(1);
//			}
//		}
//
//	};

		private TransactionProcessorImpl()
		{
			ThreadPool.scheduleThread("ProcessTransactions", processTransactionsThread, 5);
			ThreadPool.scheduleThread("RemoveUnconfirmedTransactions", removeUnconfirmedTransactionsThread, 1);
			if(enableTransactionRebroadcasting)
			{
				ThreadPool.scheduleThread("RebroadcastTransactions", rebroadcastTransactionsThread, 60);
				ThreadPool.runAfterStart(new Runnable() { public void run() { try (DbIterator<TransactionImpl> oldNonBroadcastedTransactions = AllUnconfirmedTransactions) { for(TransactionImpl transaction : oldNonBroadcastedTransactions) { nonBroadcastedTransactions.add(transaction); } } } });
			}
		}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public boolean addListener(Listener<List<? extends Transaction>> listener, Event eventType)
		public bool addListener(Listener<IList<?>> listener, Event eventType)
		{
			return transactionListeners.addListener(listener, eventType);
		}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public boolean removeListener(Listener<List<? extends Transaction>> listener, Event eventType)
		public bool removeListener(Listener<IList<?>> listener, Event eventType)
		{
			return transactionListeners.removeListener(listener, eventType);
		}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: void notifyListeners(List<? extends Transaction> transactions, Event eventType)
		void notifyListeners(IList<?> transactions, Event eventType)
		{
			transactionListeners.notify(transactions, eventType);
		}

		public DbIterator<TransactionImpl> AllUnconfirmedTransactions
		{
			return unconfirmedTransactionTable.getAll(0, -1);
		}

		public Transaction getUnconfirmedTransaction(long transactionId)
		{
			return unconfirmedTransactionTable.get(unconfirmedTransactionDbKeyFactory.newKey(transactionId));
		}

		public Transaction.Builder newTransactionBuilder(sbyte[] senderPublicKey, long amountNQT, long feeNQT, short deadline, Attachment attachment)
		{
			sbyte version = (sbyte) getTransactionVersion(Nxt.Blockchain.Height);
			int timestamp = Nxt.EpochTime;
			TransactionImpl.BuilderImpl builder = new TransactionImpl.BuilderImpl(version, senderPublicKey, amountNQT, feeNQT, timestamp, deadline, (Attachment.AbstractAttachment)attachment);
			if(version > 0)
			{
				Block ecBlock = EconomicClustering.getECBlock(timestamp);
				builder.ecBlockHeight(ecBlock.Height);
				builder.ecBlockId(ecBlock.Id);
			}
			return builder;
		}

		public void broadcast(Transaction transaction) throws NxtException.ValidationException
		{
			if(! transaction.verifySignature())
			{
				throw new NxtException.NotValidException("Transaction signature verification failed");
			}
			IList<Transaction> processedTransactions;
			lock (BlockchainImpl.Instance)
			{
				if(TransactionDb.hasTransaction(transaction.Id))
				{
					Logger.logMessage("Transaction " + transaction.StringId + " already in blockchain, will not broadcast again");
					return;
				}
				if(unconfirmedTransactionTable.get(((TransactionImpl) transaction).DbKey) != null)
				{
					if(enableTransactionRebroadcasting)
					{
						nonBroadcastedTransactions.add((TransactionImpl) transaction);
						Logger.logMessage("Transaction " + transaction.StringId + " already in unconfirmed pool, will re-broadcast");
					}
					else
					{
						Logger.logMessage("Transaction " + transaction.StringId + " already in unconfirmed pool, will not broadcast again");
					}
					return;
				}
				processedTransactions = processTransactions(Collections.singleton((TransactionImpl) transaction), true);
			}
			if(processedTransactions.Contains(transaction))
			{
				if(enableTransactionRebroadcasting)
				{
					nonBroadcastedTransactions.add((TransactionImpl) transaction);
				}
				Logger.logDebugMessage("Accepted new transaction " + transaction.StringId);
			}
			else
			{
				Logger.logDebugMessage("Could not accept new transaction " + transaction.StringId);
				throw new NxtException.NotValidException("Invalid transaction " + transaction.StringId);
			}
		}

		public void processPeerTransactions(JSONObject request) throws NxtException.ValidationException
		{
			JSONArray transactionsData = (JSONArray)request.get("transactions");
			processPeerTransactions(transactionsData);
		}

		public Transaction parseTransaction(sbyte[] bytes) throws NxtException.ValidationException
		{
			return TransactionImpl.parseTransaction(bytes);
		}

		public TransactionImpl parseTransaction(JSONObject transactionData) throws NxtException.NotValidException
		{
			return TransactionImpl.parseTransaction(transactionData);
		}

		public void clearUnconfirmedTransactions()
		{
			lock (BlockchainImpl.Instance)
			{
				IList<Transaction> removed = new List<>();
				try
				{
					Db.beginTransaction();
					using (DbIterator<TransactionImpl> unconfirmedTransactions = AllUnconfirmedTransactions)
					{
						foreach (TransactionImpl transaction in unconfirmedTransactions)
						{
							transaction.undoUnconfirmed();
							removed.Add(transaction);
						}
					}
					unconfirmedTransactionTable.truncate();
					Db.commitTransaction();
				}
				catch(Exception e)
				{
					Logger.logErrorMessage(e.ToString(), e);
					Db.rollbackTransaction();
					throw e;
				}
				finally
				{
					Db.endTransaction();
				}
				transactionListeners.notify(removed, Event.REMOVED_UNCONFIRMED_TRANSACTIONS);
			}
		}

		void requeueAllUnconfirmedTransactions()
		{
			IList<Transaction> removed = new List<>();
			using (DbIterator<TransactionImpl> unconfirmedTransactions = AllUnconfirmedTransactions)
			{
				foreach (TransactionImpl transaction in unconfirmedTransactions)
				{
					transaction.undoUnconfirmed();
					removed.Add(transaction);
					lostTransactions.add(transaction);
				}
			}
			unconfirmedTransactionTable.truncate();
			transactionListeners.notify(removed, Event.REMOVED_UNCONFIRMED_TRANSACTIONS);
		}

		void removeUnconfirmedTransaction(TransactionImpl transaction)
		{
			if(!Db.InTransaction)
			{
				try
				{
					Db.beginTransaction();
					removeUnconfirmedTransaction(transaction);
					Db.commitTransaction();
				}
				catch(Exception e)
				{
					Logger.logErrorMessage(e.ToString(), e);
					Db.rollbackTransaction();
					throw e;
				}
				finally
				{
					Db.endTransaction();
				}
				return;
			}
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("DELETE FROM unconfirmed_transaction WHERE id = ?"))
			{
				pstmt.setLong(1, transaction.Id);
				int deleted = pstmt.executeUpdate();
				if(deleted > 0)
				{
					transaction.undoUnconfirmed();
					transactionListeners.notify(Collections.singletonList(transaction), Event.REMOVED_UNCONFIRMED_TRANSACTIONS);
				}
			}
			catch(SQLException e)
			{
				Logger.logErrorMessage(e.ToString(), e);
				throw new Exception(e.ToString(), e);
			}
		}

		int getTransactionVersion(int previousBlockHeight)
		{
			return previousBlockHeight < Constants.DIGITAL_GOODS_STORE_BLOCK ? 0 : 1;
		}

		void processLater(ICollection<TransactionImpl> transactions)
		{
			lock (BlockchainImpl.Instance)
			{
				foreach (TransactionImpl transaction in transactions)
				{
					lostTransactions.add(transaction);
				}
			}
		}

		private void processPeerTransactions(JSONArray transactionsData) throws NxtException.ValidationException
		{
			if(Nxt.Blockchain.LastBlock.Timestamp < Nxt.EpochTime - 60 * 1440 && ! testUnconfirmedTransactions)
			{
				return;
			}
			if(Nxt.Blockchain.Height <= Constants.NQT_BLOCK)
			{
				return;
			}
			IList<TransactionImpl> transactions = new List<>();
			foreach (object transactionData in transactionsData)
			{
				try
				{
					TransactionImpl transaction = parseTransaction((JSONObject) transactionData);
					transaction.validate();
					transactions.Add(transaction);
				}
				catch(NxtException.NotCurrentlyValidException ignore)
				{
				}
				catch(NxtException.NotValidException e)
				{
					Logger.logDebugMessage("Invalid transaction from peer: " + ((JSONObject) transactionData).toJSONString());
					throw e;
				}
			}
			processTransactions(transactions, true);
			nonBroadcastedTransactions.removeAll(transactions);
		}

		IList<Transaction> processTransactions(ICollection<TransactionImpl> transactions, final bool sendToPeers)
		{
			if(transactions.Empty)
			{
				return Collections.emptyList();
			}
			IList<Transaction> sendToPeersTransactions = new List<>();
			IList<Transaction> addedUnconfirmedTransactions = new List<>();
			IList<Transaction> addedDoubleSpendingTransactions = new List<>();

			foreach (TransactionImpl transaction in transactions)
			{

				try
				{

					int curTime = Nxt.EpochTime;
					if(transaction.Timestamp > curTime + 15 || transaction.Expiration < curTime || transaction.Deadline > 1440)
					{
						continue;
					}
					if(transaction.Version < 1)
					{
						continue;
					}

					lock (BlockchainImpl.Instance)
					{
						try
						{
							Db.beginTransaction();
							if(Nxt.Blockchain.Height < Constants.NQT_BLOCK)
							{
								break; // not ready to process transactions
							}

							if(TransactionDb.hasTransaction(transaction.Id) || unconfirmedTransactionTable.get(transaction.DbKey) != null)
							{
								continue;
							}

							if(! transaction.verifySignature())
							{
								if(Account.getAccount(transaction.SenderId) != null)
								{
									Logger.logDebugMessage("Transaction " + transaction.JSONObject.toJSONString() + " failed to verify");
								}
								continue;
							}

							if(transaction.applyUnconfirmed())
							{
								if(sendToPeers)
								{
									if(nonBroadcastedTransactions.contains(transaction))
									{
										Logger.logDebugMessage("Received back transaction " + transaction.StringId + " that we generated, will not forward to peers");
										nonBroadcastedTransactions.remove(transaction);
									}
									else
									{
										sendToPeersTransactions.Add(transaction);
									}
								}
								unconfirmedTransactionTable.insert(transaction);
								addedUnconfirmedTransactions.Add(transaction);
							}
							else
							{
								addedDoubleSpendingTransactions.Add(transaction);
							}
							Db.commitTransaction();
						}
						catch(Exception e)
						{
							Db.rollbackTransaction();
							throw e;
						}
						finally
						{
							Db.endTransaction();
						}
					}
				}
				catch(Exception e)
				{
					Logger.logMessage("Error processing transaction", e);
				}

			}

			if(sendToPeersTransactions.Count > 0)
			{
				Peers.sendToSomePeers(sendToPeersTransactions);
			}

			if(addedUnconfirmedTransactions.Count > 0)
			{
				transactionListeners.notify(addedUnconfirmedTransactions, Event.ADDED_UNCONFIRMED_TRANSACTIONS);
			}
			if(addedDoubleSpendingTransactions.Count > 0)
			{
				transactionListeners.notify(addedDoubleSpendingTransactions, Event.ADDED_DOUBLESPENDING_TRANSACTIONS);
			}
			return addedUnconfirmedTransactions;
		}

	}

}