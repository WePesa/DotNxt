using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using DigitalGoodsStore = nxt.DigitalGoodsStore;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.DUPLICATE_REFUND;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.GOODS_NOT_DELIVERED;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DGS_REFUND;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_PURCHASE;

	public sealed class DGSRefund : CreateTransaction
	{

		internal static readonly DGSRefund instance = new DGSRefund();

		private DGSRefund() : base(new APITag[] {APITag.DGS, APITag.CREATE_TRANSACTION}, "purchase", "refundNQT")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account sellerAccount = ParameterParser.getSenderAccount(req);
			DigitalGoodsStore.Purchase purchase = ParameterParser.getPurchase(req);
			if(sellerAccount.Id != purchase.SellerId)
			{
				return INCORRECT_PURCHASE;
			}
			if(purchase.RefundNote != null)
			{
				return DUPLICATE_REFUND;
			}
			if(purchase.EncryptedGoods == null)
			{
				return GOODS_NOT_DELIVERED;
			}

			string refundValueNQT = Convert.emptyToNull(req.getParameter("refundNQT"));
			long refundNQT = 0;
			try
			{
				if(refundValueNQT != null)
				{
					refundNQT = Convert.ToInt64(refundValueNQT);
				}
			}
			catch(Exception e)
			{
				return INCORRECT_DGS_REFUND;
			}
			if(refundNQT < 0 || refundNQT > Constants.MAX_BALANCE_NQT)
			{
				return INCORRECT_DGS_REFUND;
			}

			Account buyerAccount = Account.getAccount(purchase.BuyerId);

			Attachment attachment = new Attachment.DigitalGoodsRefund(purchase.Id, refundNQT);
			return createTransaction(req, sellerAccount, buyerAccount.Id, 0, attachment);

		}

	}

}