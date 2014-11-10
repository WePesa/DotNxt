using System;
using System.Collections.Generic;

namespace nxt
{

	using Crypto = nxt.crypto.Crypto;
	using DbKey = nxt.db.DbKey;
	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;


	internal sealed class TransactionImpl : Transaction
	{

		internal sealed class BuilderImpl : Builder
		{

			private readonly short deadline;
			private readonly sbyte[] senderPublicKey;
			private readonly long amountNQT;
			private readonly long feeNQT;
			private readonly TransactionType type;
			private readonly sbyte version;
			private readonly int timestamp;
			private readonly Attachment.AbstractAttachment attachment;

			private long recipientId;
			private string referencedTransactionFullHash;
			private sbyte[] signature;
			private Appendix.Message message;
			private Appendix.EncryptedMessage encryptedMessage;
			private Appendix.EncryptToSelfMessage encryptToSelfMessage;
			private Appendix.PublicKeyAnnouncement publicKeyAnnouncement;
			private long blockId;
			private int height = int.MAX_VALUE;
			private long id;
			private long senderId;
			private int blockTimestamp = -1;
			private string fullHash;
			private int ecBlockHeight;
			private long ecBlockId;

			internal BuilderImpl(sbyte version, sbyte[] senderPublicKey, long amountNQT, long feeNQT, int timestamp, short deadline, Attachment.AbstractAttachment attachment)
			{
				this.version = version;
				this.timestamp = timestamp;
				this.deadline = deadline;
				this.senderPublicKey = senderPublicKey;
				this.amountNQT = amountNQT;
				this.feeNQT = feeNQT;
				this.attachment = attachment;
				this.type = attachment.TransactionType;
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public TransactionImpl build() throws NxtException.NotValidException
			public override TransactionImpl build()
			{
				return new TransactionImpl(this);
			}

			public override BuilderImpl recipientId(long recipientId)
			{
				this.recipientId = recipientId;
				return this;
			}

			public override BuilderImpl referencedTransactionFullHash(string referencedTransactionFullHash)
			{
				this.referencedTransactionFullHash = referencedTransactionFullHash;
				return this;
			}

			internal BuilderImpl referencedTransactionFullHash(sbyte[] referencedTransactionFullHash)
			{
				if(referencedTransactionFullHash != null)
				{
					this.referencedTransactionFullHash = Convert.toHexString(referencedTransactionFullHash);
				}
				return this;
			}

			public override BuilderImpl message(Appendix.Message message)
			{
				this.message = message;
				return this;
			}

			public override BuilderImpl encryptedMessage(Appendix.EncryptedMessage encryptedMessage)
			{
				this.encryptedMessage = encryptedMessage;
				return this;
			}

			public override BuilderImpl encryptToSelfMessage(Appendix.EncryptToSelfMessage encryptToSelfMessage)
			{
				this.encryptToSelfMessage = encryptToSelfMessage;
				return this;
			}

			public override BuilderImpl publicKeyAnnouncement(Appendix.PublicKeyAnnouncement publicKeyAnnouncement)
			{
				this.publicKeyAnnouncement = publicKeyAnnouncement;
				return this;
			}

			internal BuilderImpl id(long id)
			{
				this.id = id;
				return this;
			}

			internal BuilderImpl signature(sbyte[] signature)
			{
				this.signature = signature;
				return this;
			}

			internal BuilderImpl blockId(long blockId)
			{
				this.blockId = blockId;
				return this;
			}

			internal BuilderImpl height(int height)
			{
				this.height = height;
				return this;
			}

			internal BuilderImpl senderId(long senderId)
			{
				this.senderId = senderId;
				return this;
			}

			internal BuilderImpl fullHash(string fullHash)
			{
				this.fullHash = fullHash;
				return this;
			}

			internal BuilderImpl fullHash(sbyte[] fullHash)
			{
				if(fullHash != null)
				{
					this.fullHash = Convert.toHexString(fullHash);
				}
				return this;
			}

			internal BuilderImpl blockTimestamp(int blockTimestamp)
			{
				this.blockTimestamp = blockTimestamp;
				return this;
			}

			internal BuilderImpl ecBlockHeight(int height)
			{
				this.ecBlockHeight = height;
				return this;
			}

			internal BuilderImpl ecBlockId(long blockId)
			{
				this.ecBlockId = blockId;
				return this;
			}

		}

		private readonly short deadline;
		private readonly sbyte[] senderPublicKey;
		private readonly long recipientId;
		private readonly long amountNQT;
		private readonly long feeNQT;
		private readonly string referencedTransactionFullHash;
		private readonly TransactionType type;
		private readonly int ecBlockHeight;
		private readonly long ecBlockId;
		private readonly sbyte version;
		private readonly int timestamp;
		private readonly Attachment.AbstractAttachment attachment;
		private readonly Appendix.Message message;
		private readonly Appendix.EncryptedMessage encryptedMessage;
		private readonly Appendix.EncryptToSelfMessage encryptToSelfMessage;
		private readonly Appendix.PublicKeyAnnouncement publicKeyAnnouncement;

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: private final List<? extends Appendix.AbstractAppendix> appendages;
		private readonly IList<?> appendages;
		private readonly int appendagesSize;

		private volatile int height = int.MAX_VALUE;
		private volatile long blockId;
		private volatile Block block;
		private volatile sbyte[] signature;
		private volatile int blockTimestamp = -1;
		private volatile long id;
		private volatile string stringId;
		private volatile long senderId;
		private volatile string fullHash;
		private volatile DbKey dbKey;

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private TransactionImpl(BuilderImpl builder) throws NxtException.NotValidException
		private TransactionImpl(BuilderImpl builder)
		{

			this.timestamp = builder.timestamp;
			this.deadline = builder.deadline;
			this.senderPublicKey = builder.senderPublicKey;
			this.recipientId = builder.recipientId;
			this.amountNQT = builder.amountNQT;
			this.referencedTransactionFullHash = builder.referencedTransactionFullHash;
			this.signature = builder.signature;
			this.type = builder.type;
			this.version = builder.version;
			this.blockId = builder.blockId;
			this.height = builder.height;
			this.id = builder.id;
			this.senderId = builder.senderId;
			this.blockTimestamp = builder.blockTimestamp;
			this.fullHash = builder.fullHash;
			this.ecBlockHeight = builder.ecBlockHeight;
			this.ecBlockId = builder.ecBlockId;

			IList<Appendix.AbstractAppendix> list = new List<>();
			if((this.attachment = builder.attachment) != null)
			{
				list.Add(this.attachment);
			}
			if((this.message = builder.message) != null)
			{
				list.Add(this.message);
			}
			if((this.encryptedMessage = builder.encryptedMessage) != null)
			{
				list.Add(this.encryptedMessage);
			}
			if((this.publicKeyAnnouncement = builder.publicKeyAnnouncement) != null)
			{
				list.Add(this.publicKeyAnnouncement);
			}
			if((this.encryptToSelfMessage = builder.encryptToSelfMessage) != null)
			{
				list.Add(this.encryptToSelfMessage);
			}
			this.appendages = Collections.unmodifiableList(list);
			int appendagesSize = 0;
			foreach (Appendix appendage in appendages)
			{
				appendagesSize += appendage.Size;
			}
			this.appendagesSize = appendagesSize;
			int effectiveHeight = (height < int.MaxValue ? height : Nxt.Blockchain.Height);
			long minimumFeeNQT = type.minimumFeeNQT(effectiveHeight, appendagesSize);
			if(builder.feeNQT > 0 && builder.feeNQT < minimumFeeNQT)
			{
				throw new NxtException.NotValidException(string.Format("Requested fee {0:D} less than the minimum fee {1:D}", builder.feeNQT, minimumFeeNQT));
			}
			if(builder.feeNQT <= 0)
			{
				feeNQT = minimumFeeNQT;
			}
			else
			{
				feeNQT = builder.feeNQT;
			}

			if((timestamp == 0 && Array.Equals(senderPublicKey, Genesis.CREATOR_PUBLIC_KEY)) ? (deadline != 0 || feeNQT != 0) : deadline < 1 || feeNQT > Constants.MAX_BALANCE_NQT || amountNQT < 0 || amountNQT > Constants.MAX_BALANCE_NQT || type == null)
			{
				throw new NxtException.NotValidException("Invalid transaction parameters:\n type: " + type + ", timestamp: " + timestamp + ", deadline: " + deadline + ", fee: " + feeNQT + ", amount: " + amountNQT);
			}

			if(attachment == null || type != attachment.TransactionType)
			{
				throw new NxtException.NotValidException("Invalid attachment " + attachment + " for transaction of type " + type);
			}

			if(! type.hasRecipient())
			{
				if(recipientId != 0 || AmountNQT != 0)
				{
					throw new NxtException.NotValidException("Transactions of this type must have recipient == Genesis, amount == 0");
				}
			}

			foreach (Appendix.AbstractAppendix appendage in appendages)
			{
				if(! appendage.verifyVersion(this.version))
				{
					throw new NxtException.NotValidException("Invalid attachment version " + appendage.Version + " for transaction version " + this.version);
				}
			}

		}

		public override short Deadline
		{
			get
			{
				return deadline;
			}
		}

		public override sbyte[] SenderPublicKey
		{
			get
			{
				return senderPublicKey;
			}
		}

		public override long RecipientId
		{
			get
			{
				return recipientId;
			}
		}

		public override long AmountNQT
		{
			get
			{
				return amountNQT;
			}
		}

		public override long FeeNQT
		{
			get
			{
				return feeNQT;
			}
		}

		public override string ReferencedTransactionFullHash
		{
			get
			{
				return referencedTransactionFullHash;
			}
		}

		public override int Height
		{
			get
			{
				return height;
			}
			set
			{
				this.height = value;
			}
		}


		public override sbyte[] Signature
		{
			get
			{
				return signature;
			}
		}

		public override TransactionType Type
		{
			get
			{
				return type;
			}
		}

		public override sbyte Version
		{
			get
			{
				return version;
			}
		}

		public override long BlockId
		{
			get
			{
				return blockId;
			}
		}

		public override Block Block
		{
			get
			{
				if(block == null && blockId != 0)
				{
					block = Nxt.Blockchain.getBlock(blockId);
				}
				return block;
			}
			set
			{
				this.block = value;
				this.blockId = value.Id;
				this.height = value.Height;
				this.blockTimestamp = value.Timestamp;
			}
		}


		internal void unsetBlock()
		{
			this.block = null;
			this.blockId = 0;
			this.blockTimestamp = -1;
		// must keep the height set, as transactions already having been included in a popped-off block before
		// get priority when sorted for inclusion in a new block
		}

		public override int Timestamp
		{
			get
			{
				return timestamp;
			}
		}

		public override int BlockTimestamp
		{
			get
			{
				return blockTimestamp;
			}
		}

		public override int Expiration
		{
			get
			{
				return timestamp + deadline * 60;
			}
		}

		public override Attachment Attachment
		{
			get
			{
				return attachment;
			}
		}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public List<? extends Appendix> getAppendages()
		public override IList<?> getAppendages()
		{
			get
	//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
	//ORIGINAL LINE: public List<? extends Appendix> getAppendages()
			{
				return appendages;
			}
		}

		public override long Id
		{
			get
			{
				if(id == 0)
				{
					if(signature == null)
					{
						throw new InvalidOperationException("Transaction is not signed yet");
					}
					sbyte[] hash;
					if(useNQT())
					{
						sbyte[] data = zeroSignature(Bytes);
						sbyte[] signatureHash = Crypto.sha256().digest(signature);
						MessageDigest digest = Crypto.sha256();
						digest.update(data);
						hash = digest.digest(signatureHash);
					}
					else
					{
						hash = Crypto.sha256().digest(Bytes);
					}
					BigInteger bigInteger = new BigInteger(1, new sbyte[] {hash[7], hash[6], hash[5], hash[4], hash[3], hash[2], hash[1], hash[0]});
					id = (long)bigInteger;
					stringId = bigInteger.ToString();
					fullHash = Convert.toHexString(hash);
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

		public override string FullHash
		{
			get
			{
				if(fullHash == null)
				{
					Id;
				}
				return fullHash;
			}
		}

		public override long SenderId
		{
			get
			{
				if(senderId == 0)
				{
					senderId = Account.getId(senderPublicKey);
				}
				return senderId;
			}
		}

		internal DbKey DbKey
		{
			get
			{
				if(dbKey == null)
				{
					dbKey = TransactionProcessorImpl.Instance.unconfirmedTransactionDbKeyFactory.newKey(Id);
				}
				return dbKey;
			}
		}

		public override Appendix.Message Message
		{
			get
			{
				return message;
			}
		}

		public override Appendix.EncryptedMessage EncryptedMessage
		{
			get
			{
				return encryptedMessage;
			}
		}

		public override Appendix.EncryptToSelfMessage EncryptToSelfMessage
		{
			get
			{
				return encryptToSelfMessage;
			}
		}

		internal Appendix.PublicKeyAnnouncement PublicKeyAnnouncement
		{
			get
			{
				return publicKeyAnnouncement;
			}
		}

		public override sbyte[] Bytes
		{
			get
			{
				try
				{
					ByteBuffer buffer = ByteBuffer.allocate(Size);
					buffer.order(ByteOrder.LITTLE_ENDIAN);
					buffer.put(type.Type);
					buffer.put((sbyte)((version << 4) | type.Subtype));
					buffer.putInt(timestamp);
					buffer.putShort(deadline);
					buffer.put(senderPublicKey);
					buffer.putLong(type.hasRecipient() ? recipientId : Genesis.CREATOR_ID);
					if(useNQT())
					{
						buffer.putLong(amountNQT);
						buffer.putLong(feeNQT);
						if(referencedTransactionFullHash != null)
						{
							buffer.put(Convert.parseHexString(referencedTransactionFullHash));
						}
						else
						{
							buffer.put(new sbyte[32]);
						}
					}
					else
					{
						buffer.putInt((int)(amountNQT / Constants.ONE_NXT));
						buffer.putInt((int)(feeNQT / Constants.ONE_NXT));
						if(referencedTransactionFullHash != null)
						{
							buffer.putLong(Convert.fullHashToId(Convert.parseHexString(referencedTransactionFullHash)));
						}
						else
						{
							buffer.putLong(0L);
						}
					}
					buffer.put(signature != null ? signature : new sbyte[64]);
					if(version > 0)
					{
						buffer.putInt(Flags);
						buffer.putInt(ecBlockHeight);
						buffer.putLong(ecBlockId);
					}
					foreach (Appendix appendage in appendages)
					{
						appendage.putBytes(buffer);
					}
					return buffer.array();
				}
				catch(Exception e)
				{
					Logger.logDebugMessage("Failed to get transaction bytes for transaction: " + JSONObject.toJSONString());
					throw e;
				}
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static TransactionImpl parseTransaction(byte[] bytes) throws NxtException.ValidationException
		static TransactionImpl parseTransaction(sbyte[] bytes)
		{
			try
			{
				ByteBuffer buffer = ByteBuffer.wrap(bytes);
				buffer.order(ByteOrder.LITTLE_ENDIAN);
				sbyte type = buffer.get();
				sbyte subtype = buffer.get();
				sbyte version = (sbyte)((subtype & 0xF0) >> 4);
				subtype = (sbyte)(subtype & 0x0F);
				int timestamp = buffer.Int;
				short deadline = buffer.Short;
				sbyte[] senderPublicKey = new sbyte[32];
				buffer.get(senderPublicKey);
				long recipientId = buffer.Long;
				long amountNQT = buffer.Long;
				long feeNQT = buffer.Long;
				string referencedTransactionFullHash = null;
				sbyte[] referencedTransactionFullHashBytes = new sbyte[32];
				buffer.get(referencedTransactionFullHashBytes);
				if(Convert.emptyToNull(referencedTransactionFullHashBytes) != null)
				{
					referencedTransactionFullHash = Convert.toHexString(referencedTransactionFullHashBytes);
				}
				sbyte[] signature = new sbyte[64];
				buffer.get(signature);
				signature = Convert.emptyToNull(signature);
				int flags = 0;
				int ecBlockHeight = 0;
				long ecBlockId = 0;
				if(version > 0)
				{
					flags = buffer.Int;
					ecBlockHeight = buffer.Int;
					ecBlockId = buffer.Long;
				}
				TransactionType transactionType = TransactionType.findTransactionType(type, subtype);
				TransactionImpl.BuilderImpl builder = new TransactionImpl.BuilderImpl(version, senderPublicKey, amountNQT, feeNQT, timestamp, deadline, transactionType.parseAttachment(buffer, version)).referencedTransactionFullHash(referencedTransactionFullHash).signature(signature).ecBlockHeight(ecBlockHeight).ecBlockId(ecBlockId);
				if(transactionType.hasRecipient())
				{
					builder.recipientId(recipientId);
				}
				int position = 1;
				if((flags & position) != 0 || (version == 0 && transactionType == TransactionType.Messaging.ARBITRARY_MESSAGE))
				{
					builder.message(new Appendix.Message(buffer, version));
				}
				position <<= 1;
				if((flags & position) != 0)
				{
					builder.encryptedMessage(new Appendix.EncryptedMessage(buffer, version));
				}
				position <<= 1;
				if((flags & position) != 0)
				{
					builder.publicKeyAnnouncement(new Appendix.PublicKeyAnnouncement(buffer, version));
				}
				position <<= 1;
				if((flags & position) != 0)
				{
					builder.encryptToSelfMessage(new Appendix.EncryptToSelfMessage(buffer, version));
				}
				return builder.build();
			}
			catch(NxtException.NotValidException|Exception e)
			{
				Logger.logDebugMessage("Failed to parse transaction bytes: " + Convert.toHexString(bytes));
				throw e;
			}
		}

		public override sbyte[] UnsignedBytes
		{
			get
			{
				return zeroSignature(Bytes);
			}
		}

//    
//    @Override
//    public Collection<TransactionType> getPhasingTransactionTypes() {
//        return getType().getPhasingTransactionTypes();
//    }
//
//    @Override
//    public Collection<TransactionType> getPhasedTransactionTypes() {
//        return getType().getPhasedTransactionTypes();
//    }
//    

		public override JSONObject JSONObject
		{
			get
			{
				JSONObject json = new JSONObject();
				json.put("type", type.Type);
				json.put("subtype", type.Subtype);
				json.put("timestamp", timestamp);
				json.put("deadline", deadline);
				json.put("senderPublicKey", Convert.toHexString(senderPublicKey));
				if(type.hasRecipient())
				{
					json.put("recipient", Convert.toUnsignedLong(recipientId));
				}
				json.put("amountNQT", amountNQT);
				json.put("feeNQT", feeNQT);
				if(referencedTransactionFullHash != null)
				{
					json.put("referencedTransactionFullHash", referencedTransactionFullHash);
				}
				json.put("ecBlockHeight", ecBlockHeight);
				json.put("ecBlockId", Convert.toUnsignedLong(ecBlockId));
				json.put("signature", Convert.toHexString(signature));
				JSONObject attachmentJSON = new JSONObject();
				foreach (Appendix appendage in appendages)
				{
					attachmentJSON.putAll(appendage.JSONObject);
				}
				if(! attachmentJSON.Empty)
				{
					json.put("attachment", attachmentJSON);
				}
				json.put("version", version);
				return json;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static TransactionImpl parseTransaction(JSONObject transactionData) throws NxtException.NotValidException
		static TransactionImpl parseTransaction(JSONObject transactionData)
		{
			try
			{
				sbyte type = (sbyte)((long?) transactionData.get("type"));
				sbyte subtype = (sbyte)((long?) transactionData.get("subtype"));
				int timestamp = (int)((long?) transactionData.get("timestamp"));
				short deadline = (short)((long?) transactionData.get("deadline"));
				sbyte[] senderPublicKey = Convert.parseHexString((string) transactionData.get("senderPublicKey"));
				long amountNQT = Convert.parseLong(transactionData.get("amountNQT"));
				long feeNQT = Convert.parseLong(transactionData.get("feeNQT"));
				string referencedTransactionFullHash = (string) transactionData.get("referencedTransactionFullHash");
				sbyte[] signature = Convert.parseHexString((string) transactionData.get("signature"));
				long? versionValue = (long?) transactionData.get("version");
				sbyte version = versionValue == null ? 0 : (sbyte)versionValue;
				JSONObject attachmentData = (JSONObject) transactionData.get("attachment");

				TransactionType transactionType = TransactionType.findTransactionType(type, subtype);
				if(transactionType == null)
				{
					throw new NxtException.NotValidException("Invalid transaction type: " + type + ", " + subtype);
				}
				TransactionImpl.BuilderImpl builder = new TransactionImpl.BuilderImpl(version, senderPublicKey, amountNQT, feeNQT, timestamp, deadline, transactionType.parseAttachment(attachmentData)).referencedTransactionFullHash(referencedTransactionFullHash).signature(signature);
				if(transactionType.hasRecipient())
				{
					long recipientId = Convert.parseUnsignedLong((string) transactionData.get("recipient"));
					builder.recipientId(recipientId);
				}
				if(attachmentData != null)
				{
					builder.message(Appendix.Message.parse(attachmentData));
					builder.encryptedMessage(Appendix.EncryptedMessage.parse(attachmentData));
					builder.publicKeyAnnouncement((Appendix.PublicKeyAnnouncement.parse(attachmentData)));
					builder.encryptToSelfMessage(Appendix.EncryptToSelfMessage.parse(attachmentData));
				}
				if(version > 0)
				{
					builder.ecBlockHeight((int)((long?) transactionData.get("ecBlockHeight")));
					builder.ecBlockId(Convert.parseUnsignedLong((string) transactionData.get("ecBlockId")));
				}
				return builder.build();
			}
			catch(NxtException.NotValidException|Exception e)
			{
				Logger.logDebugMessage("Failed to parse transaction: " + transactionData.toJSONString());
				throw e;
			}
		}


		public override int ECBlockHeight
		{
			get
			{
				return ecBlockHeight;
			}
		}

		public override long ECBlockId
		{
			get
			{
				return ecBlockId;
			}
		}

		public override void sign(string secretPhrase)
		{
			if(signature != null)
			{
				throw new InvalidOperationException("Transaction already signed");
			}
			signature = Crypto.sign(Bytes, secretPhrase);
		}

		public override bool Equals(object o)
		{
			return o is TransactionImpl && this.Id == ((Transaction)o).Id;
		}

		public override int GetHashCode()
		{
			return(int)(Id ^ ((int)((uint)Id >> 32)));
		}

		public override int compareTo(Transaction other)
		{
			return long.compare(this.Id, other.Id);
		}

		public bool verifySignature()
		{
			Account account = Account.getAccount(SenderId);
			if(account == null)
			{
				return false;
			}
			if(signature == null)
			{
				return false;
			}
			sbyte[] data = zeroSignature(Bytes);
			return Crypto.verify(signature, data, senderPublicKey, useNQT()) && account.setOrVerify(senderPublicKey, this.Height);
		}

		internal int Size
		{
			get
			{
				return signatureOffset() + 64 + (version > 0 ? 4 + 4 + 8 : 0) + appendagesSize;
			}
		}

		private int signatureOffset()
		{
			return 1 + 1 + 4 + 2 + 32 + 8 + (useNQT() ? 8 + 8 + 32 : 4 + 4 + 8);
		}

		private bool useNQT()
		{
			return this.height > Constants.NQT_BLOCK && (this.height < int.MaxValue || Nxt.Blockchain.Height >= Constants.NQT_BLOCK);
		}

		private sbyte[] zeroSignature(sbyte[] data)
		{
			int start = signatureOffset();
			for(int i = start; i < start + 64; i++)
			{
				data[i] = 0;
			}
			return data;
		}

		private int Flags
		{
			get
			{
				int flags = 0;
				int position = 1;
				if(message != null)
				{
					flags |= position;
				}
				position <<= 1;
				if(encryptedMessage != null)
				{
					flags |= position;
				}
				position <<= 1;
				if(publicKeyAnnouncement != null)
				{
					flags |= position;
				}
				position <<= 1;
				if(encryptToSelfMessage != null)
				{
					flags |= position;
				}
				return flags;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void validate() throws NxtException.ValidationException
		public override void validate()
		{
			foreach (Appendix.AbstractAppendix appendage in appendages)
			{
				appendage.validate(this);
			}
			long minimumFeeNQT = type.minimumFeeNQT(Nxt.Blockchain.Height, appendagesSize);
			if(feeNQT < minimumFeeNQT)
			{
				throw new NxtException.NotCurrentlyValidException(string.Format("Transaction fee {0:D} less than minimum fee {1:D} at height {2:D}", feeNQT, minimumFeeNQT, Nxt.Blockchain.Height));
			}
			if(Nxt.Blockchain.Height >= Constants.PUBLIC_KEY_ANNOUNCEMENT_BLOCK)
			{
			// TODO: allow at next hard fork
				if(type.hasRecipient() && recipientId != 0) // && recipientId != getSenderId()
				{
					Account recipientAccount = Account.getAccount(recipientId);
					if((recipientAccount == null || recipientAccount.PublicKey == null) && publicKeyAnnouncement == null)
					{
						throw new NxtException.NotCurrentlyValidException("Recipient account does not have a public key, must attach a public key announcement");
					}
				}
			}
		}

	// returns false iff double spending
		internal bool applyUnconfirmed()
		{
			Account senderAccount = Account.getAccount(SenderId);
			return senderAccount != null && type.applyUnconfirmed(this, senderAccount);
		}

		internal void apply()
		{
			Account senderAccount = Account.getAccount(SenderId);
			senderAccount.apply(senderPublicKey, this.Height);
			Account recipientAccount = Account.getAccount(recipientId);
			if(recipientAccount == null && recipientId != 0)
			{
				recipientAccount = Account.addOrGetAccount(recipientId);
			}
			foreach (Appendix.AbstractAppendix appendage in appendages)
			{
				appendage.apply(this, senderAccount, recipientAccount);
			}
		}

		internal void undoUnconfirmed()
		{
			Account senderAccount = Account.getAccount(SenderId);
			type.undoUnconfirmed(this, senderAccount);
		}

		internal bool isDuplicate(IDictionary<TransactionType, Set<string>> duplicates)
		{
			return type.isDuplicate(this, duplicates);
		}

	}

}