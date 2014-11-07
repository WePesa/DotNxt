using System.Collections.Generic;

namespace nxt
{

	using JSONObject = org.json.simple.JSONObject;


	public interface Block
	{

		int Version {get;}

		long Id {get;}

		string StringId {get;}

		int Height {get;}

		int Timestamp {get;}

		long GeneratorId {get;}

		sbyte[] GeneratorPublicKey {get;}

		long PreviousBlockId {get;}

		sbyte[] PreviousBlockHash {get;}

		long NextBlockId {get;}

		long TotalAmountNQT {get;}

		long TotalFeeNQT {get;}

		int PayloadLength {get;}

		sbyte[] PayloadHash {get;}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Transaction> getTransactions();
		IList<?> getTransactions() {get;}
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: List<? extends Transaction> getTransactions();

		sbyte[] GenerationSignature {get;}

		sbyte[] BlockSignature {get;}

		long BaseTarget {get;}

		BigInteger CumulativeDifficulty {get;}

		JSONObject JSONObject {get;}

	}

}