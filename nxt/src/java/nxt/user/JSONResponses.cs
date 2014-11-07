namespace nxt.user
{

	using JSON = nxt.util.JSON;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	public sealed class JSONResponses
	{

		public static readonly JSONStreamAware INVALID_SECRET_PHRASE;
		static JSONResponses()
		{
			JSONObject response = new JSONObject();
			response.put("response", "showMessage");
			response.put("message", "Invalid secret phrase!");
			INVALID_SECRET_PHRASE = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("response", "lockAccount");
			LOCK_ACCOUNT = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("response", "showMessage");
			response.put("message", "This operation is allowed to local host users only!");
			LOCAL_USERS_ONLY = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("response", "notifyOfAcceptedTransaction");
			NOTIFY_OF_ACCEPTED_TRANSACTION = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("response", "denyAccess");
			DENY_ACCESS = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("response", "showMessage");
			response.put("message", "Incorrect request!");
			INCORRECT_REQUEST = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("response", "showMessage");
			response.put("message", "This request is only accepted using POST!");
			POST_REQUIRED = JSON.prepare(response);
		}

		public static readonly JSONStreamAware LOCK_ACCOUNT;

		public static readonly JSONStreamAware LOCAL_USERS_ONLY;

		public static readonly JSONStreamAware NOTIFY_OF_ACCEPTED_TRANSACTION;

		public static readonly JSONStreamAware DENY_ACCESS;

		public static readonly JSONStreamAware INCORRECT_REQUEST;

		public static readonly JSONStreamAware POST_REQUIRED;

		private JSONResponses() // never
		{
		}

	}

}