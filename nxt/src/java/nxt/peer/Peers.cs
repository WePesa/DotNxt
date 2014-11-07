using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace nxt.peer
{

	using Account = nxt.Account;
	using Block = nxt.Block;
	using Constants = nxt.Constants;
	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using Db = nxt.db.Db;
	using JSON = nxt.util.JSON;
	using Listener = nxt.util.Listener;
	using Listeners = nxt.util.Listeners;
	using Logger = nxt.util.Logger;
	using ThreadPool = nxt.util.ThreadPool;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;
	using FilterHolder = org.eclipse.jetty.servlet.FilterHolder;
	using FilterMapping = org.eclipse.jetty.servlet.FilterMapping;
	using ServletHandler = org.eclipse.jetty.servlet.ServletHandler;
	using ServletHolder = org.eclipse.jetty.servlet.ServletHolder;
	using DoSFilter = org.eclipse.jetty.servlets.DoSFilter;
	using GzipFilter = org.eclipse.jetty.servlets.GzipFilter;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;


	public sealed class Peers
	{

		public enum Event
		{
			BLACKLIST,
			UNBLACKLIST,
			DEACTIVATE,
			REMOVE,
			DOWNLOADED_VOLUME,
			UPLOADED_VOLUME,
			WEIGHT,
			ADDED_ACTIVE_PEER,
			CHANGED_ACTIVE_PEER,
			NEW_PEER
		}

		internal const int LOGGING_MASK_EXCEPTIONS = 1;
		internal const int LOGGING_MASK_NON200_RESPONSES = 2;
		internal const int LOGGING_MASK_200_RESPONSES = 4;
		internal static readonly int communicationLoggingMask;

		internal static readonly Set<string> wellKnownPeers;
		internal static readonly Set<string> knownBlacklistedPeers;

		internal static readonly int connectTimeout;
		internal static readonly int readTimeout;
		internal static readonly int blacklistingPeriod;
		internal static readonly bool getMorePeers;

		internal const int DEFAULT_PEER_PORT = 7874;
		internal const int TESTNET_PEER_PORT = 6874;
		private static readonly string myPlatform;
		private static readonly string myAddress;
		private static readonly int myPeerServerPort;
		private static readonly string myHallmark;
		private static readonly bool shareMyAddress;
		private static readonly int maxNumberOfConnectedPublicPeers;
		private static readonly bool enableHallmarkProtection;
		private static readonly int pushThreshold;
		private static readonly int pullThreshold;
		private static readonly int sendToPeersLimit;
		private static readonly bool usePeersDb;
		private static readonly bool savePeers;
		private static readonly string dumpPeersVersion;


		internal static readonly JSONStreamAware myPeerInfoRequest;
		internal static readonly JSONStreamAware myPeerInfoResponse;

		private static readonly Listeners<Peer, Event> listeners = new Listeners<>();

		private static readonly ConcurrentMap<string, PeerImpl> peers = new ConcurrentHashMap<>();
		private static readonly ConcurrentMap<string, string> announcedAddresses = new ConcurrentHashMap<>();

		internal static readonly ICollection<PeerImpl> allPeers = Collections.unmodifiableCollection(peers.values());

		private static readonly ExecutorService sendToPeersService = Executors.newFixedThreadPool(10);

		static Peers()
		{

			myPlatform = Nxt.getStringProperty("nxt.myPlatform");
			myAddress = Nxt.getStringProperty("nxt.myAddress");
			if(myAddress != null && myAddress.EndsWith(":" + TESTNET_PEER_PORT) && !Constants.isTestnet)
			{
				throw new Exception("Port " + TESTNET_PEER_PORT + " should only be used for testnet!!!");
			}
			myPeerServerPort = Nxt.getIntProperty("nxt.peerServerPort");
			if(myPeerServerPort == TESTNET_PEER_PORT && !Constants.isTestnet)
			{
				throw new Exception("Port " + TESTNET_PEER_PORT + " should only be used for testnet!!!");
			}
			shareMyAddress = Nxt.getBooleanProperty("nxt.shareMyAddress") && ! Constants.isOffline;
			myHallmark = Nxt.getStringProperty("nxt.myHallmark");
			if(Peers.myHallmark != null && Peers.myHallmark.Length > 0)
			{
				try
				{
					Hallmark hallmark = Hallmark.parseHallmark(Peers.myHallmark);
					if(!hallmark.Valid || myAddress == null)
					{
						throw new Exception();
					}
					URI uri = new URI("http://" + myAddress.Trim());
					string host = uri.Host;
					if(!hallmark.Host.Equals(host))
					{
						throw new Exception();
					}
				}
				catch(Exception | URISyntaxException e)
				{
					Logger.logMessage("Your hallmark is invalid: " + Peers.myHallmark + " for your address: " + myAddress);
					throw new Exception(e.ToString(), e);
				}
			}

			JSONObject json = new JSONObject();
			if(myAddress != null && myAddress.Length > 0)
			{
				try
				{
					URI uri = new URI("http://" + myAddress.Trim());
					string host = uri.Host;
					int port = uri.Port;
					if(!Constants.isTestnet)
					{
						if(port >= 0)
							json.put("announcedAddress", myAddress);
						else
							json.put("announcedAddress", host + (myPeerServerPort != DEFAULT_PEER_PORT ? ":" + myPeerServerPort : ""));
					}
					else
					{
						json.put("announcedAddress", host);
					}
				}
				catch(URISyntaxException e)
				{
					Logger.logMessage("Your announce address is invalid: " + myAddress);
					throw new Exception(e.ToString(), e);
				}
			}
			if(Peers.myHallmark != null && Peers.myHallmark.Length > 0)
			{
				json.put("hallmark", Peers.myHallmark);
			}
			json.put("application", Nxt.APPLICATION);
			json.put("version", Nxt.VERSION);
			json.put("platform", Peers.myPlatform);
			json.put("shareAddress", Peers.shareMyAddress);
			Logger.logDebugMessage("My peer info:\n" + json.toJSONString());
			myPeerInfoResponse = JSON.prepare(json);
			json.put("requestType", "getInfo");
			myPeerInfoRequest = JSON.prepareRequest(json);

			IList<string> wellKnownPeersList = Constants.isTestnet ? Nxt.getStringListProperty("nxt.testnetPeers") : Nxt.getStringListProperty("nxt.wellKnownPeers");
			if(wellKnownPeersList.Count == 0 || Constants.isOffline)
			{
				wellKnownPeers = Collections.emptySet();
			}
			else
			{
				wellKnownPeers = Collections.unmodifiableSet(new HashSet<>(wellKnownPeersList));
			}

			IList<string> knownBlacklistedPeersList = Nxt.getStringListProperty("nxt.knownBlacklistedPeers");
			if(knownBlacklistedPeersList.Count == 0)
			{
				knownBlacklistedPeers = Collections.emptySet();
			}
			else
			{
				knownBlacklistedPeers = Collections.unmodifiableSet(new HashSet<>(knownBlacklistedPeersList));
			}

			maxNumberOfConnectedPublicPeers = Nxt.getIntProperty("nxt.maxNumberOfConnectedPublicPeers");
			connectTimeout = Nxt.getIntProperty("nxt.connectTimeout");
			readTimeout = Nxt.getIntProperty("nxt.readTimeout");
			enableHallmarkProtection = Nxt.getBooleanProperty("nxt.enableHallmarkProtection");
			pushThreshold = Nxt.getIntProperty("nxt.pushThreshold");
			pullThreshold = Nxt.getIntProperty("nxt.pullThreshold");

			blacklistingPeriod = Nxt.getIntProperty("nxt.blacklistingPeriod");
			communicationLoggingMask = Nxt.getIntProperty("nxt.communicationLoggingMask");
			sendToPeersLimit = Nxt.getIntProperty("nxt.sendToPeersLimit");
			usePeersDb = Nxt.getBooleanProperty("nxt.usePeersDb") && ! Constants.isOffline;
			savePeers = usePeersDb && Nxt.getBooleanProperty("nxt.savePeers");
			getMorePeers = Nxt.getBooleanProperty("nxt.getMorePeers");
			dumpPeersVersion = Nxt.getStringProperty("nxt.dumpPeersVersion");

//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final List<Future<String>> unresolvedPeers = Collections.synchronizedList(new ArrayList<Future<String>>());
			IList<Future<string>> unresolvedPeers = Collections.synchronizedList(new List<Future<string>>());

			ThreadPool.runBeforeStart(new Runnable() { private void loadPeers(ICollection<string> addresses) { for(final string address : addresses) { Future<string> unresolvedAddress = sendToPeersService.submit(new Callable<string>() { public string call() { Peer peer = Peers.addPeer(address); return peer == null ? address : null; } }); unresolvedPeers.Add(unresolvedAddress); } } Override public void run() { if(! wellKnownPeers.Empty) { loadPeers(wellKnownPeers); } if(usePeersDb) { Logger.logDebugMessage("Loading known peers from the database..."); loadPeers(PeerDb.loadPeers()); } } }, false);

			ThreadPool.runAfterStart(new Runnable() { public void run() { for(Future<string> unresolvedPeer : unresolvedPeers) { try { string badAddress = unresolvedPeer.get(5, TimeUnit.SECONDS); if(badAddress != null) { Logger.logDebugMessage("Failed to resolve peer address: " + badAddress); } } catch(InterruptedException e) { Thread.CurrentThread.interrupt(); } catch(ExecutionException e) { Logger.logDebugMessage("Failed to add peer", e); } catch(TimeoutException e) { } } Logger.logDebugMessage("Known peers: " + peers.size()); } });

				if(Peers.shareMyAddress)
				{
					peerServer = new Server();
					ServerConnector connector = new ServerConnector(peerServer);
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int port = Constants.isTestnet ? TESTNET_PEER_PORT : Peers.myPeerServerPort;
					int port = Constants.isTestnet ? TESTNET_PEER_PORT : Peers.myPeerServerPort;
					connector.Port = port;
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String host = Nxt.getStringProperty("nxt.peerServerHost");
					string host = Nxt.getStringProperty("nxt.peerServerHost");
					connector.Host = host;
					connector.IdleTimeout = Nxt.getIntProperty("nxt.peerServerIdleTimeout");
					connector.ReuseAddress = true;
					peerServer.addConnector(connector);

					ServletHolder peerServletHolder = new ServletHolder(new PeerServlet());
					bool isGzipEnabled = Nxt.getBooleanProperty("nxt.enablePeerServerGZIPFilter");
					peerServletHolder.setInitParameter("isGzipEnabled", bool.ToString(isGzipEnabled));
					ServletHandler peerHandler = new ServletHandler();
					peerHandler.addServletWithMapping(peerServletHolder, "/*");
					if(Nxt.getBooleanProperty("nxt.enablePeerServerDoSFilter"))
					{
						FilterHolder dosFilterHolder = peerHandler.addFilterWithMapping(typeof(DoSFilter), "/*", FilterMapping.DEFAULT);
						dosFilterHolder.setInitParameter("maxRequestsPerSec", Nxt.getStringProperty("nxt.peerServerDoSFilter.maxRequestsPerSec"));
						dosFilterHolder.setInitParameter("delayMs", Nxt.getStringProperty("nxt.peerServerDoSFilter.delayMs"));
						dosFilterHolder.setInitParameter("maxRequestMs", Nxt.getStringProperty("nxt.peerServerDoSFilter.maxRequestMs"));
						dosFilterHolder.setInitParameter("trackSessions", "false");
						dosFilterHolder.AsyncSupported = true;
					}
					if(isGzipEnabled)
					{
						FilterHolder gzipFilterHolder = peerHandler.addFilterWithMapping(typeof(GzipFilter), "/*", FilterMapping.DEFAULT);
						gzipFilterHolder.setInitParameter("methods", "GET,POST");
						gzipFilterHolder.AsyncSupported = true;
					}

					peerServer.Handler = peerHandler;
					peerServer.StopAtShutdown = true;
					ThreadPool.runBeforeStart(new Runnable() { public void run() { try { peerServer.start(); Logger.logMessage("Started peer networking server at " + host + ":" + port); } catch(Exception e) { Logger.logErrorMessage("Failed to start peer networking server", e); throw new Exception(e.ToString(), e); } } }, true);
				}
				else
				{
					peerServer = null;
					Logger.logMessage("shareMyAddress is disabled, will not start peer networking server");
				}
			Account.addListener(new Listener<Account>() { public void notify(Account account) { for(PeerImpl peer : Peers.peers.values()) { if(peer.Hallmark != null && peer.Hallmark.AccountId == account.Id) { Peers.listeners.notify(peer, Peers.Event.WEIGHT); } } } }, Account.Event.BALANCE);
			if(! Constants.isOffline)
			{
				ThreadPool.scheduleThread("PeerConnecting", Peers.peerConnectingThread, 5);
				ThreadPool.scheduleThread("PeerUnBlacklisting", Peers.peerUnBlacklistingThread, 1);
				if(Peers.getMorePeers)
				{
					ThreadPool.scheduleThread("GetMorePeers", Peers.getMorePeersThread, 5);
				}
			}
		}

		private class Init
		{

			private static readonly Server peerServer;


			private static void init()
			{
			}

			private Init()
			{
			}

		}

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private static final Runnable peerUnBlacklistingThread = new Runnable()
//	{
//
//		@Override public void run()
//		{
//
//			try
//			{
//				try
//				{
//
//					long curTime = System.currentTimeMillis();
//					for (PeerImpl peer : peers.values())
//					{
//						peer.updateBlacklistedStatus(curTime);
//					}
//
//				}
//				catch (Exception e)
//				{
//					Logger.logDebugMessage("Error un-blacklisting peer", e);
//				}
//			}
//			catch (Throwable t)
//			{
//				Logger.logMessage("CRITICAL ERROR. PLEASE REPORT TO THE DEVELOPERS.\n" + t.toString());
//				t.printStackTrace();
//				System.exit(1);
//			}
//
//		}
//
//	};

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private static final Runnable peerConnectingThread = new Runnable()
//	{
//
//		@Override public void run()
//		{
//
//			try
//			{
//				try
//				{
//
//					if (getNumberOfConnectedPublicPeers() < Peers.maxNumberOfConnectedPublicPeers)
//					{
//						PeerImpl peer = (PeerImpl)getAnyPeer(ThreadLocalRandom.current().nextInt(2) == 0 ? Peer.State.NON_CONNECTED : Peer.State.DISCONNECTED, false);
//						if (peer != null)
//						{
//							peer.connect();
//						}
//					}
//
//					int now = Nxt.getEpochTime();
//					for (PeerImpl peer : peers.values())
//					{
//						if (peer.getState() == Peer.State.CONNECTED && now - peer.getLastUpdated() > 3600)
//						{
//							peer.connect();
//						}
//					}
//
//				}
//				catch (Exception e)
//				{
//					Logger.logDebugMessage("Error connecting to peer", e);
//				}
//			}
//			catch (Throwable t)
//			{
//				Logger.logMessage("CRITICAL ERROR. PLEASE REPORT TO THE DEVELOPERS.\n" + t.toString());
//				t.printStackTrace();
//				System.exit(1);
//			}
//
//		}
//
//	};

//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//		private static final Runnable getMorePeersThread = new Runnable()
//	{
//
//		private final JSONStreamAware getPeersRequest;
//		{
//			JSONObject request = new JSONObject();
//			request.put("requestType", "getPeers");
//			getPeersRequest = JSON.prepareRequest(request);
//		}
//
//		private volatile boolean addedNewPeer;
//		{
//			Peers.addListener(new Listener<Peer>() { @Override public void notify(Peer peer) { addedNewPeer = true; } }, Event.NEW_PEER);
//		}
//
//		@Override public void run()
//		{
//
//			try
//			{
//				try
//				{
//
//					Peer peer = getAnyPeer(Peer.State.CONNECTED, true);
//					if (peer == null)
//					{
//						return;
//					}
//					JSONObject response = peer.send(getPeersRequest);
//					if (response == null)
//					{
//						return;
//					}
//					JSONArray peers = (JSONArray)response.get("peers");
//					Set<String> addedAddresses = new HashSet<>();
//					if (peers != null)
//					{
//						for (Object announcedAddress : peers)
//						{
//							if (addPeer((String) announcedAddress) != null)
//							{
//								addedAddresses.add((String) announcedAddress);
//							}
//						}
//						if (savePeers && addedNewPeer)
//						{
//							updateSavedPeers();
//							addedNewPeer = false;
//						}
//					}
//
//					JSONArray myPeers = new JSONArray();
//					for (Peer myPeer : Peers.getAllPeers())
//					{
//						if (! myPeer.isBlacklisted() && myPeer.getAnnouncedAddress() != null && myPeer.getState() == Peer.State.CONNECTED && myPeer.shareAddress() && ! addedAddresses.contains(myPeer.getAnnouncedAddress()) && ! myPeer.getAnnouncedAddress().equals(peer.getAnnouncedAddress()))
//						{
//							myPeers.add(myPeer.getAnnouncedAddress());
//						}
//					}
//					if (myPeers.size() > 0)
//					{
//						JSONObject request = new JSONObject();
//						request.put("requestType", "addPeers");
//						request.put("peers", myPeers);
//						peer.send(JSON.prepareRequest(request));
//					}
//
//				}
//				catch (Exception e)
//				{
//					Logger.logDebugMessage("Error requesting peers from a peer", e);
//				}
//			}
//			catch (Throwable t)
//			{
//				Logger.logMessage("CRITICAL ERROR. PLEASE REPORT TO THE DEVELOPERS.\n" + t.toString());
//				t.printStackTrace();
//				System.exit(1);
//			}
//
//		}
//
//		private void updateSavedPeers()
//		{
//			Set<String> oldPeers = new HashSet<>(PeerDb.loadPeers());
//			Set<String> currentPeers = new HashSet<>();
//			for (Peer peer : Peers.peers.values())
//			{
//				if (peer.getAnnouncedAddress() != null && ! peer.isBlacklisted())
//				{
//					currentPeers.add(peer.getAnnouncedAddress());
//				}
//			}
//			Set<String> toDelete = new HashSet<>(oldPeers);
//			toDelete.removeAll(currentPeers);
//			try
//			{
//				Db.beginTransaction();
//				PeerDb.deletePeers(toDelete);
//				//Logger.logDebugMessage("Deleted " + toDelete.size() + " peers from the peers database");
//				currentPeers.removeAll(oldPeers);
//				PeerDb.addPeers(currentPeers);
//				//Logger.logDebugMessage("Added " + currentPeers.size() + " peers to the peers database");
//				Db.commitTransaction();
//			}
//			catch (Exception e)
//			{
//				Db.rollbackTransaction();
//				throw e;
//			}
//			finally
//			{
//				Db.endTransaction();
//			}
//		}
//
//	};



		public static void init()
		{
			Init.init();
		}

		public static void shutdown()
		{
			if(Init.peerServer != null)
			{
				try
				{
					Init.peerServer.stop();
				}
				catch(Exception e)
				{
					Logger.logShutdownMessage("Failed to stop peer server", e);
				}
			}
			if(dumpPeersVersion != null)
			{
				StringBuilder buf = new StringBuilder();
				foreach (KeyValuePair<string, string> entry in announcedAddresses.entrySet())
				{
					Peer peer = peers.get(entry.Value);
					if(peer != null && peer.State == Peer.State.CONNECTED && peer.shareAddress() && !peer.Blacklisted && peer.Version != null && peer.Version.StartsWith(dumpPeersVersion))
					{
						buf.Append("('").append(entry.Key).append("'), ");
					}
				}
				Logger.logShutdownMessage(buf.ToString());
			}
			ThreadPool.shutdownExecutor(sendToPeersService);

		}

		public static bool addListener(Listener<Peer> listener, Event eventType)
		{
			return Peers.listeners.addListener(listener, eventType);
		}

		public static bool removeListener(Listener<Peer> listener, Event eventType)
		{
			return Peers.listeners.removeListener(listener, eventType);
		}

		internal static void notifyListeners(Peer peer, Event eventType)
		{
			Peers.listeners.notify(peer, eventType);
		}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public static Collection<? extends Peer> getAllPeers()
		public static ICollection<?> getAllPeers()
		{
			get
	//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
	//ORIGINAL LINE: public static Collection<? extends Peer> getAllPeers()
			{
				return allPeers;
			}
		}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public static Collection<? extends Peer> getActivePeers()
		public static ICollection<?> getActivePeers()
		{
			get
	//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
	//ORIGINAL LINE: public static Collection<? extends Peer> getActivePeers()
			{
				IList<PeerImpl> activePeers = new List<>();
				foreach (PeerImpl peer in peers.values())
				{
					if(peer.State != Peer.State.NON_CONNECTED)
					{
						activePeers.Add(peer);
					}
				}
				return activePeers;
			}
		}

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public static Collection<? extends Peer> getPeers(Peer.State state)
		public static ICollection<?> getPeers(Peer.State state) where ? : Peer
//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public static Collection<? extends Peer> getPeers(Peer.State state)
		{
			IList<PeerImpl> peerList = new List<>();
			foreach (PeerImpl peer in peers.values())
			{
				if(peer.State == state)
				{
					peerList.Add(peer);
				}
			}
			return peerList;
		}

		public static Peer getPeer(string peerAddress)
		{
			return peers.get(peerAddress);
		}

		public static Peer addPeer(string announcedAddress)
		{
			if(announcedAddress == null)
			{
				return null;
			}
			announcedAddress = announcedAddress.Trim();
			Peer peer;
			if((peer = peers.get(announcedAddress)) != null)
			{
				return peer;
			}
			string address;
			if((address = announcedAddresses.get(announcedAddress)) != null && (peer = peers.get(address)) != null)
			{
				return peer;
			}
			try
			{
				URI uri = new URI("http://" + announcedAddress);
				string host = uri.Host;
				if((peer = peers.get(host)) != null)
				{
					return peer;
				}
				InetAddress inetAddress = InetAddress.getByName(host);
				return addPeer(inetAddress.HostAddress, announcedAddress);
			}
			catch(URISyntaxException | UnknownHostException e)
			{
			//Logger.logDebugMessage("Invalid peer address: " + announcedAddress + ", " + e.toString());
				return null;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: static PeerImpl addPeer(final String address, final String announcedAddress)
		internal static PeerImpl addPeer(string address, string announcedAddress)
		{

		//re-add the [] to ipv6 addresses lost in getHostAddress() above
			string clean_address = address;
			if(StringHelperClass.StringSplit(clean_address, ":", true).length > 2)
			{
				clean_address = "[" + clean_address + "]";
			}
			PeerImpl peer;
			if((peer = peers.get(clean_address)) != null)
			{
				return peer;
			}
			string peerAddress = normalizeHostAndPort(clean_address);
			if(peerAddress == null)
			{
				return null;
			}
			if((peer = peers.get(peerAddress)) != null)
			{
				return peer;
			}

			string announcedPeerAddress = address.Equals(announcedAddress) ? peerAddress : normalizeHostAndPort(announcedAddress);

			if(Peers.myAddress != null && Peers.myAddress.Length > 0 && Peers.myAddress.equalsIgnoreCase(announcedPeerAddress))
			{
				return null;
			}

			peer = new PeerImpl(peerAddress, announcedPeerAddress);
			if(Constants.isTestnet && peer.Port > 0 && peer.Port != TESTNET_PEER_PORT)
			{
				Logger.logDebugMessage("Peer " + peerAddress + " on testnet is not using port " + TESTNET_PEER_PORT + ", ignoring");
				return null;
			}
			peers.put(peerAddress, peer);
			if(announcedAddress != null)
			{
				updateAddress(peer);
			}
			listeners.notify(peer, Event.NEW_PEER);
			return peer;
		}

		internal static PeerImpl removePeer(PeerImpl peer)
		{
			if(peer.AnnouncedAddress != null)
			{
				announcedAddresses.remove(peer.AnnouncedAddress);
			}
			return peers.remove(peer.PeerAddress);
		}

		internal static void updateAddress(PeerImpl peer)
		{
			string oldAddress = announcedAddresses.put(peer.AnnouncedAddress, peer.PeerAddress);
			if(oldAddress != null && !peer.PeerAddress.Equals(oldAddress))
			{
			//Logger.logDebugMessage("Peer " + peer.getAnnouncedAddress() + " has changed address from " + oldAddress
			//        + " to " + peer.getPeerAddress());
				Peer oldPeer = peers.remove(oldAddress);
				if(oldPeer != null)
				{
					Peers.notifyListeners(oldPeer, Peers.Event.REMOVE);
				}
			}
		}

		public static void sendToSomePeers(Block block)
		{
			JSONObject request = block.JSONObject;
			request.put("requestType", "processBlock");
			sendToSomePeers(request);
		}

		public static void sendToSomePeers(IList<Transaction> transactions)
		{
			JSONObject request = new JSONObject();
			JSONArray transactionsData = new JSONArray();
			foreach (Transaction transaction in transactions)
			{
				transactionsData.add(transaction.JSONObject);
			}
			request.put("requestType", "processTransactions");
			request.put("transactions", transactionsData);
			sendToSomePeers(request);
		}

//JAVA TO VB & C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: private static void sendToSomePeers(final JSONObject request)
		private static void sendToSomePeers(JSONObject request)
		{

//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final JSONStreamAware jsonRequest = JSON.prepareRequest(request);
			JSONStreamAware jsonRequest = JSON.prepareRequest(request);

			int successful = 0;
			IList<Future<JSONObject>> expectedResponses = new List<>();
			foreach (Peer peer in peers.values())
			{

				if(Peers.enableHallmarkProtection && peer.Weight < Peers.pushThreshold)
				{
					continue;
				}

				if(! peer.Blacklisted && peer.State == Peer.State.CONNECTED && peer.AnnouncedAddress != null)
				{
					Future<JSONObject> futureResponse = sendToPeersService.submit(new Callable<JSONObject>() { public JSONObject call() { return peer.send(jsonRequest); } });
					expectedResponses.Add(futureResponse);
				}
				if(expectedResponses.Count >= Peers.sendToPeersLimit - successful)
				{
					foreach (Future<JSONObject> future in expectedResponses)
					{
						try
						{
							JSONObject response = future.get();
							if(response != null && response.get("error") == null)
							{
								successful += 1;
							}
						}
						catch(InterruptedException e)
						{
							Thread.CurrentThread.interrupt();
						}
						catch(ExecutionException e)
						{
							Logger.logDebugMessage("Error in sendToSomePeers", e);
						}

					}
					expectedResponses.Clear();
				}
				if(successful >= Peers.sendToPeersLimit)
				{
					return;
				}

			}

		}

		public static Peer getAnyPeer(Peer.State state, bool applyPullThreshold)
		{

			IList<Peer> selectedPeers = new List<>();
			foreach (Peer peer in peers.values())
			{
				if(! peer.Blacklisted && peer.State == state && peer.shareAddress() && (!applyPullThreshold || ! Peers.enableHallmarkProtection || peer.Weight >= Peers.pullThreshold))
				{
					selectedPeers.Add(peer);
				}
			}

			if(selectedPeers.Count > 0)
			{
				if(! Peers.enableHallmarkProtection)
				{
					return selectedPeers[ThreadLocalRandom.current().Next(selectedPeers.Count)];
				}

				long totalWeight = 0;
				foreach (Peer peer in selectedPeers)
				{
					long weight = peer.Weight;
					if(weight == 0)
					{
						weight = 1;
					}
					totalWeight += weight;
				}

				long hit = ThreadLocalRandom.current().nextLong(totalWeight);
				foreach (Peer peer in selectedPeers)
				{
					long weight = peer.Weight;
					if(weight == 0)
					{
						weight = 1;
					}
					if((hit -= weight) < 0)
					{
						return peer;
					}
				}
			}
			return null;
		}

		internal static string normalizeHostAndPort(string address)
		{
			try
			{
				if(address == null)
				{
					return null;
				}
				URI uri = new URI("http://" + address.Trim());
				string host = uri.Host;
				if(host == null || host.Equals("") || host.Equals("localhost") || host.Equals("127.0.0.1") || host.Equals("[0:0:0:0:0:0:0:1]"))
				{
					return null;
				}
				InetAddress inetAddress = InetAddress.getByName(host);
				if(inetAddress.AnyLocalAddress || inetAddress.LoopbackAddress || inetAddress.LinkLocalAddress)
				{
					return null;
				}
				int port = uri.Port;
				return port == -1 ? host : host + ':' + port;
			}
			catch(URISyntaxException |UnknownHostException e)
			{
				return null;
			}
		}

		private static int NumberOfConnectedPublicPeers
		{
			get
			{
				int numberOfConnectedPeers = 0;
				foreach (Peer peer in peers.values())
				{
					if(peer.State == Peer.State.CONNECTED && peer.AnnouncedAddress != null && (peer.Weight > 0 || ! Peers.enableHallmarkProtection))
					{
						numberOfConnectedPeers++;
					}
				}
				return numberOfConnectedPeers;
			}
		}

		private Peers() // never
		{
		}

	}

}