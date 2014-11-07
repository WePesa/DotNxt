using System;
using System.Collections.Generic;

namespace nxt.http
{

	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Db = nxt.db.Db;
	using JSON = nxt.util.JSON;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using ServletException = javax.servlet.ServletException;
	using HttpServlet = javax.servlet.http.HttpServlet;
	using HttpServletRequest = javax.servlet.http.HttpServletRequest;
	using HttpServletResponse = javax.servlet.http.HttpServletResponse;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.ERROR_INCORRECT_REQUEST;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.ERROR_NOT_ALLOWED;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.POST_REQUIRED;

	public sealed class APIServlet : HttpServlet
	{

		internal abstract class APIRequestHandler
		{

			private readonly IList<string> parameters;
			private readonly Set<APITag> apiTags;

			internal APIRequestHandler(APITag[] apiTags, params string[] parameters)
			{
				this.parameters = Collections.unmodifiableList(parameters);
				this.apiTags = Collections.unmodifiableSet(new HashSet<>(apiTags));
			}

			internal IList<string> Parameters
			{
				get
				{
					return parameters;
				}
			}

			internal Set<APITag> APITags
			{
				get
				{
					return apiTags;
				}
			}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract JSONStreamAware processRequest(HttpServletRequest request) throws NxtException;
			internal abstract JSONStreamAware processRequest(HttpServletRequest request);

			internal virtual bool requirePost()
			{
				return false;
			}

			internal virtual bool startDbTransaction()
			{
				return false;
			}

		}

		private const bool enforcePost = Nxt.getBooleanProperty("nxt.apiServerEnforcePOST");

		internal static readonly IDictionary<string, APIRequestHandler> apiRequestHandlers;

		static APIServlet()
		{

			IDictionary<string, APIRequestHandler> map = new Dictionary<>();

			map.Add("broadcastTransaction", BroadcastTransaction.instance);
			map.Add("calculateFullHash", CalculateFullHash.instance);
			map.Add("cancelAskOrder", CancelAskOrder.instance);
			map.Add("cancelBidOrder", CancelBidOrder.instance);
		//map.put("castVote", CastVote.instance);
		//map.put("createPoll", CreatePoll.instance);
			map.Add("decryptFrom", DecryptFrom.instance);
			map.Add("dgsListing", DGSListing.instance);
			map.Add("dgsDelisting", DGSDelisting.instance);
			map.Add("dgsDelivery", DGSDelivery.instance);
			map.Add("dgsFeedback", DGSFeedback.instance);
			map.Add("dgsPriceChange", DGSPriceChange.instance);
			map.Add("dgsPurchase", DGSPurchase.instance);
			map.Add("dgsQuantityChange", DGSQuantityChange.instance);
			map.Add("dgsRefund", DGSRefund.instance);
			map.Add("decodeHallmark", DecodeHallmark.instance);
			map.Add("decodeToken", DecodeToken.instance);
			map.Add("encryptTo", EncryptTo.instance);
			map.Add("generateToken", GenerateToken.instance);
			map.Add("getAccount", GetAccount.instance);
			map.Add("getAccountBlockIds", GetAccountBlockIds.instance);
			map.Add("getAccountBlocks", GetAccountBlocks.instance);
			map.Add("getAccountId", GetAccountId.instance);
			map.Add("getAccountPublicKey", GetAccountPublicKey.instance);
			map.Add("getAccountTransactionIds", GetAccountTransactionIds.instance);
			map.Add("getAccountTransactions", GetAccountTransactions.instance);
			map.Add("getAccountLessors", GetAccountLessors.instance);
			map.Add("sellAlias", SellAlias.instance);
			map.Add("buyAlias", BuyAlias.instance);
			map.Add("getAlias", GetAlias.instance);
			map.Add("getAliases", GetAliases.instance);
			map.Add("getAllAssets", GetAllAssets.instance);
			map.Add("getAsset", GetAsset.instance);
			map.Add("getAssets", GetAssets.instance);
			map.Add("getAssetIds", GetAssetIds.instance);
			map.Add("getAssetsByIssuer", GetAssetsByIssuer.instance);
			map.Add("getAssetAccounts", GetAssetAccounts.instance);
			map.Add("getBalance", GetBalance.instance);
			map.Add("getBlock", GetBlock.instance);
			map.Add("getBlockId", GetBlockId.instance);
			map.Add("getBlocks", GetBlocks.instance);
			map.Add("getBlockchainStatus", GetBlockchainStatus.instance);
			map.Add("getConstants", GetConstants.instance);
			map.Add("getDGSGoods", GetDGSGoods.instance);
			map.Add("getDGSGood", GetDGSGood.instance);
			map.Add("getDGSPurchases", GetDGSPurchases.instance);
			map.Add("getDGSPurchase", GetDGSPurchase.instance);
			map.Add("getDGSPendingPurchases", GetDGSPendingPurchases.instance);
			map.Add("getGuaranteedBalance", GetGuaranteedBalance.instance);
			map.Add("getECBlock", GetECBlock.instance);
			map.Add("getMyInfo", GetMyInfo.instance);
		//map.put("getNextBlockGenerators", GetNextBlockGenerators.instance);
			map.Add("getPeer", GetPeer.instance);
			map.Add("getPeers", GetPeers.instance);
		//map.put("getPoll", GetPoll.instance);
		//map.put("getPollIds", GetPollIds.instance);
			map.Add("getState", GetState.instance);
			map.Add("getTime", GetTime.instance);
			map.Add("getTrades", GetTrades.instance);
			map.Add("getAllTrades", GetAllTrades.instance);
			map.Add("getAssetTransfers", GetAssetTransfers.instance);
			map.Add("getTransaction", GetTransaction.instance);
			map.Add("getTransactionBytes", GetTransactionBytes.instance);
			map.Add("getUnconfirmedTransactionIds", GetUnconfirmedTransactionIds.instance);
			map.Add("getUnconfirmedTransactions", GetUnconfirmedTransactions.instance);
			map.Add("getAccountCurrentAskOrderIds", GetAccountCurrentAskOrderIds.instance);
			map.Add("getAccountCurrentBidOrderIds", GetAccountCurrentBidOrderIds.instance);
			map.Add("getAccountCurrentAskOrders", GetAccountCurrentAskOrders.instance);
			map.Add("getAccountCurrentBidOrders", GetAccountCurrentBidOrders.instance);
			map.Add("getAllOpenAskOrders", GetAllOpenAskOrders.instance);
			map.Add("getAllOpenBidOrders", GetAllOpenBidOrders.instance);
			map.Add("getAskOrder", GetAskOrder.instance);
			map.Add("getAskOrderIds", GetAskOrderIds.instance);
			map.Add("getAskOrders", GetAskOrders.instance);
			map.Add("getBidOrder", GetBidOrder.instance);
			map.Add("getBidOrderIds", GetBidOrderIds.instance);
			map.Add("getBidOrders", GetBidOrders.instance);
			map.Add("issueAsset", IssueAsset.instance);
			map.Add("leaseBalance", LeaseBalance.instance);
			map.Add("longConvert", LongConvert.instance);
			map.Add("markHost", MarkHost.instance);
			map.Add("parseTransaction", ParseTransaction.instance);
			map.Add("placeAskOrder", PlaceAskOrder.instance);
			map.Add("placeBidOrder", PlaceBidOrder.instance);
			map.Add("rsConvert", RSConvert.instance);
			map.Add("readMessage", ReadMessage.instance);
			map.Add("sendMessage", SendMessage.instance);
			map.Add("sendMoney", SendMoney.instance);
			map.Add("setAccountInfo", SetAccountInfo.instance);
			map.Add("setAlias", SetAlias.instance);
			map.Add("signTransaction", SignTransaction.instance);
			map.Add("startForging", StartForging.instance);
			map.Add("stopForging", StopForging.instance);
			map.Add("getForging", GetForging.instance);
			map.Add("transferAsset", TransferAsset.instance);

			if(API.enableDebugAPI)
			{
				map.Add("clearUnconfirmedTransactions", ClearUnconfirmedTransactions.instance);
				map.Add("fullReset", FullReset.instance);
				map.Add("popOff", PopOff.instance);
				map.Add("scan", Scan.instance);
			}

			apiRequestHandlers = Collections.unmodifiableMap(map);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException
		protected internal override void doGet(HttpServletRequest req, HttpServletResponse resp)
		{
			process(req, resp);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException
		protected internal override void doPost(HttpServletRequest req, HttpServletResponse resp)
		{
			process(req, resp);
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void process(HttpServletRequest req, HttpServletResponse resp) throws IOException
		private void process(HttpServletRequest req, HttpServletResponse resp)
		{

			resp.setHeader("Cache-Control", "no-cache, no-store, must-revalidate, private");
			resp.setHeader("Pragma", "no-cache");
			resp.setDateHeader("Expires", 0);

			JSONStreamAware response = JSON.emptyJSON;

			try
			{

				long startTime = System.currentTimeMillis();

				if(API.allowedBotHosts != null && ! API.allowedBotHosts.contains(req.RemoteHost))
				{
					response = ERROR_NOT_ALLOWED;
					return;
				}

				string requestType = req.getParameter("requestType");
				if(requestType == null)
				{
					response = ERROR_INCORRECT_REQUEST;
					return;
				}

				APIRequestHandler apiRequestHandler = apiRequestHandlers[requestType];
				if(apiRequestHandler == null)
				{
					response = ERROR_INCORRECT_REQUEST;
					return;
				}

				if(enforcePost && apiRequestHandler.requirePost() && ! "POST".Equals(req.Method))
				{
					response = POST_REQUIRED;
					return;
				}

				try
				{
					if(apiRequestHandler.startDbTransaction())
					{
						Db.beginTransaction();
					}
					response = apiRequestHandler.processRequest(req);
				}
				catch(ParameterException e)
				{
					response = e.ErrorResponse;
				}
				catch(NxtException |Exception e)
				{
					Logger.logDebugMessage("Error processing API request", e);
					response = ERROR_INCORRECT_REQUEST;
				}
				finally
				{
					if(apiRequestHandler.startDbTransaction())
					{
						Db.endTransaction();
					}
				}

				if(response is JSONObject)
				{
					((JSONObject)response).put("requestProcessingTime", System.currentTimeMillis() - startTime);
				}

			}
			finally
			{
				resp.ContentType = "text/plain; charset=UTF-8";
				using (Writer writer = resp.Writer)
				{
					response.writeJSONString(writer);
				}
			}

		}

	}

}