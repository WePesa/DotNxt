namespace nxt.http
{

	using Account = nxt.Account;
	using Asset = nxt.Asset;
	using Attachment = nxt.Attachment;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.NOT_ENOUGH_FUNDS;

	public sealed class PlaceBidOrder : CreateTransaction
	{

		internal static readonly PlaceBidOrder instance = new PlaceBidOrder();

		private PlaceBidOrder() : base(new APITag[] {APITag.AE, APITag.CREATE_TRANSACTION}, "asset", "quantityQNT", "priceNQT")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Asset asset = ParameterParser.getAsset(req);
			long priceNQT = ParameterParser.getPriceNQT(req);
			long quantityQNT = ParameterParser.getQuantityQNT(req);
			long feeNQT = ParameterParser.getFeeNQT(req);
			Account account = ParameterParser.getSenderAccount(req);

			try
			{
				if(Convert.safeAdd(feeNQT, Convert.safeMultiply(priceNQT, quantityQNT)) > account.UnconfirmedBalanceNQT)
				{
					return NOT_ENOUGH_FUNDS;
				}
			}
			catch(ArithmeticException e)
			{
				return NOT_ENOUGH_FUNDS;
			}

			Attachment attachment = new Attachment.ColoredCoinsBidOrderPlacement(asset.Id, quantityQNT, priceNQT);
			return createTransaction(req, account, attachment);
		}

	}

}