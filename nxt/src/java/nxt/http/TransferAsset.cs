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

	public sealed class TransferAsset : CreateTransaction
	{

		internal static readonly TransferAsset instance = new TransferAsset();

		private TransferAsset() : base(new APITag[] {APITag.AE, APITag.CREATE_TRANSACTION}, "recipient", "asset", "quantityQNT")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			long recipient = ParameterParser.getRecipientId(req);

			Asset asset = ParameterParser.getAsset(req);
			long quantityQNT = ParameterParser.getQuantityQNT(req);
			Account account = ParameterParser.getSenderAccount(req);

			long assetBalance = account.getUnconfirmedAssetBalanceQNT(asset.Id);
			if(assetBalance < 0 || quantityQNT > assetBalance)
			{
				return NOT_ENOUGH_ASSETS;
			}

			Attachment attachment = new Attachment.ColoredCoinsAssetTransfer(asset.Id, quantityQNT);
			return createTransaction(req, account, recipient, 0, attachment);

		}

	}

}