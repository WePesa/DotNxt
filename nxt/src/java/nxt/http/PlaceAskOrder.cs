namespace nxt.http
{

	using Account = nxt.Account;
	using Asset = nxt.Asset;
	using Attachment = nxt.Attachment;
	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.NOT_ENOUGH_ASSETS;

	public sealed class PlaceAskOrder : CreateTransaction
	{

		internal static readonly PlaceAskOrder instance = new PlaceAskOrder();

		private PlaceAskOrder() : base(new APITag[] {APITag.AE, APITag.CREATE_TRANSACTION}, "asset", "quantityQNT", "priceNQT")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Asset asset = ParameterParser.getAsset(req);
			long priceNQT = ParameterParser.getPriceNQT(req);
			long quantityQNT = ParameterParser.getQuantityQNT(req);
			Account account = ParameterParser.getSenderAccount(req);

			long assetBalance = account.getUnconfirmedAssetBalanceQNT(asset.Id);
			if(assetBalance < 0 || quantityQNT > assetBalance)
			{
				return NOT_ENOUGH_ASSETS;
			}

			Attachment attachment = new Attachment.ColoredCoinsAskOrderPlacement(asset.Id, quantityQNT, priceNQT);
			return createTransaction(req, account, attachment);

		}

	}

}