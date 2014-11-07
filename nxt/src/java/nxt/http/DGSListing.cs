namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DGS_LISTING_DESCRIPTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DGS_LISTING_NAME;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DGS_LISTING_TAGS;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_NAME;

	public sealed class DGSListing : CreateTransaction
	{

		internal static readonly DGSListing instance = new DGSListing();

		private DGSListing() : base(new APITag[] {APITag.DGS, APITag.CREATE_TRANSACTION}, "name", "description", "tags", "quantity", "priceNQT")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string name = Convert.emptyToNull(req.getParameter("name"));
			string description = Convert.nullToEmpty(req.getParameter("description"));
			string tags = Convert.nullToEmpty(req.getParameter("tags"));
			long priceNQT = ParameterParser.getPriceNQT(req);
			int quantity = ParameterParser.getGoodsQuantity(req);

			if(name == null)
			{
				return MISSING_NAME;
			}
			name = name.Trim();
			if(name.Length > Constants.MAX_DGS_LISTING_NAME_LENGTH)
			{
				return INCORRECT_DGS_LISTING_NAME;
			}

			if(description.Length > Constants.MAX_DGS_LISTING_DESCRIPTION_LENGTH)
			{
				return INCORRECT_DGS_LISTING_DESCRIPTION;
			}

			if(tags.Length > Constants.MAX_DGS_LISTING_TAGS_LENGTH)
			{
				return INCORRECT_DGS_LISTING_TAGS;
			}

			Account account = ParameterParser.getSenderAccount(req);
			Attachment attachment = new Attachment.DigitalGoodsListing(name, description, tags, quantity, priceNQT);
			return createTransaction(req, account, attachment);

		}

	}

}