namespace nxt.http
{

	using Constants = nxt.Constants;
	using JSON = nxt.util.JSON;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;


	public sealed class JSONResponses
	{

		public static readonly JSONStreamAware INCORRECT_ALIAS = incorrect("alias");
		public static readonly JSONStreamAware INCORRECT_ALIAS_OWNER = incorrect("alias","(invalid alias owner)");
		public static readonly JSONStreamAware INCORRECT_ALIAS_LENGTH = incorrect("alias", "(length must be in [1.." + Constants.MAX_ALIAS_LENGTH + "] range)");
		public static readonly JSONStreamAware INCORRECT_ALIAS_NAME = incorrect("alias", "(must contain only digits and latin letters)");
		public static readonly JSONStreamAware INCORRECT_ALIAS_NOTFORSALE = incorrect("alias", "(alias is not for sale at the moment)");
		public static readonly JSONStreamAware INCORRECT_URI_LENGTH = incorrect("uri", "(length must be not longer than " + Constants.MAX_ALIAS_URI_LENGTH + " characters)");
		public static readonly JSONStreamAware MISSING_SECRET_PHRASE = missing("secretPhrase");
		public static readonly JSONStreamAware INCORRECT_PUBLIC_KEY = incorrect("publicKey");
		public static readonly JSONStreamAware MISSING_ALIAS_NAME = missing("aliasName");
		public static readonly JSONStreamAware MISSING_ALIAS_OR_ALIAS_NAME = missing("alias", "aliasName");
		public static readonly JSONStreamAware MISSING_FEE = missing("feeNQT");
		public static readonly JSONStreamAware MISSING_DEADLINE = missing("deadline");
		public static readonly JSONStreamAware INCORRECT_DEADLINE = incorrect("deadline");
		public static readonly JSONStreamAware INCORRECT_FEE = incorrect("fee");
		public static readonly JSONStreamAware MISSING_TRANSACTION_BYTES_OR_JSON = missing("transactionBytes", "transactionJSON");
		public static readonly JSONStreamAware INCORRECT_TRANSACTION_BYTES_OR_JSON = incorrect("transactionBytes or transactionJSON");
		public static readonly JSONStreamAware MISSING_ORDER = missing("order");
		public static readonly JSONStreamAware INCORRECT_ORDER = incorrect("order");
		public static readonly JSONStreamAware UNKNOWN_ORDER = unknown("order");
		public static readonly JSONStreamAware MISSING_HALLMARK = missing("hallmark");
		public static readonly JSONStreamAware INCORRECT_HALLMARK = incorrect("hallmark");
		public static readonly JSONStreamAware MISSING_WEBSITE = missing("website");
		public static readonly JSONStreamAware INCORRECT_WEBSITE = incorrect("website");
		public static readonly JSONStreamAware MISSING_TOKEN = missing("token");
		public static readonly JSONStreamAware INCORRECT_TOKEN = incorrect("token");
		public static readonly JSONStreamAware MISSING_ACCOUNT = missing("account");
		public static readonly JSONStreamAware INCORRECT_ACCOUNT = incorrect("account");
		public static readonly JSONStreamAware MISSING_TIMESTAMP = missing("timestamp");
		public static readonly JSONStreamAware INCORRECT_TIMESTAMP = incorrect("timestamp");
		public static readonly JSONStreamAware UNKNOWN_ACCOUNT = unknown("account");
		public static readonly JSONStreamAware UNKNOWN_ALIAS = unknown("alias");
		public static readonly JSONStreamAware MISSING_ASSET = missing("asset");
		public static readonly JSONStreamAware UNKNOWN_ASSET = unknown("asset");
		public static readonly JSONStreamAware INCORRECT_ASSET = incorrect("asset");
		public static readonly JSONStreamAware MISSING_ASSET_NAME = missing("assetName");
		public static readonly JSONStreamAware MISSING_BLOCK = missing("block");
		public static readonly JSONStreamAware UNKNOWN_BLOCK = unknown("block");
		public static readonly JSONStreamAware INCORRECT_BLOCK = incorrect("block");
		public static readonly JSONStreamAware MISSING_NUMBER_OF_CONFIRMATIONS = missing("numberOfConfirmations");
		public static readonly JSONStreamAware INCORRECT_NUMBER_OF_CONFIRMATIONS = incorrect("numberOfConfirmations");
		public static readonly JSONStreamAware MISSING_PEER = missing("peer");
		public static readonly JSONStreamAware UNKNOWN_PEER = unknown("peer");
		public static readonly JSONStreamAware MISSING_TRANSACTION = missing("transaction");
		public static readonly JSONStreamAware UNKNOWN_TRANSACTION = unknown("transaction");
		public static readonly JSONStreamAware INCORRECT_TRANSACTION = incorrect("transaction");
		public static readonly JSONStreamAware INCORRECT_ASSET_DESCRIPTION = incorrect("description", "(length must not exceed " + Constants.MAX_ASSET_DESCRIPTION_LENGTH + " characters)");
		public static readonly JSONStreamAware INCORRECT_ASSET_NAME = incorrect("name", "(must contain only digits and latin letters)");
		public static readonly JSONStreamAware INCORRECT_ASSET_NAME_LENGTH = incorrect("name", "(length must be in [" + Constants.MIN_ASSET_NAME_LENGTH + ".." + Constants.MAX_ASSET_NAME_LENGTH + "] range)");
		public static readonly JSONStreamAware INCORRECT_ASSET_TRANSFER_COMMENT = incorrect("comment", "(length must not exceed " + Constants.MAX_ASSET_TRANSFER_COMMENT_LENGTH + " characters)");
		public static readonly JSONStreamAware MISSING_NAME = missing("name");
		public static readonly JSONStreamAware MISSING_QUANTITY = missing("quantityQNT");
		public static readonly JSONStreamAware INCORRECT_QUANTITY = incorrect("quantity");
		public static readonly JSONStreamAware INCORRECT_ASSET_QUANTITY = incorrect("quantity", "(must be in [1.." + Constants.MAX_ASSET_QUANTITY_QNT + "] range)");
		public static readonly JSONStreamAware INCORRECT_DECIMALS = incorrect("decimals");
		public static readonly JSONStreamAware MISSING_HOST = missing("host");
		public static readonly JSONStreamAware MISSING_DATE = missing("date");
		public static readonly JSONStreamAware MISSING_WEIGHT = missing("weight");
		public static readonly JSONStreamAware INCORRECT_HOST = incorrect("host", "(the length exceeds 100 chars limit)");
		public static readonly JSONStreamAware INCORRECT_WEIGHT = incorrect("weight");
		public static readonly JSONStreamAware INCORRECT_DATE = incorrect("date");
		public static readonly JSONStreamAware MISSING_PRICE = missing("priceNQT");
		public static readonly JSONStreamAware INCORRECT_PRICE = incorrect("price");
		public static readonly JSONStreamAware INCORRECT_REFERENCED_TRANSACTION = incorrect("referencedTransactionFullHash");
		public static readonly JSONStreamAware MISSING_MESSAGE = missing("message");
		public static readonly JSONStreamAware MISSING_RECIPIENT = missing("recipient");
		public static readonly JSONStreamAware INCORRECT_RECIPIENT = incorrect("recipient");
		public static readonly JSONStreamAware INCORRECT_ARBITRARY_MESSAGE = incorrect("message");
		public static readonly JSONStreamAware MISSING_AMOUNT = missing("amountNQT");
		public static readonly JSONStreamAware INCORRECT_AMOUNT = incorrect("amount");
		public static readonly JSONStreamAware MISSING_DESCRIPTION = missing("description");
		public static readonly JSONStreamAware MISSING_MINNUMBEROFOPTIONS = missing("minNumberOfOptions");
		public static readonly JSONStreamAware MISSING_MAXNUMBEROFOPTIONS = missing("maxNumberOfOptions");
		public static readonly JSONStreamAware MISSING_OPTIONSAREBINARY = missing("optionsAreBinary");
		public static readonly JSONStreamAware MISSING_POLL = missing("poll");
		public static readonly JSONStreamAware INCORRECT_POLL_NAME_LENGTH = incorrect("name", "(length must be not longer than " + Constants.MAX_POLL_NAME_LENGTH + " characters)");
		public static readonly JSONStreamAware INCORRECT_POLL_DESCRIPTION_LENGTH = incorrect("description", "(length must be not longer than " + Constants.MAX_POLL_DESCRIPTION_LENGTH + " characters)");
		public static readonly JSONStreamAware INCORRECT_POLL_OPTION_LENGTH = incorrect("option", "(length must be not longer than " + Constants.MAX_POLL_OPTION_LENGTH + " characters)");
		public static readonly JSONStreamAware INCORRECT_MINNUMBEROFOPTIONS = incorrect("minNumberOfOptions");
		public static readonly JSONStreamAware INCORRECT_MAXNUMBEROFOPTIONS = incorrect("maxNumberOfOptions");
		public static readonly JSONStreamAware INCORRECT_OPTIONSAREBINARY = incorrect("optionsAreBinary");
		public static readonly JSONStreamAware INCORRECT_POLL = incorrect("poll");
		public static readonly JSONStreamAware INCORRECT_VOTE = incorrect("vote");
		public static readonly JSONStreamAware UNKNOWN_POLL = unknown("poll");
		public static readonly JSONStreamAware INCORRECT_ACCOUNT_NAME_LENGTH = incorrect("name", "(length must be less than " + Constants.MAX_ACCOUNT_NAME_LENGTH + " characters)");
		public static readonly JSONStreamAware INCORRECT_ACCOUNT_DESCRIPTION_LENGTH = incorrect("description", "(length must be less than " + Constants.MAX_ACCOUNT_DESCRIPTION_LENGTH + " characters)");
		public static readonly JSONStreamAware MISSING_PERIOD = missing("period");
		public static readonly JSONStreamAware INCORRECT_PERIOD = incorrect("period", "(period must be at least 1440 blocks)");
		public static readonly JSONStreamAware INCORRECT_UNSIGNED_BYTES = incorrect("unsignedTransactionBytes");
		public static readonly JSONStreamAware MISSING_UNSIGNED_BYTES = missing("unsignedTransactionBytes");
		public static readonly JSONStreamAware MISSING_SIGNATURE_HASH = missing("signatureHash");
		public static readonly JSONStreamAware INCORRECT_DGS_LISTING_NAME = incorrect("name", "(length must be not longer than " + Constants.MAX_DGS_LISTING_NAME_LENGTH + " characters)");
		public static readonly JSONStreamAware INCORRECT_DGS_LISTING_DESCRIPTION = incorrect("description", "(length must be not longer than " + Constants.MAX_DGS_LISTING_DESCRIPTION_LENGTH + " characters)");
		public static readonly JSONStreamAware INCORRECT_DGS_LISTING_TAGS = incorrect("tags", "(length must be not longer than " + Constants.MAX_DGS_LISTING_TAGS_LENGTH + " characters)");
		public static readonly JSONStreamAware MISSING_GOODS = missing("goods");
		public static readonly JSONStreamAware INCORRECT_GOODS = incorrect("goods");
		public static readonly JSONStreamAware UNKNOWN_GOODS = unknown("goods");
		public static readonly JSONStreamAware INCORRECT_DELTA_QUANTITY = incorrect("deltaQuantity");
		public static readonly JSONStreamAware MISSING_DELTA_QUANTITY = missing("deltaQuantity");
		public static readonly JSONStreamAware MISSING_DELIVERY_DEADLINE_TIMESTAMP = missing("deliveryDeadlineTimestamp");
		public static readonly JSONStreamAware INCORRECT_DELIVERY_DEADLINE_TIMESTAMP = incorrect("deliveryDeadlineTimestamp");
		public static readonly JSONStreamAware INCORRECT_PURCHASE_QUANTITY = incorrect("quantity", "(quantity exceeds available goods quantity)");
		public static readonly JSONStreamAware INCORRECT_PURCHASE_PRICE = incorrect("priceNQT", "(purchase price doesn't match goods price)");
		public static readonly JSONStreamAware INCORRECT_PURCHASE = incorrect("purchase");
		public static readonly JSONStreamAware MISSING_PURCHASE = missing("purchase");
		public static readonly JSONStreamAware INCORRECT_DGS_GOODS = incorrect("goodsToEncrypt");
		public static readonly JSONStreamAware INCORRECT_DGS_DISCOUNT = incorrect("discountNQT");
		public static readonly JSONStreamAware INCORRECT_DGS_REFUND = incorrect("refundNQT");
		public static readonly JSONStreamAware MISSING_SELLER = missing("seller");
		public static readonly JSONStreamAware INCORRECT_ENCRYPTED_MESSAGE = incorrect("encryptedMessageData");
		public static readonly JSONStreamAware INCORRECT_DGS_ENCRYPTED_GOODS = incorrect("goodsData");
		public static readonly JSONStreamAware MISSING_SECRET_PHRASE_OR_PUBLIC_KEY = missing("secretPhrase", "publicKey");
		public static readonly JSONStreamAware INCORRECT_HEIGHT = incorrect("height");
		public static readonly JSONStreamAware MISSING_HEIGHT = missing("height");
		public static readonly JSONStreamAware INCORRECT_PLAIN_MESSAGE = incorrect("messageToEncrypt");

		public static readonly JSONStreamAware NOT_ENOUGH_FUNDS;
		static JSONResponses()
		{
			JSONObject response = new JSONObject();
			response.put("errorCode", 6);
			response.put("errorDescription", "Not enough funds");
			NOT_ENOUGH_FUNDS = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 6);
			response.put("errorDescription", "Not enough assets");
			NOT_ENOUGH_ASSETS = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 7);
			response.put("errorDescription", "Not allowed");
			ERROR_NOT_ALLOWED = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 1);
			response.put("errorDescription", "Incorrect request");
			ERROR_INCORRECT_REQUEST = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 5);
			response.put("errorDescription", "Account is not forging");
			NOT_FORGING = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 1);
			response.put("errorDescription", "This request is only accepted using POST!");
			POST_REQUIRED = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 9);
			response.put("errorDescription", "Feature not available");
			FEATURE_NOT_AVAILABLE = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 8);
			response.put("errorDescription", "Decryption failed");
			DECRYPTION_FAILED = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 8);
			response.put("errorDescription", "Purchase already delivered");
			ALREADY_DELIVERED = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 8);
			response.put("errorDescription", "Refund already sent");
			DUPLICATE_REFUND = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 8);
			response.put("errorDescription", "Goods have not been delivered yet");
			GOODS_NOT_DELIVERED = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 8);
			response.put("errorDescription", "No attached message found");
			NO_MESSAGE = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("errorCode", 8);
			response.put("errorDescription", "Requested height not available");
			HEIGHT_NOT_AVAILABLE = JSON.prepare(response);
		}

		public static readonly JSONStreamAware NOT_ENOUGH_ASSETS;

		public static readonly JSONStreamAware ERROR_NOT_ALLOWED;

		public static readonly JSONStreamAware ERROR_INCORRECT_REQUEST;

		public static readonly JSONStreamAware NOT_FORGING;

		public static readonly JSONStreamAware POST_REQUIRED;

		public static readonly JSONStreamAware FEATURE_NOT_AVAILABLE;

		public static readonly JSONStreamAware DECRYPTION_FAILED;

		public static readonly JSONStreamAware ALREADY_DELIVERED;

		public static readonly JSONStreamAware DUPLICATE_REFUND;

		public static readonly JSONStreamAware GOODS_NOT_DELIVERED;

		public static readonly JSONStreamAware NO_MESSAGE;

		public static readonly JSONStreamAware HEIGHT_NOT_AVAILABLE;

		private static JSONStreamAware missing(params string[] paramNames)
		{
			JSONObject response = new JSONObject();
			response.put("errorCode", 3);
			if(paramNames.length == 1)
			{
				response.put("errorDescription", "\"" + paramNames[0] + "\"" + " not specified");
			}
			else
			{
				response.put("errorDescription", "At least one of " + Arrays.ToString(paramNames) + " must be specified");
			}
			return JSON.prepare(response);
		}

		private static JSONStreamAware incorrect(string paramName)
		{
			return incorrect(paramName, null);
		}

		private static JSONStreamAware incorrect(string paramName, string details)
		{
			JSONObject response = new JSONObject();
			response.put("errorCode", 4);
			response.put("errorDescription", "Incorrect \"" + paramName + (details != null ? "\" " + details : "\""));
			return JSON.prepare(response);
		}

		private static JSONStreamAware unknown(string objectName)
		{
			JSONObject response = new JSONObject();
			response.put("errorCode", 5);
			response.put("errorDescription", "Unknown " + objectName);
			return JSON.prepare(response);
		}

		private JSONResponses() // never
		{
		}

	}

}