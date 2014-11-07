using System;

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
	import static nxt.http.JSONResponses.INCORRECT_ASSET_DESCRIPTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ASSET_NAME;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_ASSET_NAME_LENGTH;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_DECIMALS;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_NAME;

	public sealed class IssueAsset : CreateTransaction
	{

		internal static readonly IssueAsset instance = new IssueAsset();

		private IssueAsset() : base(new APITag[] {APITag.AE, APITag.CREATE_TRANSACTION}, "name", "description", "quantityQNT", "decimals")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string name = req.getParameter("name");
			string description = req.getParameter("description");
			string decimalsValue = Convert.emptyToNull(req.getParameter("decimals"));

			if(name == null)
			{
				return MISSING_NAME;
			}

			name = name.Trim();
			if(name.Length < Constants.MIN_ASSET_NAME_LENGTH || name.Length > Constants.MAX_ASSET_NAME_LENGTH)
			{
				return INCORRECT_ASSET_NAME_LENGTH;
			}
			string normalizedName = name.ToLower();
			for(int i = 0; i < normalizedName.Length; i++)
			{
				if(Constants.ALPHABET.IndexOf(normalizedName[i]) < 0)
				{
					return INCORRECT_ASSET_NAME;
				}
			}

			if(description != null && description.Length > Constants.MAX_ASSET_DESCRIPTION_LENGTH)
			{
				return INCORRECT_ASSET_DESCRIPTION;
			}

			sbyte decimals = 0;
			if(decimalsValue != null)
			{
				try
				{
					decimals = Convert.ToByte(decimalsValue);
					if(decimals < 0 || decimals > 8)
					{
						return INCORRECT_DECIMALS;
					}
				}
				catch(NumberFormatException e)
				{
					return INCORRECT_DECIMALS;
				}
			}

			long quantityQNT = ParameterParser.getQuantityQNT(req);
			Account account = ParameterParser.getSenderAccount(req);
			Attachment attachment = new Attachment.ColoredCoinsAssetIssuance(name, description, quantityQNT, decimals);
			return createTransaction(req, account, attachment);

		}

	}

}