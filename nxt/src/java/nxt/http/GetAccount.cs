using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using NxtException = nxt.NxtException;
	using DbIterator = nxt.db.DbIterator;
	using Convert = nxt.util.Convert;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetAccount : APIServlet.APIRequestHandler
	{

		internal static readonly GetAccount instance = new GetAccount();

		private GetAccount() : base(new APITag[] {APITag.ACCOUNTS}, "account")
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			Account account = ParameterParser.getAccount(req);

			JSONObject response = JSONData.accountBalance(account);
			JSONData.putAccount(response, "account", account.Id);

			if(account.PublicKey != null)
			{
				response.put("publicKey", Convert.toHexString(account.PublicKey));
			}
			if(account.Name != null)
			{
				response.put("name", account.Name);
			}
			if(account.Description != null)
			{
				response.put("description", account.Description);
			}
			if(account.CurrentLesseeId != 0)
			{
				JSONData.putAccount(response, "currentLessee", account.CurrentLesseeId);
				response.put("currentLeasingHeightFrom", account.CurrentLeasingHeightFrom);
				response.put("currentLeasingHeightTo", account.CurrentLeasingHeightTo);
				if(account.NextLesseeId != 0)
				{
					JSONData.putAccount(response, "nextLessee", account.NextLesseeId);
					response.put("nextLeasingHeightFrom", account.NextLeasingHeightFrom);
					response.put("nextLeasingHeightTo", account.NextLeasingHeightTo);
				}
			}
			using (DbIterator<Account> lessors = account.Lessors)
			{
				if(lessors.hasNext())
				{
					JSONArray lessorIds = new JSONArray();
					JSONArray lessorIdsRS = new JSONArray();
					while(lessors.hasNext())
					{
						Account lessor = lessors.next();
						lessorIds.add(Convert.toUnsignedLong(lessor.Id));
						lessorIdsRS.add(Convert.rsAccount(lessor.Id));
					}
					response.put("lessors", lessorIds);
					response.put("lessorsRS", lessorIdsRS);
				}
			}

			using (DbIterator<Account.AccountAsset> accountAssets = account.getAssets(0, -1))
			{
				JSONArray assetBalances = new JSONArray();
				JSONArray unconfirmedAssetBalances = new JSONArray();
				while(accountAssets.hasNext())
				{
					Account.AccountAsset accountAsset = accountAssets.next();
					JSONObject assetBalance = new JSONObject();
					assetBalance.put("asset", Convert.toUnsignedLong(accountAsset.AssetId));
					assetBalance.put("balanceQNT", Convert.ToString(accountAsset.QuantityQNT));
					assetBalances.add(assetBalance);
					JSONObject unconfirmedAssetBalance = new JSONObject();
					unconfirmedAssetBalance.put("asset", Convert.toUnsignedLong(accountAsset.AssetId));
					unconfirmedAssetBalance.put("unconfirmedBalanceQNT", Convert.ToString(accountAsset.UnconfirmedQuantityQNT));
					unconfirmedAssetBalances.add(unconfirmedAssetBalance);
				}
				if(assetBalances.size() > 0)
				{
					response.put("assetBalances", assetBalances);
				}
				if(unconfirmedAssetBalances.size() > 0)
				{
					response.put("unconfirmedAssetBalances", unconfirmedAssetBalances);
				}
			}
			return response;

		}

	}

}