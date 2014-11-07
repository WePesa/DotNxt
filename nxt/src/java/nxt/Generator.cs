using System;
using System.Collections.Generic;

namespace nxt
{

	using Crypto = nxt.crypto.Crypto;
	using Convert = nxt.util.Convert;
	using Listener = nxt.util.Listener;
	using Listeners = nxt.util.Listeners;
	using Logger = nxt.util.Logger;
	using ThreadPool = nxt.util.ThreadPool;


	public sealed class Generator : Comparable<Generator>
	{

		public enum Event
		{
			GENERATION_DEADLINE,
			START_FORGING,
			STOP_FORGING
		}

		private static readonly sbyte[] fakeForgingPublicKey = Nxt.getBooleanProperty("nxt.enableFakeForging") ? Account.getAccount(Convert.parseAccountId(Nxt.getStringProperty("nxt.fakeForgingAccount"))).PublicKey : null;

		private static readonly Listeners<Generator, Event> listeners = new Listeners<>();

		private static readonly ConcurrentMap<string, Generator> generators = new ConcurrentHashMap<>();
		private static readonly ICollection<Generator> allGenerators = Collections.unmodifiableCollection(generators.values());
		private static volatile IList<Generator> sortedForgers;

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private static final Runnable generateBlocksThread = new Runnable()
//	{
//
//		private volatile int lastTimestamp;
//		private volatile long lastBlockId;
//
//		@Override public void run()
//		{
//
//			try
//			{
//				try
//				{
//					int timestamp = Nxt.getEpochTime();
//					if (timestamp == lastTimestamp)
//					{
//						return;
//					}
//					lastTimestamp = timestamp;
//					synchronized (Nxt.getBlockchain())
//					{
//						Block lastBlock = Nxt.getBlockchain().getLastBlock();
//						if (lastBlock == null || lastBlock.getHeight() < Constants.LAST_KNOWN_BLOCK)
//						{
//							return;
//						}
//						if (lastBlock.getId() != lastBlockId || sortedForgers == null)
//						{
//							lastBlockId = lastBlock.getId();
//							List<Generator> forgers = new ArrayList<>();
//							for (Generator generator : generators.values())
//							{
//								generator.setLastBlock(lastBlock);
//								if (generator.effectiveBalance.signum() > 0)
//								{
//									forgers.add(generator);
//								}
//							}
//							Collections.sort(forgers);
//							sortedForgers = Collections.unmodifiableList(forgers);
//						}
//						for (Generator generator : sortedForgers)
//						{
//							if (generator.getHitTime() > timestamp + 1 || generator.forge(lastBlock, timestamp))
//							{
//								return;
//							}
//						}
//					} // synchronized
//				}
//				catch (Exception e)
//				{
//					Logger.logDebugMessage("Error in block generation thread", e);
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

		static Generator()
		{
			ThreadPool.scheduleThread("GenerateBlocks", generateBlocksThread, 500, TimeUnit.MILLISECONDS);
		}

		internal static void init()
		{
		}

		public static bool addListener(Listener<Generator> listener, Event eventType)
		{
			return listeners.addListener(listener, eventType);
		}

		public static bool removeListener(Listener<Generator> listener, Event eventType)
		{
			return listeners.removeListener(listener, eventType);
		}

		public static Generator startForging(string secretPhrase)
		{
			Generator generator = new Generator(secretPhrase);
			Generator old = generators.putIfAbsent(secretPhrase, generator);
			if(old != null)
			{
				Logger.logDebugMessage("Account " + Convert.toUnsignedLong(old.AccountId) + " is already forging");
				return old;
			}
			listeners.notify(generator, Event.START_FORGING);
			Logger.logDebugMessage("Account " + Convert.toUnsignedLong(generator.AccountId) + " started forging, deadline " + generator.Deadline + " seconds");
			return generator;
		}

		public static Generator stopForging(string secretPhrase)
		{
			Generator generator = generators.remove(secretPhrase);
			if(generator != null)
			{
				sortedForgers = null;
				Logger.logDebugMessage("Account " + Convert.toUnsignedLong(generator.AccountId) + " stopped forging");
				listeners.notify(generator, Event.STOP_FORGING);
			}
			return generator;
		}

		public static Generator getGenerator(string secretPhrase)
		{
			return generators.get(secretPhrase);
		}

		public static ICollection<Generator> AllGenerators
		{
			get
			{
				return allGenerators;
			}
		}

		internal static bool verifyHit(BigInteger hit, BigInteger effectiveBalance, Block previousBlock, int timestamp)
		{
			int elapsedTime = timestamp - previousBlock.Timestamp;
			if(elapsedTime <= 0)
			{
				return false;
			}
			BigInteger effectiveBaseTarget = BigInteger.valueOf(previousBlock.BaseTarget).multiply(effectiveBalance);
			BigInteger prevTarget = effectiveBaseTarget.multiply(BigInteger.valueOf(elapsedTime - 1));
			BigInteger target = prevTarget.add(effectiveBaseTarget);
			return hit.CompareTo(target) < 0 && (previousBlock.Height < Constants.TRANSPARENT_FORGING_BLOCK_8 || hit.CompareTo(prevTarget) >= 0 || (Constants.isTestnet ? elapsedTime > 300 : elapsedTime > 3600) || Constants.isOffline);
		}

		internal static long getHitTime(Account account, Block block)
		{
			return getHitTime(BigInteger.valueOf(account.EffectiveBalanceNXT), getHit(account.PublicKey, block), block);
		}

		internal static bool allowsFakeForging(sbyte[] publicKey)
		{
			return Constants.isTestnet && publicKey != null && Array.Equals(publicKey, fakeForgingPublicKey);
		}

		private static BigInteger getHit(sbyte[] publicKey, Block block)
		{
			if(allowsFakeForging(publicKey))
			{
				return BigInteger.ZERO;
			}
			if(block.Height < Constants.TRANSPARENT_FORGING_BLOCK)
			{
				throw new System.ArgumentException("Not supported below Transparent Forging Block");
			}
			MessageDigest digest = Crypto.sha256();
			digest.update(block.GenerationSignature);
			sbyte[] generationSignatureHash = digest.digest(publicKey);
			return new BigInteger(1, new sbyte[] {generationSignatureHash[7], generationSignatureHash[6], generationSignatureHash[5], generationSignatureHash[4], generationSignatureHash[3], generationSignatureHash[2], generationSignatureHash[1], generationSignatureHash[0]});
		}

		private static long getHitTime(BigInteger effectiveBalance, BigInteger hit, Block block)
		{
			return block.Timestamp + (long)hit.divide(BigInteger.valueOf(block.BaseTarget).multiply(effectiveBalance));
		}


		private readonly long accountId;
		private readonly string secretPhrase;
		private readonly sbyte[] publicKey;
		private volatile long hitTime;
		private volatile BigInteger hit;
		private volatile BigInteger effectiveBalance;

		private Generator(string secretPhrase)
		{
			this.secretPhrase = secretPhrase;
			this.publicKey = Crypto.getPublicKey(secretPhrase);
			this.accountId = Account.getId(publicKey);
			if(Nxt.Blockchain.Height >= Constants.LAST_KNOWN_BLOCK)
			{
				LastBlock = Nxt.Blockchain.LastBlock;
			}
			sortedForgers = null;
		}

		public sbyte[] PublicKey
		{
			get
			{
				return publicKey;
			}
		}

		public long AccountId
		{
			get
			{
				return accountId;
			}
		}

		public long Deadline
		{
			get
			{
				return Math.Max(hitTime - Nxt.Blockchain.LastBlock.Timestamp, 0);
			}
		}

		public long HitTime
		{
			get
			{
				return hitTime;
			}
		}

		public override int compareTo(Generator g)
		{
			int i = this.hit.multiply(g.effectiveBalance).CompareTo(g.hit.multiply(this.effectiveBalance));
			if(i != 0)
			{
				return i;
			}
			return long.compare(accountId, g.accountId);
		}

		public override string ToString()
		{
			return "account: " + Convert.toUnsignedLong(accountId) + " deadline: " + Deadline;
		}

		private Block LastBlock
		{
			set
			{
				Account account = Account.getAccount(accountId);
				effectiveBalance = BigInteger.valueOf(account == null || account.EffectiveBalanceNXT <= 0 ? 0 : account.EffectiveBalanceNXT);
				if(effectiveBalance.signum() == 0)
				{
					return;
				}
				hit = getHit(publicKey, value);
				hitTime = getHitTime(effectiveBalance, hit, value);
				listeners.notify(this, Event.GENERATION_DEADLINE);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private boolean forge(Block lastBlock, int timestamp) throws BlockchainProcessor.BlockNotAcceptedException
		private bool forge(Block lastBlock, int timestamp)
		{
			if(verifyHit(hit, effectiveBalance, lastBlock, timestamp))
			{
				while(true)
				{
					try
					{
						BlockchainProcessorImpl.Instance.generateBlock(secretPhrase, timestamp);
						return true;
					}
					catch(BlockchainProcessor.TransactionNotAcceptedException e)
					{
						if(Nxt.EpochTime - timestamp > 10)
						{
							throw e;
						}
					}
				}
			}
			return false;
		}

	}

}