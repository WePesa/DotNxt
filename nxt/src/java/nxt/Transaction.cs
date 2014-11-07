using System.Collections.Generic;

namespace nxt
{

	using JSONObject = org.json.simple.JSONObject;


	public interface Transaction : Comparable<Transaction>
	{

//JAVA TO VB & C# CONVERTER TODO TASK: Interfaces cannot contain types in .NET:
//		public static interface Builder
//	{
//
//		Builder recipientId(long recipientId);
//
//		Builder referencedTransactionFullHash(String referencedTransactionFullHash);
//
//		Builder message(Appendix.Message message);
//
//		Builder encryptedMessage(Appendix.EncryptedMessage encryptedMessage);
//
//		Builder encryptToSelfMessage(Appendix.EncryptToSelfMessage encryptToSelfMessage);
//
//		Builder publicKeyAnnouncement(Appendix.PublicKeyAnnouncement publicKeyAnnouncement);
//
//		Transaction build() throws NxtException.NotValidException;
//
//	}

		long Id {get;}

		string StringId {get;}

		long SenderId {get;}

		sbyte[] SenderPublicKey {get;}

		long RecipientId {get;}

		int Height {get;}

		long BlockId {get;}

		Block Block {get;}

		int Timestamp {get;}

		int BlockTimestamp {get;}

		short Deadline {get;}

		int Expiration {get;}

		long AmountNQT {get;}

		long FeeNQT {get;}

		string ReferencedTransactionFullHash {get;}

		sbyte[] Signature {get;}

		string FullHash {get;}

		TransactionType Type {get;}

		Attachment Attachment {get;}

		void sign(string secretPhrase);

		bool verifySignature();

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void validate() throws NxtException.ValidationException;
		void validate();

		sbyte[] Bytes {get;}

		sbyte[] UnsignedBytes {get;}

		JSONObject JSONObject {get;}

		sbyte Version {get;}

		Appendix.Message Message {get;}

		Appendix.EncryptedMessage EncryptedMessage {get;}

		Appendix.EncryptToSelfMessage EncryptToSelfMessage {get;}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Appendix> getAppendages();
		IList<?> getAppendages() {get;}
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Appendix> getAppendages();

//    
//    Collection<TransactionType> getPhasingTransactionTypes();
//
//    Collection<TransactionType> getPhasedTransactionTypes();
//    

		int ECBlockHeight {get;}

		long ECBlockId {get;}

	}

}