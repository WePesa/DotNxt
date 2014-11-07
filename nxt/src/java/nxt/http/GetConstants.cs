namespace nxt.http
{

	using Constants = nxt.Constants;
	using Genesis = nxt.Genesis;
	using TransactionType = nxt.TransactionType;
	using Convert = nxt.util.Convert;
	using JSON = nxt.util.JSON;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

	public sealed class GetConstants : APIServlet.APIRequestHandler
	{

		internal static readonly GetConstants instance = new GetConstants();

		private static readonly JSONStreamAware CONSTANTS;

		static GetConstants()
		{

			JSONObject response = new JSONObject();
			response.put("genesisBlockId", Convert.toUnsignedLong(Genesis.GENESIS_BLOCK_ID));
			response.put("genesisAccountId", Convert.toUnsignedLong(Genesis.CREATOR_ID));
			response.put("maxBlockPayloadLength", Constants.MAX_PAYLOAD_LENGTH);
			response.put("maxArbitraryMessageLength", Constants.MAX_ARBITRARY_MESSAGE_LENGTH);

			JSONArray transactionTypes = new JSONArray();
			JSONObject transactionType = new JSONObject();
			transactionType.put("value", TransactionType.Payment.ORDINARY.Type);
			transactionType.put("description", "Payment");
			JSONArray subtypes = new JSONArray();
			JSONObject subtype = new JSONObject();
			subtype.put("value", TransactionType.Payment.ORDINARY.Subtype);
			subtype.put("description", "Ordinary payment");
			subtypes.add(subtype);
			transactionType.put("subtypes", subtypes);
			transactionTypes.add(transactionType);
			transactionType = new JSONObject();
			transactionType.put("value", TransactionType.Messaging.ARBITRARY_MESSAGE.Type);
			transactionType.put("description", "Messaging");
			subtypes = new JSONArray();
			subtype = new JSONObject();
			subtype.put("value", TransactionType.Messaging.ARBITRARY_MESSAGE.Subtype);
			subtype.put("description", "Arbitrary message");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.Messaging.ALIAS_ASSIGNMENT.Subtype);
			subtype.put("description", "Alias assignment");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.Messaging.ALIAS_SELL.Subtype);
			subtype.put("description", "Alias sell");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.Messaging.ALIAS_BUY.Subtype);
			subtype.put("description", "Alias buy");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.Messaging.POLL_CREATION.Subtype);
			subtype.put("description", "Poll creation");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.Messaging.VOTE_CASTING.Subtype);
			subtype.put("description", "Vote casting");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.Messaging.HUB_ANNOUNCEMENT.Subtype);
			subtype.put("description", "Hub terminal announcement");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.Messaging.ACCOUNT_INFO.Subtype);
			subtype.put("description", "Account info");
			subtypes.add(subtype);
			transactionType.put("subtypes", subtypes);
			transactionTypes.add(transactionType);
			transactionType = new JSONObject();
			transactionType.put("value", TransactionType.ColoredCoins.ASSET_ISSUANCE.Type);
			transactionType.put("description", "Colored coins");
			subtypes = new JSONArray();
			subtype = new JSONObject();
			subtype.put("value", TransactionType.ColoredCoins.ASSET_ISSUANCE.Subtype);
			subtype.put("description", "Asset issuance");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.ColoredCoins.ASSET_TRANSFER.Subtype);
			subtype.put("description", "Asset transfer");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.ColoredCoins.ASK_ORDER_PLACEMENT.Subtype);
			subtype.put("description", "Ask order placement");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.ColoredCoins.BID_ORDER_PLACEMENT.Subtype);
			subtype.put("description", "Bid order placement");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.ColoredCoins.ASK_ORDER_CANCELLATION.Subtype);
			subtype.put("description", "Ask order cancellation");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.ColoredCoins.BID_ORDER_CANCELLATION.Subtype);
			subtype.put("description", "Bid order cancellation");
			subtypes.add(subtype);
			transactionType.put("subtypes", subtypes);
			transactionTypes.add(transactionType);
			transactionType = new JSONObject();
			transactionType.put("value", TransactionType.DigitalGoods.LISTING.Type);
			transactionType.put("description", "Digital goods");
			subtypes = new JSONArray();
			subtype = new JSONObject();
			subtype.put("value", TransactionType.DigitalGoods.LISTING.Subtype);
			subtype.put("description", "Listing");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.DigitalGoods.DELISTING.Subtype);
			subtype.put("description", "Delisting");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.DigitalGoods.PRICE_CHANGE.Subtype);
			subtype.put("description", "Price change");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.DigitalGoods.QUANTITY_CHANGE.Subtype);
			subtype.put("description", "Quantity change");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.DigitalGoods.PURCHASE.Subtype);
			subtype.put("description", "Purchase");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.DigitalGoods.DELIVERY.Subtype);
			subtype.put("description", "Delivery");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.DigitalGoods.FEEDBACK.Subtype);
			subtype.put("description", "Feedback");
			subtypes.add(subtype);
			subtype = new JSONObject();
			subtype.put("value", TransactionType.DigitalGoods.REFUND.Subtype);
			subtype.put("description", "Refund");
			subtypes.add(subtype);
			transactionType.put("subtypes", subtypes);
			transactionTypes.add(transactionType);
			transactionType = new JSONObject();
			transactionType.put("value", TransactionType.AccountControl.EFFECTIVE_BALANCE_LEASING.Type);
			transactionType.put("description", "Account Control");
			subtypes = new JSONArray();
			subtype = new JSONObject();
			subtype.put("value", TransactionType.AccountControl.EFFECTIVE_BALANCE_LEASING.Subtype);
			subtype.put("description", "Effective balance leasing");
			subtypes.add(subtype);
			transactionType.put("subtypes", subtypes);
			transactionTypes.add(transactionType);
			response.put("transactionTypes", transactionTypes);

			JSONArray peerStates = new JSONArray();
			JSONObject peerState = new JSONObject();
			peerState.put("value", 0);
			peerState.put("description", "Non-connected");
			peerStates.add(peerState);
			peerState = new JSONObject();
			peerState.put("value", 1);
			peerState.put("description", "Connected");
			peerStates.add(peerState);
			peerState = new JSONObject();
			peerState.put("value", 2);
			peerState.put("description", "Disconnected");
			peerStates.add(peerState);
			response.put("peerStates", peerStates);

			CONSTANTS = JSON.prepare(response);

		}

		private GetConstants() : base(new APITag[] {APITag.INFO})
		{
		}

		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{
			return CONSTANTS;
		}

	}

}