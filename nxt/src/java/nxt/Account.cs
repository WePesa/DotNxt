using System;

namespace nxt
{

	using Crypto = nxt.crypto.Crypto;
	using EncryptedData = nxt.crypto.EncryptedData;
	using Db = nxt.db.Db;
	using DbClause = nxt.db.DbClause;
	using DbIterator = nxt.db.DbIterator;
	using DbKey = nxt.db.DbKey;
	using DbUtils = nxt.db.DbUtils;
	using DerivedDbTable = nxt.db.DerivedDbTable;
	using VersionedEntityDbTable = nxt.db.VersionedEntityDbTable;
	using Convert = nxt.util.Convert;
	using Listener = nxt.util.Listener;
	using Listeners = nxt.util.Listeners;
	using Logger = nxt.util.Logger;


	public sealed class Account
	{

		public enum Event
		{
			BALANCE,
			UNCONFIRMED_BALANCE,
			ASSET_BALANCE,
			UNCONFIRMED_ASSET_BALANCE,
			LEASE_SCHEDULED,
			LEASE_STARTED,
			LEASE_ENDED
		}

		public class AccountAsset
		{

			private readonly long accountId;
			private readonly long assetId;
			private readonly DbKey dbKey;
			private long quantityQNT;
			private long unconfirmedQuantityQNT;

			private AccountAsset(long accountId, long assetId, long quantityQNT, long unconfirmedQuantityQNT)
			{
				this.accountId = accountId;
				this.assetId = assetId;
				this.dbKey = accountAssetDbKeyFactory.newKey(this.accountId, this.assetId);
				this.quantityQNT = quantityQNT;
				this.unconfirmedQuantityQNT = unconfirmedQuantityQNT;
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private AccountAsset(ResultSet rs) throws SQLException
			private AccountAsset(ResultSet rs)
			{
				this.accountId = rs.getLong("account_id");
				this.assetId = rs.getLong("asset_id");
				this.dbKey = accountAssetDbKeyFactory.newKey(this.accountId, this.assetId);
				this.quantityQNT = rs.getLong("quantity");
				this.unconfirmedQuantityQNT = rs.getLong("unconfirmed_quantity");
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void save(Connection con) throws SQLException
			private void save(Connection con)
			{
				using (PreparedStatement pstmt = con.prepareStatement("MERGE INTO account_asset " + "(account_id, asset_id, quantity, unconfirmed_quantity, height, latest) " + "KEY (account_id, asset_id, height) VALUES (?, ?, ?, ?, ?, TRUE)"))
				{
					int i = 0;
					pstmt.setLong(++i, this.accountId);
					pstmt.setLong(++i, this.assetId);
					pstmt.setLong(++i, this.quantityQNT);
					pstmt.setLong(++i, this.unconfirmedQuantityQNT);
					pstmt.setInt(++i, Nxt.Blockchain.Height);
					pstmt.executeUpdate();
				}
			}

			public virtual long AccountId
			{
				get
				{
					return accountId;
				}
			}

			public virtual long AssetId
			{
				get
				{
					return assetId;
				}
			}

			public virtual long QuantityQNT
			{
				get
				{
					return quantityQNT;
				}
			}

			public virtual long UnconfirmedQuantityQNT
			{
				get
				{
					return unconfirmedQuantityQNT;
				}
			}

			private void save()
			{
				checkBalance(this.accountId, this.quantityQNT, this.unconfirmedQuantityQNT);
				if(this.quantityQNT > 0 || this.unconfirmedQuantityQNT > 0)
				{
					accountAssetTable.insert(this);
				}
				else
				{
					accountAssetTable.delete(this);
				}
			}

			public override string ToString()
			{
				return "AccountAsset account_id: " + Convert.toUnsignedLong(accountId) + " asset_id: " + Convert.toUnsignedLong(assetId) + " quantity: " + quantityQNT + " unconfirmedQuantity: " + unconfirmedQuantityQNT;
			}

		}

		public class AccountLease
		{

			public readonly long lessorId;
			public readonly long lesseeId;
			public readonly int fromHeight;
			public readonly int toHeight;

			private AccountLease(long lessorId, long lesseeId, int fromHeight, int toHeight)
			{
				this.lessorId = lessorId;
				this.lesseeId = lesseeId;
				this.fromHeight = fromHeight;
				this.toHeight = toHeight;
			}

		}

		internal class DoubleSpendingException : Exception
		{

			internal DoubleSpendingException(string message) : base(message)
			{
			}

		}

		static Account()
		{

			Nxt.BlockchainProcessor.addListener(new Listener<Block>() { public void notify(Block block) { int height = block.Height; try (DbIterator<Account> leasingAccounts = LeasingAccounts) { while(leasingAccounts.hasNext()) { Account account = leasingAccounts.next(); if(height == account.currentLeasingHeightFrom) { leaseListeners.notify(new AccountLease(account.Id, account.currentLesseeId, height, account.currentLeasingHeightTo), Event.LEASE_STARTED); } else if(height == account.currentLeasingHeightTo) { leaseListeners.notify(new AccountLease(account.Id, account.currentLesseeId, account.currentLeasingHeightFrom, height), Event.LEASE_ENDED); if(account.nextLeasingHeightFrom == int.MaxValue) { account.currentLeasingHeightFrom = int.MaxValue; account.currentLesseeId = 0; accountTable.insert(account); } else { account.currentLeasingHeightFrom = account.nextLeasingHeightFrom; account.currentLeasingHeightTo = account.nextLeasingHeightTo; account.currentLesseeId = account.nextLesseeId; account.nextLeasingHeightFrom = int.MaxValue; account.nextLesseeId = 0; accountTable.insert(account); if(height == account.currentLeasingHeightFrom) { leaseListeners.notify(new AccountLease(account.Id, account.currentLesseeId, height, account.currentLeasingHeightTo), Event.LEASE_STARTED); } } } } } } }, BlockchainProcessor.Event.AFTER_BLOCK_APPLY);

		}

		private static final DbKey.LongKeyFactory<Account> accountDbKeyFactory = new DbKey.LongKeyFactory<Account>("id")
		{

			public DbKey newKey(Account account)
			{
				return account.dbKey;
			}

		}

		private static final VersionedEntityDbTable<Account> accountTable = new VersionedEntityDbTable<Account>("account", accountDbKeyFactory)
		{

			protected Account load(Connection con, ResultSet rs) throws SQLException
			{
				return new Account(rs);
			}

			protected void save(Connection con, Account account) throws SQLException
			{
				account.save(con);
			}

		}

		private static final DbKey.LinkKeyFactory<AccountAsset> accountAssetDbKeyFactory = new DbKey.LinkKeyFactory<AccountAsset>("account_id", "asset_id")
		{

			public DbKey newKey(AccountAsset accountAsset)
			{
				return accountAsset.dbKey;
			}

		}

		private static final VersionedEntityDbTable<AccountAsset> accountAssetTable = new VersionedEntityDbTable<AccountAsset>("account_asset", accountAssetDbKeyFactory)
		{

			protected AccountAsset load(Connection con, ResultSet rs) throws SQLException
			{
				return new AccountAsset(rs);
			}

			protected void save(Connection con, AccountAsset accountAsset) throws SQLException
			{
				accountAsset.save(con);
			}

			protected string defaultSort()
			{
				return " ORDER BY quantity DESC, account_id, asset_id ";
			}

		}

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private static final DerivedDbTable accountGuaranteedBalanceTable = new DerivedDbTable("account_guaranteed_balance")
//	{
//
//		@Override public void trim(int height)
//		{
//			try (Connection con = Db.getConnection(); PreparedStatement pstmtDelete = con.prepareStatement("DELETE FROM account_guaranteed_balance " + "WHERE height < ?"))
//			{
//				pstmtDelete.setInt(1, height - 1440);
//				pstmtDelete.executeUpdate();
//			}
//			catch (SQLException e)
//			{
//				throw new RuntimeException(e.toString(), e);
//			}
//		}
//
//	};

		private static final Listeners<Account, Event> listeners = new Listeners<>();

		private static final Listeners<AccountAsset, Event> assetListeners = new Listeners<>();

		private static final Listeners<AccountLease, Event> leaseListeners = new Listeners<>();

		public static bool addListener(Listener<Account> listener, Event eventType)
		{
			return listeners.addListener(listener, eventType);
		}

		public static bool removeListener(Listener<Account> listener, Event eventType)
		{
			return listeners.removeListener(listener, eventType);
		}

		public static bool addAssetListener(Listener<AccountAsset> listener, Event eventType)
		{
			return assetListeners.addListener(listener, eventType);
		}

		public static bool removeAssetListener(Listener<AccountAsset> listener, Event eventType)
		{
			return assetListeners.removeListener(listener, eventType);
		}

		public static bool addLeaseListener(Listener<AccountLease> listener, Event eventType)
		{
			return leaseListeners.addListener(listener, eventType);
		}

		public static bool removeLeaseListener(Listener<AccountLease> listener, Event eventType)
		{
			return leaseListeners.removeListener(listener, eventType);
		}

		public static DbIterator<Account> getAllAccounts(int from, int to)
		{
			return accountTable.getAll(from, to);
		}

		public static int Count
		{
			return accountTable.Count;
		}

		public static int getAssetAccountsCount(long assetId)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT COUNT(*) FROM account_asset WHERE asset_id = ? AND latest = TRUE"))
			{
				pstmt.setLong(1, assetId);
				using (ResultSet rs = pstmt.executeQuery())
				{
					rs.next();
					return rs.getInt(1);
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public static Account getAccount(long id)
		{
			return id == 0 ? null : accountTable.get(accountDbKeyFactory.newKey(id));
		}

		public static Account getAccount(long id, int height)
		{
			return id == 0 ? null : accountTable.get(accountDbKeyFactory.newKey(id), height);
		}

		public static Account getAccount(sbyte[] publicKey)
		{
			Account account = accountTable.get(accountDbKeyFactory.newKey(getId(publicKey)));
			if(account == null)
			{
				return null;
			}
			if(account.PublicKey == null || Array.Equals(account.PublicKey, publicKey))
			{
				return account;
			}
			throw new Exception("DUPLICATE KEY for account " + Convert.toUnsignedLong(account.Id) + " existing key " + Convert.toHexString(account.PublicKey) + " new key " + Convert.toHexString(publicKey));
		}

		public static long getId(sbyte[] publicKey)
		{
			sbyte[] publicKeyHash = Crypto.sha256().digest(publicKey);
			return Convert.fullHashToId(publicKeyHash);
		}

		static Account addOrGetAccount(long id)
		{
			Account account = accountTable.get(accountDbKeyFactory.newKey(id));
			if(account == null)
			{
				account = new Account(id);
				accountTable.insert(account);
			}
			return account;
		}

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private static final DbClause leasingAccountsClause = new DbClause(" current_lessee_id >= ? ")
//	{
//		@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//		{
//			pstmt.setLong(index++, Long.MIN_VALUE);
//			return index;
//		}
//	};

		public static DbIterator<Account> LeasingAccounts
		{
			return accountTable.getManyBy(leasingAccountsClause, 0, -1);
		}

		public static DbIterator<AccountAsset> getAssetAccounts(long assetId, int from, int to)
		{
			return accountAssetTable.getManyBy(new DbClause.LongClause("asset_id", assetId), from, to, " ORDER BY quantity DESC, account_id ");
		}

		public static DbIterator<AccountAsset> getAssetAccounts(long assetId, int height, int from, int to)
		{
			if(height < 0)
			{
				return getAssetAccounts(assetId, from, to);
			}
			return accountAssetTable.getManyBy(new DbClause.LongClause("asset_id", assetId), height, from, to, " ORDER BY quantity DESC, account_id ");
		}

		static void init()
		{
		}


		private final long id;
		private final DbKey dbKey;
		private final int creationHeight;
		private sbyte[] publicKey;
		private int keyHeight;
		private long balanceNQT;
		private long unconfirmedBalanceNQT;
		private long forgedBalanceNQT;

		private int currentLeasingHeightFrom;
		private int currentLeasingHeightTo;
		private long currentLesseeId;
		private int nextLeasingHeightFrom;
		private int nextLeasingHeightTo;
		private long nextLesseeId;
		private string name;
		private string description;

		private Account(long id)
		{
			if(id != Crypto.rsDecode(Crypto.rsEncode(id)))
			{
				Logger.logMessage("CRITICAL ERROR: Reed-Solomon encoding fails for " + id);
			}
			this.id = id;
			this.dbKey = accountDbKeyFactory.newKey(this.id);
			this.creationHeight = Nxt.Blockchain.Height;
			currentLeasingHeightFrom = int.MaxValue;
		}

		private Account(ResultSet rs) throws SQLException
		{
			this.id = rs.getLong("id");
			this.dbKey = accountDbKeyFactory.newKey(this.id);
			this.creationHeight = rs.getInt("creation_height");
			this.publicKey = rs.getBytes("public_key");
			this.keyHeight = rs.getInt("key_height");
			this.balanceNQT = rs.getLong("balance");
			this.unconfirmedBalanceNQT = rs.getLong("unconfirmed_balance");
			this.forgedBalanceNQT = rs.getLong("forged_balance");
			this.name = rs.getString("name");
			this.description = rs.getString("description");
			this.currentLeasingHeightFrom = rs.getInt("current_leasing_height_from");
			this.currentLeasingHeightTo = rs.getInt("current_leasing_height_to");
			this.currentLesseeId = rs.getLong("current_lessee_id");
			this.nextLeasingHeightFrom = rs.getInt("next_leasing_height_from");
			this.nextLeasingHeightTo = rs.getInt("next_leasing_height_to");
			this.nextLesseeId = rs.getLong("next_lessee_id");
		}

		private void save(Connection con) throws SQLException
		{
			using (PreparedStatement pstmt = con.prepareStatement("MERGE INTO account (id, creation_height, public_key, " + "key_height, balance, unconfirmed_balance, forged_balance, name, description, " + "current_leasing_height_from, current_leasing_height_to, current_lessee_id, " + "next_leasing_height_from, next_leasing_height_to, next_lessee_id, " + "height, latest) " + "KEY (id, height) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, TRUE)"))
			{
				int i = 0;
				pstmt.setLong(++i, this.Id);
				pstmt.setInt(++i, this.CreationHeight);
				DbUtils.setBytes(pstmt, ++i, this.PublicKey);
				pstmt.setInt(++i, this.KeyHeight);
				pstmt.setLong(++i, this.BalanceNQT);
				pstmt.setLong(++i, this.UnconfirmedBalanceNQT);
				pstmt.setLong(++i, this.ForgedBalanceNQT);
				DbUtils.setString(pstmt, ++i, this.Name);
				DbUtils.setString(pstmt, ++i, this.Description);
				DbUtils.setIntZeroToNull(pstmt, ++i, this.CurrentLeasingHeightFrom);
				DbUtils.setIntZeroToNull(pstmt, ++i, this.CurrentLeasingHeightTo);
				DbUtils.setLongZeroToNull(pstmt, ++i, this.CurrentLesseeId);
				DbUtils.setIntZeroToNull(pstmt, ++i, this.NextLeasingHeightFrom);
				DbUtils.setIntZeroToNull(pstmt, ++i, this.NextLeasingHeightTo);
				DbUtils.setLongZeroToNull(pstmt, ++i, this.NextLesseeId);
				pstmt.setInt(++i, Nxt.Blockchain.Height);
				pstmt.executeUpdate();
			}
		}

		public long Id
		{
			return id;
		}

		public string Name
		{
			return name;
		}

		public string Description
		{
			return description;
		}

		void setAccountInfo(string name, string description)
		{
			this.name = Convert.emptyToNull(name.Trim());
			this.description = Convert.emptyToNull(description.Trim());
			accountTable.insert(this);
		}

		public sbyte[] PublicKey
		{
			if(this.keyHeight == -1)
			{
				return null;
			}
			return publicKey;
		}

		private int CreationHeight
		{
			return creationHeight;
		}

		private int KeyHeight
		{
			return keyHeight;
		}

		public EncryptedData encryptTo(sbyte[] data, string senderSecretPhrase)
		{
			if(PublicKey == null)
			{
				throw new System.ArgumentException("Recipient account doesn't have a public key set");
			}
			return EncryptedData.encrypt(data, Crypto.getPrivateKey(senderSecretPhrase), publicKey);
		}

		public sbyte[] decryptFrom(EncryptedData encryptedData, string recipientSecretPhrase)
		{
			if(PublicKey == null)
			{
				throw new System.ArgumentException("Sender account doesn't have a public key set");
			}
			return encryptedData.decrypt(Crypto.getPrivateKey(recipientSecretPhrase), publicKey);
		}

		public long BalanceNQT
		{
			return balanceNQT;
		}

		public long UnconfirmedBalanceNQT
		{
			return unconfirmedBalanceNQT;
		}

		public long ForgedBalanceNQT
		{
			return forgedBalanceNQT;
		}

		public long EffectiveBalanceNXT
		{

			Block lastBlock = Nxt.Blockchain.LastBlock;
			if(lastBlock.Height >= Constants.TRANSPARENT_FORGING_BLOCK_6 && (PublicKey == null || lastBlock.Height - keyHeight <= 1440))
			{
				return 0; // cfb: Accounts with the public key revealed less than 1440 blocks ago are not allowed to generate blocks
			}
			if(lastBlock.Height < Constants.TRANSPARENT_FORGING_BLOCK_3 && this.creationHeight < Constants.TRANSPARENT_FORGING_BLOCK_2)
			{
				if(this.creationHeight == 0)
				{
					return BalanceNQT / Constants.ONE_NXT;
				}
				if(lastBlock.Height - this.creationHeight < 1440)
				{
					return 0;
				}
				long receivedInlastBlock = 0;
				foreach (Transaction transaction in lastBlock.Transactions)
				{
					if(id == transaction.RecipientId)
					{
						receivedInlastBlock += transaction.AmountNQT;
					}
				}
				return(BalanceNQT - receivedInlastBlock) / Constants.ONE_NXT;
			}
			if(lastBlock.Height < currentLeasingHeightFrom)
			{
				return(getGuaranteedBalanceNQT(1440) + LessorsGuaranteedBalanceNQT) / Constants.ONE_NXT;
			}
			return LessorsGuaranteedBalanceNQT / Constants.ONE_NXT;
		}

		private long LessorsGuaranteedBalanceNQT
		{
			long lessorsGuaranteedBalanceNQT = 0;
			using (DbIterator<Account> lessors = Lessors)
			{
				while(lessors.hasNext())
				{
					lessorsGuaranteedBalanceNQT += lessors.next().getGuaranteedBalanceNQT(1440);
				}
			}
			return lessorsGuaranteedBalanceNQT;
		}

		private DbClause getLessorsClause(int height)
		{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//			return new DbClause(" current_lessee_id = ? AND current_leasing_height_from <= ? AND current_leasing_height_to > ? ")
//		{
//			@Override public int set(PreparedStatement pstmt, int index) throws SQLException
//			{
//				pstmt.setLong(index++, getId());
//				pstmt.setInt(index++, height);
//				pstmt.setInt(index++, height);
//				return index;
//			}
//		};
		}

		public DbIterator<Account> Lessors
		{
			return accountTable.getManyBy(getLessorsClause(Nxt.Blockchain.Height), 0, -1);
		}

		public DbIterator<Account> getLessors(int height)
		{
			if(height < 0)
			{
				return Lessors;
			}
			return accountTable.getManyBy(getLessorsClause(height), height, 0, -1);
		}

		public long getGuaranteedBalanceNQT(int numberOfConfirmations)
		{
			return getGuaranteedBalanceNQT(numberOfConfirmations, Nxt.Blockchain.Height);
		}

		public long getGuaranteedBalanceNQT(int numberOfConfirmations, int currentHeight)
		{
			if(numberOfConfirmations >= currentHeight)
			{
				return 0;
			}
			if(numberOfConfirmations > 2880 || numberOfConfirmations < 0)
			{
				throw new System.ArgumentException("Number of required confirmations must be between 0 and " + 2880);
			}
			int height = currentHeight - numberOfConfirmations;
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT SUM (additions) AS additions " + "FROM account_guaranteed_balance WHERE account_id = ? AND height > ? AND height <= ?"))
			{
				pstmt.setLong(1, this.id);
				pstmt.setInt(2, height);
				pstmt.setInt(3, currentHeight);
				using (ResultSet rs = pstmt.executeQuery())
				{
					if(!rs.next())
					{
						return balanceNQT;
					}
					return Math.Max(Convert.safeSubtract(balanceNQT, rs.getLong("additions")), 0);
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public DbIterator<AccountAsset> getAssets(int from, int to)
		{
			return accountAssetTable.getManyBy(new DbClause.LongClause("account_id", this.id), from, to);
		}

		public DbIterator<Trade> getTrades(int from, int to)
		{
			return Trade.getAccountTrades(this.id, from, to);
		}

		public DbIterator<AssetTransfer> getAssetTransfers(int from, int to)
		{
			return AssetTransfer.getAccountAssetTransfers(this.id, from, to);
		}

		public long getAssetBalanceQNT(long assetId)
		{
			AccountAsset accountAsset = accountAssetTable.get(accountAssetDbKeyFactory.newKey(this.id, assetId));
			return accountAsset == null ? 0 : accountAsset.quantityQNT;
		}

		public long getUnconfirmedAssetBalanceQNT(long assetId)
		{
			AccountAsset accountAsset = accountAssetTable.get(accountAssetDbKeyFactory.newKey(this.id, assetId));
			return accountAsset == null ? 0 : accountAsset.unconfirmedQuantityQNT;
		}

		public long CurrentLesseeId
		{
			return currentLesseeId;
		}

		public long NextLesseeId
		{
			return nextLesseeId;
		}

		public int CurrentLeasingHeightFrom
		{
			return currentLeasingHeightFrom;
		}

		public int CurrentLeasingHeightTo
		{
			return currentLeasingHeightTo;
		}

		public int NextLeasingHeightFrom
		{
			return nextLeasingHeightFrom;
		}

		public int NextLeasingHeightTo
		{
			return nextLeasingHeightTo;
		}

		void leaseEffectiveBalance(long lesseeId, short period)
		{
			Account lessee = Account.getAccount(lesseeId);
			if(lessee != null && lessee.PublicKey != null)
			{
				int height = Nxt.Blockchain.Height;
				if(currentLeasingHeightFrom == int.MaxValue)
				{
					currentLeasingHeightFrom = height + 1440;
					currentLeasingHeightTo = currentLeasingHeightFrom + period;
					currentLesseeId = lesseeId;
					nextLeasingHeightFrom = int.MaxValue;
					accountTable.insert(this);
					leaseListeners.notify(new AccountLease(this.Id, lesseeId, currentLeasingHeightFrom, currentLeasingHeightTo), Event.LEASE_SCHEDULED);
				}
				else
				{
					nextLeasingHeightFrom = height + 1440;
					if(nextLeasingHeightFrom < currentLeasingHeightTo)
					{
						nextLeasingHeightFrom = currentLeasingHeightTo;
					}
					nextLeasingHeightTo = nextLeasingHeightFrom + period;
					nextLesseeId = lesseeId;
					accountTable.insert(this);
					leaseListeners.notify(new AccountLease(this.Id, lesseeId, nextLeasingHeightFrom, nextLeasingHeightTo), Event.LEASE_SCHEDULED);

				}
			}
		}

	// returns true iff:
	// this.publicKey is set to null (in which case this.publicKey also gets set to key)
	// or
	// this.publicKey is already set to an array equal to key
		bool setOrVerify(sbyte[] key, int height)
		{
			if(this.publicKey == null)
			{
				if(Db.InTransaction)
				{
					this.publicKey = key;
					this.keyHeight = -1;
					accountTable.insert(this);
				}
				return true;
			}
			else if(Array.Equals(this.publicKey, key))
			{
				return true;
			}
			else if(this.keyHeight == -1)
			{
				Logger.logMessage("DUPLICATE KEY!!!");
				Logger.logMessage("Account key for " + Convert.toUnsignedLong(id) + " was already set to a different one at the same height " + ", current height is " + height + ", rejecting new key");
				return false;
			}
			else if(this.keyHeight >= height)
			{
				Logger.logMessage("DUPLICATE KEY!!!");
				if(Db.InTransaction)
				{
					Logger.logMessage("Changing key for account " + Convert.toUnsignedLong(id) + " at height " + height + ", was previously set to a different one at height " + keyHeight);
					this.publicKey = key;
					this.keyHeight = height;
					accountTable.insert(this);
				}
				return true;
			}
			Logger.logMessage("DUPLICATE KEY!!!");
			Logger.logMessage("Invalid key for account " + Convert.toUnsignedLong(id) + " at height " + height + ", was already set to a different one at height " + keyHeight);
			return false;
		}

		void apply(sbyte[] key, int height)
		{
			if(! setOrVerify(key, this.creationHeight))
			{
				throw new InvalidOperationException("Public key mismatch");
			}
			if(this.publicKey == null)
			{
				throw new InvalidOperationException("Public key has not been set for account " + Convert.toUnsignedLong(id) +" at height " + height + ", key height is " + keyHeight);
			}
			if(this.keyHeight == -1 || this.keyHeight > height)
			{
				this.keyHeight = height;
				accountTable.insert(this);
			}
		}

		void addToAssetBalanceQNT(long assetId, long quantityQNT)
		{
			if(quantityQNT == 0)
			{
				return;
			}
			AccountAsset accountAsset;
			accountAsset = accountAssetTable.get(accountAssetDbKeyFactory.newKey(this.id, assetId));
			long assetBalance = accountAsset == null ? 0 : accountAsset.quantityQNT;
			assetBalance = Convert.safeAdd(assetBalance, quantityQNT);
			if(accountAsset == null)
			{
				accountAsset = new AccountAsset(this.id, assetId, assetBalance, 0);
			}
			else
			{
				accountAsset.quantityQNT = assetBalance;
			}
			accountAsset.save();
			listeners.notify(this, Event.ASSET_BALANCE);
			assetListeners.notify(accountAsset, Event.ASSET_BALANCE);
		}

		void addToUnconfirmedAssetBalanceQNT(long assetId, long quantityQNT)
		{
			if(quantityQNT == 0)
			{
				return;
			}
			AccountAsset accountAsset;
			accountAsset = accountAssetTable.get(accountAssetDbKeyFactory.newKey(this.id, assetId));
			long unconfirmedAssetBalance = accountAsset == null ? 0 : accountAsset.unconfirmedQuantityQNT;
			unconfirmedAssetBalance = Convert.safeAdd(unconfirmedAssetBalance, quantityQNT);
			if(accountAsset == null)
			{
				accountAsset = new AccountAsset(this.id, assetId, 0, unconfirmedAssetBalance);
			}
			else
			{
				accountAsset.unconfirmedQuantityQNT = unconfirmedAssetBalance;
			}
			accountAsset.save();
			listeners.notify(this, Event.UNCONFIRMED_ASSET_BALANCE);
			assetListeners.notify(accountAsset, Event.UNCONFIRMED_ASSET_BALANCE);
		}

		void addToAssetAndUnconfirmedAssetBalanceQNT(long assetId, long quantityQNT)
		{
			if(quantityQNT == 0)
			{
				return;
			}
			AccountAsset accountAsset;
			accountAsset = accountAssetTable.get(accountAssetDbKeyFactory.newKey(this.id, assetId));
			long assetBalance = accountAsset == null ? 0 : accountAsset.quantityQNT;
			assetBalance = Convert.safeAdd(assetBalance, quantityQNT);
			long unconfirmedAssetBalance = accountAsset == null ? 0 : accountAsset.unconfirmedQuantityQNT;
			unconfirmedAssetBalance = Convert.safeAdd(unconfirmedAssetBalance, quantityQNT);
			if(accountAsset == null)
			{
				accountAsset = new AccountAsset(this.id, assetId, assetBalance, unconfirmedAssetBalance);
			}
			else
			{
				accountAsset.quantityQNT = assetBalance;
				accountAsset.unconfirmedQuantityQNT = unconfirmedAssetBalance;
			}
			accountAsset.save();
			listeners.notify(this, Event.ASSET_BALANCE);
			listeners.notify(this, Event.UNCONFIRMED_ASSET_BALANCE);
			assetListeners.notify(accountAsset, Event.ASSET_BALANCE);
			assetListeners.notify(accountAsset, Event.UNCONFIRMED_ASSET_BALANCE);
		}

		void addToBalanceNQT(long amountNQT)
		{
			if(amountNQT == 0)
			{
				return;
			}
			this.balanceNQT = Convert.safeAdd(this.balanceNQT, amountNQT);
			addToGuaranteedBalanceNQT(amountNQT);
			checkBalance(this.id, this.balanceNQT, this.unconfirmedBalanceNQT);
			accountTable.insert(this);
			listeners.notify(this, Event.BALANCE);
		}

		void addToUnconfirmedBalanceNQT(long amountNQT)
		{
			if(amountNQT == 0)
			{
				return;
			}
			this.unconfirmedBalanceNQT = Convert.safeAdd(this.unconfirmedBalanceNQT, amountNQT);
			checkBalance(this.id, this.balanceNQT, this.unconfirmedBalanceNQT);
			accountTable.insert(this);
			listeners.notify(this, Event.UNCONFIRMED_BALANCE);
		}

		void addToBalanceAndUnconfirmedBalanceNQT(long amountNQT)
		{
			if(amountNQT == 0)
			{
				return;
			}
			this.balanceNQT = Convert.safeAdd(this.balanceNQT, amountNQT);
			this.unconfirmedBalanceNQT = Convert.safeAdd(this.unconfirmedBalanceNQT, amountNQT);
			addToGuaranteedBalanceNQT(amountNQT);
			checkBalance(this.id, this.balanceNQT, this.unconfirmedBalanceNQT);
			accountTable.insert(this);
			listeners.notify(this, Event.BALANCE);
			listeners.notify(this, Event.UNCONFIRMED_BALANCE);
		}

		void addToForgedBalanceNQT(long amountNQT)
		{
			if(amountNQT == 0)
			{
				return;
			}
			this.forgedBalanceNQT = Convert.safeAdd(this.forgedBalanceNQT, amountNQT);
			accountTable.insert(this);
		}

		private static void checkBalance(long accountId, long confirmed, long unconfirmed)
		{
			if(accountId == Genesis.CREATOR_ID)
			{
				return;
			}
			if(confirmed < 0)
			{
				throw new DoubleSpendingException("Negative balance or quantity for account " + Convert.toUnsignedLong(accountId));
			}
			if(unconfirmed < 0)
			{
				throw new DoubleSpendingException("Negative unconfirmed balance or quantity for account " + Convert.toUnsignedLong(accountId));
			}
			if(unconfirmed > confirmed)
			{
				throw new DoubleSpendingException("Unconfirmed exceeds confirmed balance or quantity for account " + Convert.toUnsignedLong(accountId));
			}
		}

		private void addToGuaranteedBalanceNQT(long amountNQT)
		{
			if(amountNQT <= 0)
			{
				return;
			}
			int blockchainHeight = Nxt.Blockchain.Height;
			using (Connection con = Db.Connection, PreparedStatement pstmtSelect = con.prepareStatement("SELECT additions FROM account_guaranteed_balance " + "WHERE account_id = ? and height = ?"), PreparedStatement pstmtUpdate = con.prepareStatement("MERGE INTO account_guaranteed_balance (account_id, " + " additions, height) KEY (account_id, height) VALUES(?, ?, ?)"))
			{
				pstmtSelect.setLong(1, this.id);
				pstmtSelect.setInt(2, blockchainHeight);
				using (ResultSet rs = pstmtSelect.executeQuery())
				{
					long additions = amountNQT;
					if(rs.next())
					{
						additions = Convert.safeAdd(additions, rs.getLong("additions"));
					}
					pstmtUpdate.setLong(1, this.id);
					pstmtUpdate.setLong(2, additions);
					pstmtUpdate.setInt(3, blockchainHeight);
					pstmtUpdate.executeUpdate();
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

	}

}