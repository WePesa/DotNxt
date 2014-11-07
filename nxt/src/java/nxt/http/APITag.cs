namespace nxt.http
{

	public enum APITag
	{

//JAVA TO VB & C# CONVERTER TODO TASK: Enum values cannot be strings in .NET:
		ACCOUNTS("Accounts"), ALIASES("Aliases"), AE("Asset Exchange"), CREATE_TRANSACTION("Create Transaction"),
//JAVA TO VB & C# CONVERTER TODO TASK: Enum values cannot be strings in .NET:
		BLOCKS("Blocks"), DGS("Digital Goods Store"), FORGING("Forging"), INFO("Server Info"), MESSAGES("Messages"),
//JAVA TO VB & C# CONVERTER TODO TASK: Enum values cannot be strings in .NET:
		TRANSACTIONS("Transactions"), TOKENS("Tokens"), VS("Voting System"), UTILS("Utils"), DEBUG("Debug");

//JAVA TO VB & C# CONVERTER TODO TASK: Enums cannot contain fields in .NET:
//		private final String displayName;

//JAVA TO VB & C# CONVERTER TODO TASK: Enums cannot contain methods in .NET:
//		private APITag(String displayName)
//	{
//		this.displayName = displayName;
//	}


	}
	public static partial class EnumExtensionMethods
	{
			public string getDisplayName(this APITag instanceJavaToDotNetTempPropertyGetDisplayName)
		{
			return displayName;
		}
	}

}