using System;

namespace nxt.user
{

	using Peer = nxt.peer.Peer;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.user.JSONResponses.LOCAL_USERS_ONLY;

	public sealed class RemoveBlacklistedPeer : UserServlet.UserRequestHandler
	{

		internal static readonly RemoveBlacklistedPeer instance = new RemoveBlacklistedPeer();

		private RemoveBlacklistedPeer()
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req, User user) throws IOException
		internal override JSONStreamAware processRequest(HttpServletRequest req, User user)
		{
			if(Users.allowedUserHosts == null && ! InetAddress.getByName(req.RemoteAddr).LoopbackAddress)
			{
				return LOCAL_USERS_ONLY;
			}
			else
			{
				int index = Convert.ToInt32(req.getParameter("peer"));
				Peer peer = Users.getPeer(index);
				if(peer != null && peer.Blacklisted)
				{
					peer.unBlacklist();
				}
			}
			return null;
		}
	}

}