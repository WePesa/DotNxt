using System;
using System.Collections.Generic;

namespace nxt.user
{

	using Account = nxt.Account;
	using Block = nxt.Block;
	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.user.JSONResponses.LOCK_ACCOUNT;

	public sealed class UnlockAccount : UserServlet.UserRequestHandler
	{

		internal static readonly UnlockAccount instance = new UnlockAccount();

		private UnlockAccount()
		{
		}

		private static final IComparer<JSONObject> myTransactionsComparator = new IComparer<JSONObject>()
		{
			public int compare(JSONObject o1, JSONObject o2)
			{
				int t1 = (int)((Number)o1.get("timestamp"));
				int t2 = (int)((Number)o2.get("timestamp"));
				if(t1 < t2)
				{
					return 1;
				}
				if(t1 > t2)
				{
					return -1;
				}
				string id1 = (string)o1.get("id");
				string id2 = (string)o2.get("id");
				return id2.CompareTo(id1);
			}
		}

		JSONStreamAware processRequest(HttpServletRequest req, User user) throws IOException
		{
			string secretPhrase = req.getParameter("secretPhrase");
		// lock all other instances of this account being unlocked
			foreach (User u in Users.AllUsers)
			{
				if(secretPhrase.Equals(u.SecretPhrase))
				{
					u.lockAccount();
					if(! u.Inactive)
					{
						u.enqueue(LOCK_ACCOUNT);
					}
				}
			}

			long accountId = user.unlockAccount(secretPhrase);

			JSONObject response = new JSONObject();
			response.put("response", "unlockAccount");
			response.put("account", Convert.toUnsignedLong(accountId));

			if(secretPhrase.Length < 30)
			{

				response.put("secretPhraseStrength", 1);

			}
			else
			{

				response.put("secretPhraseStrength", 5);

			}

			Account account = Account.getAccount(accountId);
			if(account == null)
			{

				response.put("balanceNQT", 0);

			}
			else
			{

				response.put("balanceNQT", account.UnconfirmedBalanceNQT);

				JSONArray myTransactions = new JSONArray();
				sbyte[] accountPublicKey = account.PublicKey;
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Transaction> transactions = Nxt.getTransactionProcessor().getAllUnconfirmedTransactions())
				using (DbIterator<?> transactions = Nxt.TransactionProcessor.AllUnconfirmedTransactions)
				{
					while(transactions.hasNext())
					{
						Transaction transaction = transactions.next();
						if(Array.Equals(transaction.SenderPublicKey, accountPublicKey))
						{

							JSONObject myTransaction = new JSONObject();
							myTransaction.put("index", Users.getIndex(transaction));
							myTransaction.put("transactionTimestamp", transaction.Timestamp);
							myTransaction.put("deadline", transaction.Deadline);
							myTransaction.put("account", Convert.toUnsignedLong(transaction.RecipientId));
							myTransaction.put("sentAmountNQT", transaction.AmountNQT);
							if(accountId == transaction.RecipientId)
							{
								myTransaction.put("receivedAmountNQT", transaction.AmountNQT);
							}
							myTransaction.put("feeNQT", transaction.FeeNQT);
							myTransaction.put("numberOfConfirmations", -1);
							myTransaction.put("id", transaction.StringId);

							myTransactions.add(myTransaction);

						}
						else if(accountId == transaction.RecipientId)
						{

							JSONObject myTransaction = new JSONObject();
							myTransaction.put("index", Users.getIndex(transaction));
							myTransaction.put("transactionTimestamp", transaction.Timestamp);
							myTransaction.put("deadline", transaction.Deadline);
							myTransaction.put("account", Convert.toUnsignedLong(transaction.SenderId));
							myTransaction.put("receivedAmountNQT", transaction.AmountNQT);
							myTransaction.put("feeNQT", transaction.FeeNQT);
							myTransaction.put("numberOfConfirmations", -1);
							myTransaction.put("id", transaction.StringId);

							myTransactions.add(myTransaction);

						}
					}
				}

				SortedSet<JSONObject> myTransactionsSet = new TreeSet<>(myTransactionsComparator);

				int blockchainHeight = Nxt.Blockchain.LastBlock.Height;
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Block> blockIterator = Nxt.getBlockchain().getBlocks(account, 0))
				using (DbIterator<?> blockIterator = Nxt.Blockchain.getBlocks(account, 0))
				{
					while(blockIterator.hasNext())
					{
						Block block = blockIterator.next();
						if(block.TotalFeeNQT > 0)
						{
							JSONObject myTransaction = new JSONObject();
							myTransaction.put("index", "block" + Users.getIndex(block));
							myTransaction.put("blockTimestamp", block.Timestamp);
							myTransaction.put("block", block.StringId);
							myTransaction.put("earnedAmountNQT", block.TotalFeeNQT);
							myTransaction.put("numberOfConfirmations", blockchainHeight - block.Height);
							myTransaction.put("id", "-");
							myTransaction.put("timestamp", block.Timestamp);
							myTransactionsSet.add(myTransaction);
						}
					}
				}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: try (DbIterator<? extends Transaction> transactionIterator = Nxt.getBlockchain().getTransactions(account, (byte) -1, (byte) -1, 0))
				using (DbIterator<?> transactionIterator = Nxt.Blockchain.getTransactions(account, (sbyte) -1, (sbyte) -1, 0))
				{
					while(transactionIterator.hasNext())
					{
						Transaction transaction = transactionIterator.next();
						if(transaction.SenderId == accountId)
						{
							JSONObject myTransaction = new JSONObject();
							myTransaction.put("index", Users.getIndex(transaction));
							myTransaction.put("blockTimestamp", transaction.BlockTimestamp);
							myTransaction.put("transactionTimestamp", transaction.Timestamp);
							myTransaction.put("account", Convert.toUnsignedLong(transaction.RecipientId));
							myTransaction.put("sentAmountNQT", transaction.AmountNQT);
							if(accountId == transaction.RecipientId)
							{
								myTransaction.put("receivedAmountNQT", transaction.AmountNQT);
							}
							myTransaction.put("feeNQT", transaction.FeeNQT);
							myTransaction.put("numberOfConfirmations", blockchainHeight - transaction.Height);
							myTransaction.put("id", transaction.StringId);
							myTransaction.put("timestamp", transaction.Timestamp);
							myTransactionsSet.add(myTransaction);
						}
						else if(transaction.RecipientId == accountId)
						{
							JSONObject myTransaction = new JSONObject();
							myTransaction.put("index", Users.getIndex(transaction));
							myTransaction.put("blockTimestamp", transaction.BlockTimestamp);
							myTransaction.put("transactionTimestamp", transaction.Timestamp);
							myTransaction.put("account", Convert.toUnsignedLong(transaction.SenderId));
							myTransaction.put("receivedAmountNQT", transaction.AmountNQT);
							myTransaction.put("feeNQT", transaction.FeeNQT);
							myTransaction.put("numberOfConfirmations", blockchainHeight - transaction.Height);
							myTransaction.put("id", transaction.StringId);
							myTransaction.put("timestamp", transaction.Timestamp);
							myTransactionsSet.add(myTransaction);
						}
					}
				}

				IEnumerator<JSONObject> iterator = myTransactionsSet.GetEnumerator();
				while(myTransactions.size() < 1000 && iterator.MoveNext())
				{
					myTransactions.add(iterator.Current);
				}

				if(myTransactions.size() > 0)
				{
					JSONObject response2 = new JSONObject();
					response2.put("response", "processNewData");
					response2.put("addedMyTransactions", myTransactions);
					user.enqueue(response2);
				}
			}
			return response;
		}

		bool requirePost()
		{
			return true;
		}

	}

}