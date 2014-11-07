using System;
using System.Collections.Generic;

namespace nxt.peer
{

	using CountingInputStream = nxt.util.CountingInputStream;
	using CountingOutputStream = nxt.util.CountingOutputStream;
	using JSON = nxt.util.JSON;
	using Logger = nxt.util.Logger;
	using Response = org.eclipse.jetty.server.Response;
	using CompressedResponseWrapper = org.eclipse.jetty.servlets.gzip.CompressedResponseWrapper;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;
	using JSONValue = org.json.simple.JSONValue;

	using ServletConfig = javax.servlet.ServletConfig;
	using ServletException = javax.servlet.ServletException;
	using HttpServlet = javax.servlet.http.HttpServlet;
	using HttpServletRequest = javax.servlet.http.HttpServletRequest;
	using HttpServletResponse = javax.servlet.http.HttpServletResponse;

	public sealed class PeerServlet : HttpServlet
	{

		internal abstract class PeerRequestHandler
		{
			internal abstract JSONStreamAware processRequest(JSONObject request, Peer peer);
		}

		private static readonly IDictionary<string, PeerRequestHandler> peerRequestHandlers;

		static PeerServlet()
		{
			IDictionary<string, PeerRequestHandler> map = new Dictionary<>();
			map.Add("addPeers", AddPeers.instance);
			map.Add("getCumulativeDifficulty", GetCumulativeDifficulty.instance);
			map.Add("getInfo", GetInfo.instance);
			map.Add("getMilestoneBlockIds", GetMilestoneBlockIds.instance);
			map.Add("getNextBlockIds", GetNextBlockIds.instance);
			map.Add("getNextBlocks", GetNextBlocks.instance);
			map.Add("getPeers", GetPeers.instance);
			map.Add("getUnconfirmedTransactions", GetUnconfirmedTransactions.instance);
			map.Add("processBlock", ProcessBlock.instance);
			map.Add("processTransactions", ProcessTransactions.instance);
			peerRequestHandlers = Collections.unmodifiableMap(map);
			JSONObject response = new JSONObject();
			response.put("error", "Unsupported request type!");
			UNSUPPORTED_REQUEST_TYPE = JSON.prepare(response);
			JSONObject response = new JSONObject();
			response.put("error", "Unsupported protocol!");
			UNSUPPORTED_PROTOCOL = JSON.prepare(response);
		}

		private static readonly JSONStreamAware UNSUPPORTED_REQUEST_TYPE;

		private static readonly JSONStreamAware UNSUPPORTED_PROTOCOL;

		private bool isGzipEnabled;

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void init(ServletConfig config) throws ServletException
		public override void init(ServletConfig config)
		{
			base.init(config);
			isGzipEnabled = Convert.ToBoolean(config.getInitParameter("isGzipEnabled"));
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException
		protected internal override void doPost(HttpServletRequest req, HttpServletResponse resp)
		{

			PeerImpl peer = null;
			JSONStreamAware response;

			try
			{
				peer = Peers.addPeer(req.RemoteAddr, null);
				if(peer == null)
				{
					return;
				}
				if(peer.Blacklisted)
				{
					return;
				}

				JSONObject request;
				CountingInputStream cis = new CountingInputStream(req.InputStream);
				using (Reader reader = new InputStreamReader(cis, "UTF-8"))
				{
					request = (JSONObject) JSONValue.parse(reader);
				}
				if(request == null)
				{
					return;
				}

				if(peer.State == Peer.State.DISCONNECTED)
				{
					peer.State = Peer.State.CONNECTED;
					if(peer.AnnouncedAddress != null)
					{
						Peers.updateAddress(peer);
					}
				}
				peer.updateDownloadedVolume(cis.Count);
				if(! peer.analyzeHallmark(peer.PeerAddress, (string)request.get("hallmark")))
				{
					peer.blacklist();
					return;
				}

				if(request.get("protocol") != null && (int)((Number)request.get("protocol")) == 1)
				{
					PeerRequestHandler peerRequestHandler = peerRequestHandlers[request.get("requestType")];
					if(peerRequestHandler != null)
					{
						response = peerRequestHandler.processRequest(request, peer);
					}
					else
					{
						response = UNSUPPORTED_REQUEST_TYPE;
					}
				}
				else
				{
					Logger.logDebugMessage("Unsupported protocol " + request.get("protocol"));
					response = UNSUPPORTED_PROTOCOL;
				}

			}
			catch(Exception e)
			{
				Logger.logDebugMessage("Error processing POST request", e);
				JSONObject json = new JSONObject();
				json.put("error", e.ToString());
				response = json;
			}

			resp.ContentType = "text/plain; charset=UTF-8";
			try
			{
				long byteCount;
				if(isGzipEnabled)
				{
					using (Writer writer = new OutputStreamWriter(resp.OutputStream, "UTF-8"))
					{
						response.writeJSONString(writer);
					}
					byteCount = ((Response)((CompressedResponseWrapper) resp).Response).ContentCount;
				}
				else
				{
					CountingOutputStream cos = new CountingOutputStream(resp.OutputStream);
					using (Writer writer = new OutputStreamWriter(cos, "UTF-8"))
					{
						response.writeJSONString(writer);
					}
					byteCount = cos.Count;
				}
				if(peer != null)
				{
					peer.updateUploadedVolume(byteCount);
				}
			}
			catch(Exception e)
			{
				if(peer != null)
				{
					peer.blacklist(e);
				}
				throw e;
			}
		}

	}

}