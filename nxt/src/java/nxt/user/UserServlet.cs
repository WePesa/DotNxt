using System;
using System.Collections.Generic;

namespace nxt.user
{

	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using ServletException = javax.servlet.ServletException;
	using HttpServlet = javax.servlet.http.HttpServlet;
	using HttpServletRequest = javax.servlet.http.HttpServletRequest;
	using HttpServletResponse = javax.servlet.http.HttpServletResponse;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.user.JSONResponses.DENY_ACCESS;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.user.JSONResponses.INCORRECT_REQUEST;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.user.JSONResponses.POST_REQUIRED;

	public sealed class UserServlet : HttpServlet
	{

		internal abstract class UserRequestHandler
		{
//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: abstract JSONStreamAware processRequest(HttpServletRequest request, User user) throws NxtException, IOException;
			internal abstract JSONStreamAware processRequest(HttpServletRequest request, User user);
			internal virtual bool requirePost()
			{
				return false;
			}
		}

		private const bool enforcePost = Nxt.getBooleanProperty("nxt.uiServerEnforcePOST");

		private static readonly IDictionary<string, UserRequestHandler> userRequestHandlers;

		static UserServlet()
		{
			IDictionary<string, UserRequestHandler> map = new Dictionary<>();
			map.Add("generateAuthorizationToken", GenerateAuthorizationToken.instance);
			map.Add("getInitialData", GetInitialData.instance);
			map.Add("getNewData", GetNewData.instance);
			map.Add("lockAccount", LockAccount.instance);
			map.Add("removeActivePeer", RemoveActivePeer.instance);
			map.Add("removeBlacklistedPeer", RemoveBlacklistedPeer.instance);
			map.Add("removeKnownPeer", RemoveKnownPeer.instance);
			map.Add("sendMoney", SendMoney.instance);
			map.Add("unlockAccount", UnlockAccount.instance);
			userRequestHandlers = Collections.unmodifiableMap(map);
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

			User user = null;

			try
			{

				string userPasscode = req.getParameter("user");
				if(userPasscode == null)
				{
					return;
				}
				user = Users.getUser(userPasscode);

				if(Users.allowedUserHosts != null && ! Users.allowedUserHosts.contains(req.RemoteHost))
				{
					user.enqueue(DENY_ACCESS);
					return;
				}

				string requestType = req.getParameter("requestType");
				if(requestType == null)
				{
					user.enqueue(INCORRECT_REQUEST);
					return;
				}

				UserRequestHandler userRequestHandler = userRequestHandlers[requestType];
				if(userRequestHandler == null)
				{
					user.enqueue(INCORRECT_REQUEST);
					return;
				}

				if(enforcePost && userRequestHandler.requirePost() && ! "POST".Equals(req.Method))
				{
					user.enqueue(POST_REQUIRED);
					return;
				}

				JSONStreamAware response = userRequestHandler.processRequest(req, user);
				if(response != null)
				{
					user.enqueue(response);
				}

			}
			catch(Exception|NxtException e)
			{

				Logger.logMessage("Error processing GET request", e);
				if(user != null)
				{
					JSONObject response = new JSONObject();
					response.put("response", "showMessage");
					response.put("message", e.ToString());
					user.enqueue(response);
				}

			}
			finally
			{

				if(user != null)
				{
					user.processPendingResponses(req, resp);
				}

			}

		}

	}

}