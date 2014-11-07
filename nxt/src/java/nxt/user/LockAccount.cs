namespace nxt.user
{

	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.user.JSONResponses.LOCK_ACCOUNT;

	public sealed class LockAccount : UserServlet.UserRequestHandler
	{

		internal static readonly LockAccount instance = new LockAccount();

		private LockAccount()
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req, User user) throws IOException
		internal override JSONStreamAware processRequest(HttpServletRequest req, User user)
		{

			user.lockAccount();

			return LOCK_ACCOUNT;
		}
	}

}