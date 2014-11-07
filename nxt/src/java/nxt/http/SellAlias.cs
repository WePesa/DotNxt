using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Alias = nxt.Alias;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ALIAS_OWNER;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_PRICE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_RECIPIENT;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_PRICE;


	public sealed class SellAlias : CreateTransaction
	{

		internal static readonly SellAlias instance = new SellAlias();

		private SellAlias() : base(new APITag[] {APITag.ALIASES, APITag.CREATE_TRANSACTION}, "alias", "aliasName", "recipient", "priceNQT")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			Alias alias = ParameterParser.getAlias(req);
			Account owner = ParameterParser.getSenderAccount(req);

			string priceValueNQT = Convert.emptyToNull(req.getParameter("priceNQT"));
			if(priceValueNQT == null)
			{
				return MISSING_PRICE;
			}
			long priceNQT;
			try
			{
				priceNQT = Convert.ToInt64(priceValueNQT);
			}
			catch(Exception e)
			{
				return INCORRECT_PRICE;
			}
			if(priceNQT < 0 || priceNQT > Constants.MAX_BALANCE_NQT)
			{
				throw new ParameterException(INCORRECT_PRICE);
			}

			string recipientValue = Convert.emptyToNull(req.getParameter("recipient"));
			long recipientId = 0;
			if(recipientValue != null)
			{
				try
				{
					recipientId = Convert.parseAccountId(recipientValue);
				}
				catch(Exception e)
				{
					return INCORRECT_RECIPIENT;
				}
				if(recipientId == 0)
				{
					return INCORRECT_RECIPIENT;
				}
			}

			if(alias.AccountId != owner.Id)
			{
				return INCORRECT_ALIAS_OWNER;
			}

			Attachment attachment = new Attachment.MessagingAliasSell(alias.AliasName, priceNQT);
			return createTransaction(req, owner, recipientId, 0, attachment);
		}
	}

}