using System;
using System.Collections.Generic;

namespace nxt
{

	using Crypto = nxt.crypto.Crypto;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;


	internal sealed class BlockImpl : Block
	{

		private readonly int version;
		private readonly int timestamp;
		private readonly long previousBlockId;
		private readonly sbyte[] generatorPublicKey;
		private readonly sbyte[] previousBlockHash;
		private readonly long totalAmountNQT;
		private readonly long totalFeeNQT;
		private readonly int payloadLength;
		private readonly sbyte[] generationSignature;
		private readonly sbyte[] payloadHash;
		private volatile IList<TransactionImpl> blockTransactions;

		private sbyte[] blockSignature;
		private BigInteger cumulativeDifficulty = BigInteger.ZERO;
		private long baseTarget = Constants.INITIAL_BASE_TARGET;
		private volatile long nextBlockId;
		private int height = -1;
		private volatile long id;
		private volatile string stringId = null;
		private volatile long generatorId;


//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: BlockImpl(int version, int timestamp, long previousBlockId, long totalAmountNQT, long totalFeeNQT, int payloadLength, byte[] payloadHash, byte[] generatorPublicKey, byte[] generationSignature, byte[] blockSignature, byte[] previousBlockHash, List<TransactionImpl> transactions) throws NxtException.ValidationException
		internal BlockImpl(int version, int timestamp, long previousBlockId, long totalAmountNQT, long totalFeeNQT, int payloadLength, sbyte[] payloadHash, sbyte[] generatorPublicKey, sbyte[] generationSignature, sbyte[] blockSignature, sbyte[] previousBlockHash, IList<TransactionImpl> transactions)
		{

			if(payloadLength > Constants.MAX_PAYLOAD_LENGTH || payloadLength < 0)
			{
				throw new NxtException.NotValidException("attempted to create a block with payloadLength " + payloadLength);
			}

			this.version = version;
			this.timestamp = timestamp;
			this.previousBlockId = previousBlockId;
			this.totalAmountNQT = totalAmountNQT;
			this.totalFeeNQT = totalFeeNQT;
			this.payloadLength = payloadLength;
			this.payloadHash = payloadHash;
			this.generatorPublicKey = generatorPublicKey;
			this.generationSignature = generationSignature;
			this.blockSignature = blockSignature;
			this.previousBlockHash = previousBlockHash;
			if(transactions != null)
			{
				this.blockTransactions = Collections.unmodifiableList(transactions);
				if(blockTransactions.Count > Constants.MAX_NUMBER_OF_TRANSACTIONS)
				{
					throw new NxtException.NotValidException("attempted to create a block with " + blockTransactions.Count + " transactions");
				}
				long previousId = 0;
				foreach (Transaction transaction in this.blockTransactions)
				{
					if(transaction.Id <= previousId && previousId != 0)
					{
						throw new NxtException.NotValidException("Block transactions are not sorted!");
					}
					previousId = transaction.Id;
				}
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: BlockImpl(int version, int timestamp, long previousBlockId, long totalAmountNQT, long totalFeeNQT, int payloadLength, byte[] payloadHash, byte[] generatorPublicKey, byte[] generationSignature, byte[] blockSignature, byte[] previousBlockHash, BigInteger cumulativeDifficulty, long baseTarget, long nextBlockId, int height, long id) throws NxtException.ValidationException
		internal BlockImpl(int version, int timestamp, long previousBlockId, long totalAmountNQT, long totalFeeNQT, int payloadLength, sbyte[] payloadHash, sbyte[] generatorPublicKey, sbyte[] generationSignature, sbyte[] blockSignature, sbyte[] previousBlockHash, BigInteger cumulativeDifficulty, long baseTarget, long nextBlockId, int height, long id) : this(version, timestamp, previousBlockId, totalAmountNQT, totalFeeNQT, payloadLength, payloadHash, generatorPublicKey, generationSignature, blockSignature, previousBlockHash, null)
		{
			this.cumulativeDifficulty = cumulativeDifficulty;
			this.baseTarget = baseTarget;
			this.nextBlockId = nextBlockId;
			this.height = height;
			this.id = id;
		}

		public override int Version
		{
			get
			{
				return version;
			}
		}

		public override int Timestamp
		{
			get
			{
				return timestamp;
			}
		}

		public override long PreviousBlockId
		{
			get
			{
				return previousBlockId;
			}
		}

		public override sbyte[] GeneratorPublicKey
		{
			get
			{
				return generatorPublicKey;
			}
		}

		public override sbyte[] PreviousBlockHash
		{
			get
			{
				return previousBlockHash;
			}
		}

		public override long TotalAmountNQT
		{
			get
			{
				return totalAmountNQT;
			}
		}

		public override long TotalFeeNQT
		{
			get
			{
				return totalFeeNQT;
			}
		}

		public override int PayloadLength
		{
			get
			{
				return payloadLength;
			}
		}

		public override sbyte[] PayloadHash
		{
			get
			{
				return payloadHash;
			}
		}

		public override sbyte[] GenerationSignature
		{
			get
			{
				return generationSignature;
			}
		}

		public override sbyte[] BlockSignature
		{
			get
			{
				return blockSignature;
			}
		}

		public override IList<TransactionImpl> Transactions
		{
			get
			{
				if(blockTransactions == null)
				{
					this.blockTransactions = Collections.unmodifiableList(TransactionDb.findBlockTransactions(Id));
					foreach (TransactionImpl transaction in this.blockTransactions)
					{
						transaction.Block = this;
					}
				}
				return blockTransactions;
			}
		}

		public override long BaseTarget
		{
			get
			{
				return baseTarget;
			}
		}

		public override BigInteger CumulativeDifficulty
		{
			get
			{
				return cumulativeDifficulty;
			}
		}

		public override long NextBlockId
		{
			get
			{
				return nextBlockId;
			}
		}

		public override int Height
		{
			get
			{
				if(height == -1)
				{
					throw new InvalidOperationException("Block height not yet set");
				}
				return height;
			}
		}

		public override long Id
		{
			get
			{
				if(id == 0)
				{
					if(blockSignature == null)
					{
						throw new InvalidOperationException("Block is not signed yet");
					}
					sbyte[] hash = Crypto.sha256().digest(Bytes);
					BigInteger bigInteger = new BigInteger(1, new sbyte[] {hash[7], hash[6], hash[5], hash[4], hash[3], hash[2], hash[1], hash[0]});
					id = (long)bigInteger;
					stringId = bigInteger.ToString();
				}
				return id;
			}
		}

		public override string StringId
		{
			get
			{
				if(stringId == null)
				{
					Id;
					if(stringId == null)
					{
						stringId = Convert.toUnsignedLong(id);
					}
				}
				return stringId;
			}
		}

		public override long GeneratorId
		{
			get
			{
				if(generatorId == 0)
				{
					generatorId = Account.getId(generatorPublicKey);
				}
				return generatorId;
			}
		}

		public override bool Equals(object o)
		{
			return o is BlockImpl && this.Id == ((BlockImpl)o).Id;
		}

		public override int GetHashCode()
		{
			return(int)(Id ^ ((int)((uint)Id >> 32)));
		}

		public override JSONObject JSONObject
		{
			get
			{
				JSONObject json = new JSONObject();
				json.put("version", version);
				json.put("timestamp", timestamp);
				json.put("previousBlock", Convert.toUnsignedLong(previousBlockId));
				json.put("totalAmountNQT", totalAmountNQT);
				json.put("totalFeeNQT", totalFeeNQT);
				json.put("payloadLength", payloadLength);
				json.put("payloadHash", Convert.toHexString(payloadHash));
				json.put("generatorPublicKey", Convert.toHexString(generatorPublicKey));
				json.put("generationSignature", Convert.toHexString(generationSignature));
				if(version > 1)
				{
					json.put("previousBlockHash", Convert.toHexString(previousBlockHash));
				}
				json.put("blockSignature", Convert.toHexString(blockSignature));
				JSONArray transactionsData = new JSONArray();
				foreach (Transaction transaction in Transactions)
				{
					transactionsData.add(transaction.JSONObject);
				}
				json.put("transactions", transactionsData);
				return json;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static BlockImpl parseBlock(JSONObject blockData) throws NxtException.ValidationException
		static BlockImpl parseBlock(JSONObject blockData)
		{
			try
			{
				int version = (int)((long?) blockData.get("version"));
				int timestamp = (int)((long?) blockData.get("timestamp"));
				long previousBlock = Convert.parseUnsignedLong((string) blockData.get("previousBlock"));
				long totalAmountNQT = Convert.parseLong(blockData.get("totalAmountNQT"));
				long totalFeeNQT = Convert.parseLong(blockData.get("totalFeeNQT"));
				int payloadLength = (int)((long?) blockData.get("payloadLength"));
				sbyte[] payloadHash = Convert.parseHexString((string) blockData.get("payloadHash"));
				sbyte[] generatorPublicKey = Convert.parseHexString((string) blockData.get("generatorPublicKey"));
				sbyte[] generationSignature = Convert.parseHexString((string) blockData.get("generationSignature"));
				sbyte[] blockSignature = Convert.parseHexString((string) blockData.get("blockSignature"));
				sbyte[] previousBlockHash = version == 1 ? null : Convert.parseHexString((string) blockData.get("previousBlockHash"));
				IList<TransactionImpl> blockTransactions = new List<>();
				foreach (object transactionData in (JSONArray) blockData.get("transactions"))
				{
					blockTransactions.Add(TransactionImpl.parseTransaction((JSONObject) transactionData));
				}
				return new BlockImpl(version, timestamp, previousBlock, totalAmountNQT, totalFeeNQT, payloadLength, payloadHash, generatorPublicKey, generationSignature, blockSignature, previousBlockHash, blockTransactions);
			}
			catch(NxtException.ValidationException|Exception e)
			{
				Logger.logDebugMessage("Failed to parse block: " + blockData.toJSONString());
				throw e;
			}
		}

		internal sbyte[] Bytes
		{
			get
			{
				ByteBuffer buffer = ByteBuffer.allocate(4 + 4 + 8 + 4 + (version < 3 ? (4 + 4) : (8 + 8)) + 4 + 32 + 32 + (32 + 32) + 64);
				buffer.order(ByteOrder.LITTLE_ENDIAN);
				buffer.putInt(version);
				buffer.putInt(timestamp);
				buffer.putLong(previousBlockId);
				buffer.putInt(Transactions.Count);
				if(version < 3)
				{
					buffer.putInt((int)(totalAmountNQT / Constants.ONE_NXT));
					buffer.putInt((int)(totalFeeNQT / Constants.ONE_NXT));
				}
				else
				{
					buffer.putLong(totalAmountNQT);
					buffer.putLong(totalFeeNQT);
				}
				buffer.putInt(payloadLength);
				buffer.put(payloadHash);
				buffer.put(generatorPublicKey);
				buffer.put(generationSignature);
				if(version > 1)
				{
					buffer.put(previousBlockHash);
				}
				buffer.put(blockSignature);
				return buffer.array();
			}
		}

		internal void sign(string secretPhrase)
		{
			if(blockSignature != null)
			{
				throw new InvalidOperationException("Block already signed");
			}
			blockSignature = new sbyte[64];
			sbyte[] data = Bytes;
			sbyte[] data2 = new sbyte[data.Length - 64];
			Array.Copy(data, 0, data2, 0, data2.Length);
			blockSignature = Crypto.sign(data2, secretPhrase);
		}

		internal bool verifyBlockSignature()
		{

			Account account = Account.getAccount(GeneratorId);
			if(account == null)
			{
				return false;
			}

			sbyte[] data = Bytes;
			sbyte[] data2 = new sbyte[data.Length - 64];
			Array.Copy(data, 0, data2, 0, data2.Length);

			return Crypto.verify(blockSignature, data2, generatorPublicKey, version >= 3) && account.setOrVerify(generatorPublicKey, this.height);

		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: boolean verifyGenerationSignature() throws BlockchainProcessor.BlockOutOfOrderException
		internal bool verifyGenerationSignature()
		{

			try
			{

				BlockImpl previousBlock = (BlockImpl) Nxt.Blockchain.getBlock(this.previousBlockId);
				if(previousBlock == null)
				{
					throw new BlockchainProcessor.BlockOutOfOrderException("Can't verify signature because previous block is missing");
				}

				if(version == 1 && !Crypto.verify(generationSignature, previousBlock.generationSignature, generatorPublicKey, version >= 3))
				{
					return false;
				}

				Account account = Account.getAccount(GeneratorId);
				long effectiveBalance = account == null ? 0 : account.EffectiveBalanceNXT;
				if(effectiveBalance <= 0)
				{
					return false;
				}

				MessageDigest digest = Crypto.sha256();
				sbyte[] generationSignatureHash;
				if(version == 1)
				{
					generationSignatureHash = digest.digest(generationSignature);
				}
				else
				{
					digest.update(previousBlock.generationSignature);
					generationSignatureHash = digest.digest(generatorPublicKey);
					if(!Array.Equals(generationSignature, generationSignatureHash))
					{
						return false;
					}
				}

				BigInteger hit = new BigInteger(1, new sbyte[]{generationSignatureHash[7], generationSignatureHash[6], generationSignatureHash[5], generationSignatureHash[4], generationSignatureHash[3], generationSignatureHash[2], generationSignatureHash[1], generationSignatureHash[0]});

				return Generator.verifyHit(hit, BigInteger.valueOf(effectiveBalance), previousBlock, timestamp) || (this.height < Constants.TRANSPARENT_FORGING_BLOCK_5 && Array.BinarySearch(badBlocks, this.Id) >= 0);

			}
			catch(Exception e)
			{

				Logger.logMessage("Error verifying block generation signature", e);
				return false;

			}

		}

		private static readonly long[] badBlocks = new long[] { 5113090348579089956L, 8032405266942971936L, 7702042872885598917L, -407022268390237559L, -3320029330888410250L, -6568770202903512165L, 4288642518741472722L, 5315076199486616536L, -6175599071600228543L};
		static BlockImpl()
		{
			Array.Sort(badBlocks);
		}

		internal void apply()
		{
			Account generatorAccount = Account.addOrGetAccount(GeneratorId);
			generatorAccount.apply(generatorPublicKey, this.height);
			generatorAccount.addToBalanceAndUnconfirmedBalanceNQT(totalFeeNQT);
			generatorAccount.addToForgedBalanceNQT(totalFeeNQT);
			foreach (TransactionImpl transaction in Transactions)
			{
				transaction.apply();
			}
		}

		internal BlockImpl Previous
		{
			set
			{
				if(value != null)
				{
					if(value.Id != PreviousBlockId)
					{
					// shouldn't happen as previous id is already verified, but just in case
                        throw new InvalidOperationException("Previous block id doesn't match");
					}
					this.height = value.Height + 1;
					this.calculateBaseTarget(value);
				}
				else
				{
					this.height = 0;
				}
				foreach (TransactionImpl transaction in Transactions)
				{
					transaction.Block = this;
				}
			}
		}

		private void calculateBaseTarget(BlockImpl previousBlock)
		{

			if(this.Id == Genesis.GENESIS_BLOCK_ID && previousBlockId == 0)
			{
				baseTarget = Constants.INITIAL_BASE_TARGET;
				cumulativeDifficulty = BigInteger.ZERO;
			}
			else
			{
				long curBaseTarget = previousBlock.baseTarget;
				long newBaseTarget = (long)BigInteger.valueOf(curBaseTarget).multiply(BigInteger.valueOf(this.timestamp - previousBlock.timestamp)).divide(BigInteger.valueOf(60));
				if(newBaseTarget < 0 || newBaseTarget > Constants.MAX_BASE_TARGET)
				{
					newBaseTarget = Constants.MAX_BASE_TARGET;
				}
				if(newBaseTarget < curBaseTarget / 2)
				{
					newBaseTarget = curBaseTarget / 2;
				}
				if(newBaseTarget == 0)
				{
					newBaseTarget = 1;
				}
				long twofoldCurBaseTarget = curBaseTarget * 2;
				if(twofoldCurBaseTarget < 0)
				{
					twofoldCurBaseTarget = Constants.MAX_BASE_TARGET;
				}
				if(newBaseTarget > twofoldCurBaseTarget)
				{
					newBaseTarget = twofoldCurBaseTarget;
				}
				baseTarget = newBaseTarget;
				cumulativeDifficulty = previousBlock.cumulativeDifficulty.add(Convert.two64.divide(BigInteger.valueOf(baseTarget)));
			}
		}

	}

}