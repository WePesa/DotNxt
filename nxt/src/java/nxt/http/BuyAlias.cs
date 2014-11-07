namespace nxt.http
{

	using Account = nxt.Account;
	using Alias = nxt.Alias;
	using Attachment = nxt.Attachment;
	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ALIAS_NOTFORSALE;


	public sealed class BuyAlias : CreateTransaction
	{

		internal static readonly BuyAlias instance = new BuyAlias();

		private BuyAlias() : base(new APITag[] {APITag.ALIASES, APITag.CREATE_TRANSACTION}, "alias", "aliasName")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			Account buyer = ParameterParser.getSenderAccount(req);
			Alias alias = ParameterParser.getAlias(req);
			long amountNQT = ParameterParser.getAmountNQT(req);
			if(Alias.getOffer(alias) == null)
			{
				return INCORRECT_ALIAS_NOTFORSALE;
			}
			long sellerId = alias.AccountId;
			Attachment attachment = new Attachment.MessagingAliasBuy(alias.AliasName);
			return createTransaction(req, buyer, sellerId, amountNQT, attachment);
		}
	}

}