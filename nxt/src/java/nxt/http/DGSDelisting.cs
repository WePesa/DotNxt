namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.UNKNOWN_GOODS;

	public sealed class DGSDelisting : CreateTransaction
	{

		internal static readonly DGSDelisting instance = new DGSDelisting();

		private DGSDelisting() : base(new APITag[] {APITag.DGS, APITag.CREATE_TRANSACTION}, "goods")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			Account account = ParameterParser.getSenderAccount(req);
			DigitalGoodsStore.Goods goods = ParameterParser.getGoods(req);
			if(goods.Delisted || goods.SellerId != account.Id)
			{
				return UNKNOWN_GOODS;
			}
			Attachment attachment = new Attachment.DigitalGoodsDelisting(goods.Id);
			return createTransaction(req, account, attachment);
		}

	}

}