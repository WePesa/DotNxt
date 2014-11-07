using System;
using System.Collections.Generic;

namespace nxt.user
{

	using Account = nxt.Account;
	using Block = nxt.Block;
	using BlockchainProcessor = nxt.BlockchainProcessor;
	using Constants = nxt.Constants;
	using Generator = nxt.Generator;
	using Nxt = nxt.Nxt;
	using Transaction = nxt.Transaction;
	using TransactionProcessor = nxt.TransactionProcessor;
	using Peer = nxt.peer.Peer;
	using Peers = nxt.peer.Peers;
	using Convert = nxt.util.Convert;
	using Listener = nxt.util.Listener;
	using Logger = nxt.util.Logger;
	using ThreadPool = nxt.util.ThreadPool;
	using HttpConfiguration = org.eclipse.jetty.server.HttpConfiguration;
	using HttpConnectionFactory = org.eclipse.jetty.server.HttpConnectionFactory;
	using SecureRequestCustomizer = org.eclipse.jetty.server.SecureRequestCustomizer;
	using Server = org.eclipse.jetty.server.Server;
	using ServerConnector = org.eclipse.jetty.server.ServerConnector;
	using SslConnectionFactory = org.eclipse.jetty.server.SslConnectionFactory;
	using ContextHandler = org.eclipse.jetty.server.handler.ContextHandler;
	using DefaultHandler = org.eclipse.jetty.server.handler.DefaultHandler;
	using HandlerList = org.eclipse.jetty.server.handler.HandlerList;
	using ResourceHandler = org.eclipse.jetty.server.handler.ResourceHandler;
	using FilterHolder = org.eclipse.jetty.servlet.FilterHolder;
	using FilterMapping = org.eclipse.jetty.servlet.FilterMapping;
	using ServletHandler = org.eclipse.jetty.servlet.ServletHandler;
	using ServletHolder = org.eclipse.jetty.servlet.ServletHolder;
	using CrossOriginFilter = org.eclipse.jetty.servlets.CrossOriginFilter;
	using SslContextFactory = org.eclipse.jetty.util.ssl.SslContextFactory;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;


	public sealed class Users
	{

		private const int TESTNET_UI_PORT =6875;

		private static readonly ConcurrentMap<string, User> users = new ConcurrentHashMap<>();
		private static readonly ICollection<User> allUsers = Collections.unmodifiableCollection(users.values());

		private static readonly AtomicInteger peerCounter = new AtomicInteger();
		private static readonly ConcurrentMap<string, int?> peerIndexMap = new ConcurrentHashMap<>();
		private static readonly ConcurrentMap<int?, string> peerAddressMap = new ConcurrentHashMap<>();

		private static readonly AtomicInteger blockCounter = new AtomicInteger();
		private static readonly ConcurrentMap<long?, int?> blockIndexMap = new ConcurrentHashMap<>();

		private static readonly AtomicInteger transactionCounter = new AtomicInteger();
		private static readonly ConcurrentMap<long?, int?> transactionIndexMap = new ConcurrentHashMap<>();

		internal static readonly Set<string> allowedUserHosts;

		private static readonly Server userServer;

		static Users()
		{

			IList<string> allowedUserHostsList = Nxt.getStringListProperty("nxt.allowedUserHosts");
			if(! allowedUserHostsList.Contains("*"))
			{
				allowedUserHosts = Collections.unmodifiableSet(new HashSet<>(allowedUserHostsList));
			}
			else
			{
				allowedUserHosts = null;
			}

			bool enableUIServer = Nxt.getBooleanProperty("nxt.enableUIServer");
			if(enableUIServer)
			{
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int port = Constants.isTestnet ? TESTNET_UI_PORT : Nxt.getIntProperty("nxt.uiServerPort");
				int port = Constants.isTestnet ? TESTNET_UI_PORT : Nxt.getIntProperty("nxt.uiServerPort");
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String host = Nxt.getStringProperty("nxt.uiServerHost");
				string host = Nxt.getStringProperty("nxt.uiServerHost");
				userServer = new Server();
				ServerConnector connector;

				bool enableSSL = Nxt.getBooleanProperty("nxt.uiSSL");
				if(enableSSL)
				{
					Logger.logMessage("Using SSL (https) for the user interface server");
					HttpConfiguration https_config = new HttpConfiguration();
					https_config.SecureScheme = "https";
					https_config.SecurePort = port;
					https_config.addCustomizer(new SecureRequestCustomizer());
					SslContextFactory sslContextFactory = new SslContextFactory();
					sslContextFactory.KeyStorePath = Nxt.getStringProperty("nxt.keyStorePath");
					sslContextFactory.KeyStorePassword = Nxt.getStringProperty("nxt.keyStorePassword");
					sslContextFactory.setExcludeCipherSuites("SSL_RSA_WITH_DES_CBC_SHA", "SSL_DHE_RSA_WITH_DES_CBC_SHA", "SSL_DHE_DSS_WITH_DES_CBC_SHA", "SSL_RSA_EXPORT_WITH_RC4_40_MD5", "SSL_RSA_EXPORT_WITH_DES40_CBC_SHA", "SSL_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA", "SSL_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA");
					sslContextFactory.ExcludeProtocols = "SSLv3";
					connector = new ServerConnector(userServer, new SslConnectionFactory(sslContextFactory, "http/1.1"), new HttpConnectionFactory(https_config));
				}
				else
				{
					connector = new ServerConnector(userServer);
				}

				connector.Port = port;
				connector.Host = host;
				connector.IdleTimeout = Nxt.getIntProperty("nxt.uiServerIdleTimeout");
				connector.ReuseAddress = true;
				userServer.addConnector(connector);


				HandlerList userHandlers = new HandlerList();

				ResourceHandler userFileHandler = new ResourceHandler();
				userFileHandler.DirectoriesListed = false;
				userFileHandler.WelcomeFiles = new string[]{"index.html"};
				userFileHandler.ResourceBase = Nxt.getStringProperty("nxt.uiResourceBase");

				userHandlers.addHandler(userFileHandler);

				string javadocResourceBase = Nxt.getStringProperty("nxt.javadocResourceBase");
				if(javadocResourceBase != null)
				{
					ContextHandler contextHandler = new ContextHandler("/doc");
					ResourceHandler docFileHandler = new ResourceHandler();
					docFileHandler.DirectoriesListed = false;
					docFileHandler.WelcomeFiles = new string[]{"index.html"};
					docFileHandler.ResourceBase = javadocResourceBase;
					contextHandler.Handler = docFileHandler;
					userHandlers.addHandler(contextHandler);
				}

				ServletHandler userHandler = new ServletHandler();
				ServletHolder userHolder = userHandler.addServletWithMapping(typeof(UserServlet), "/nxt");
				userHolder.AsyncSupported = true;

				if(Nxt.getBooleanProperty("nxt.uiServerCORS"))
				{
					FilterHolder filterHolder = userHandler.addFilterWithMapping(typeof(CrossOriginFilter), "/*", FilterMapping.DEFAULT);
					filterHolder.setInitParameter("allowedHeaders", "*");
					filterHolder.AsyncSupported = true;
				}

				userHandlers.addHandler(userHandler);

				userHandlers.addHandler(new DefaultHandler());

				userServer.Handler = userHandlers;
				userServer.StopAtShutdown = true;

				ThreadPool.runBeforeStart(new Runnable() { public void run() { try { userServer.start(); Logger.logMessage("Started user interface server at " + host + ":" + port); } catch(Exception e) { Logger.logErrorMessage("Failed to start user interface server", e); throw new Exception(e.ToString(), e); } } }, true);

			}
			else
			{
				userServer = null;
				Logger.logMessage("User interface server not enabled");
			}

			if(userServer != null)
			{
				Account.addListener(new Listener<Account>() { public void notify(Account account) { JSONObject response = new JSONObject(); response.put("response", "setBalance"); response.put("balanceNQT", account.UnconfirmedBalanceNQT); sbyte[] accountPublicKey = account.PublicKey; for(User user : Users.users.values()) { if(user.SecretPhrase != null && Array.Equals(user.PublicKey, accountPublicKey)) { user.send(response); } } } }, Account.Event.UNCONFIRMED_BALANCE);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray removedActivePeers = new JSONArray(); JSONObject removedActivePeer = new JSONObject(); removedActivePeer.put("index", Users.getIndex(peer)); removedActivePeers.add(removedActivePeer); response.put("removedActivePeers", removedActivePeers); JSONArray removedKnownPeers = new JSONArray(); JSONObject removedKnownPeer = new JSONObject(); removedKnownPeer.put("index", Users.getIndex(peer)); removedKnownPeers.add(removedKnownPeer); response.put("removedKnownPeers", removedKnownPeers); JSONArray addedBlacklistedPeers = new JSONArray(); JSONObject addedBlacklistedPeer = new JSONObject(); addedBlacklistedPeer.put("index", Users.getIndex(peer)); addedBlacklistedPeer.put("address", peer.PeerAddress); addedBlacklistedPeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true)); if(peer.WellKnown) { addedBlacklistedPeer.put("wellKnown", true); } addedBlacklistedPeer.put("software", peer.Software); addedBlacklistedPeers.add(addedBlacklistedPeer); response.put("addedBlacklistedPeers", addedBlacklistedPeers); Users.sendNewDataToAll(response); } }, Peers.Event.BLACKLIST);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray removedActivePeers = new JSONArray(); JSONObject removedActivePeer = new JSONObject(); removedActivePeer.put("index", Users.getIndex(peer)); removedActivePeers.add(removedActivePeer); response.put("removedActivePeers", removedActivePeers); JSONArray addedKnownPeers = new JSONArray(); JSONObject addedKnownPeer = new JSONObject(); addedKnownPeer.put("index", Users.getIndex(peer)); addedKnownPeer.put("address", peer.PeerAddress); addedKnownPeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true)); if(peer.WellKnown) { addedKnownPeer.put("wellKnown", true); } addedKnownPeer.put("software", peer.Software); addedKnownPeers.add(addedKnownPeer); response.put("addedKnownPeers", addedKnownPeers); Users.sendNewDataToAll(response); } }, Peers.Event.DEACTIVATE);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray removedBlacklistedPeers = new JSONArray(); JSONObject removedBlacklistedPeer = new JSONObject(); removedBlacklistedPeer.put("index", Users.getIndex(peer)); removedBlacklistedPeers.add(removedBlacklistedPeer); response.put("removedBlacklistedPeers", removedBlacklistedPeers); JSONArray addedKnownPeers = new JSONArray(); JSONObject addedKnownPeer = new JSONObject(); addedKnownPeer.put("index", Users.getIndex(peer)); addedKnownPeer.put("address", peer.PeerAddress); addedKnownPeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true)); if(peer.WellKnown) { addedKnownPeer.put("wellKnown", true); } addedKnownPeer.put("software", peer.Software); addedKnownPeers.add(addedKnownPeer); response.put("addedKnownPeers", addedKnownPeers); Users.sendNewDataToAll(response); } }, Peers.Event.UNBLACKLIST);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray removedKnownPeers = new JSONArray(); JSONObject removedKnownPeer = new JSONObject(); removedKnownPeer.put("index", Users.getIndex(peer)); removedKnownPeers.add(removedKnownPeer); response.put("removedKnownPeers", removedKnownPeers); Users.sendNewDataToAll(response); } }, Peers.Event.REMOVE);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray changedActivePeers = new JSONArray(); JSONObject changedActivePeer = new JSONObject(); changedActivePeer.put("index", Users.getIndex(peer)); changedActivePeer.put("downloaded", peer.DownloadedVolume); changedActivePeers.add(changedActivePeer); response.put("changedActivePeers", changedActivePeers); Users.sendNewDataToAll(response); } }, Peers.Event.DOWNLOADED_VOLUME);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray changedActivePeers = new JSONArray(); JSONObject changedActivePeer = new JSONObject(); changedActivePeer.put("index", Users.getIndex(peer)); changedActivePeer.put("uploaded", peer.UploadedVolume); changedActivePeers.add(changedActivePeer); response.put("changedActivePeers", changedActivePeers); Users.sendNewDataToAll(response); } }, Peers.Event.UPLOADED_VOLUME);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray changedActivePeers = new JSONArray(); JSONObject changedActivePeer = new JSONObject(); changedActivePeer.put("index", Users.getIndex(peer)); changedActivePeer.put("weight", peer.Weight); changedActivePeers.add(changedActivePeer); response.put("changedActivePeers", changedActivePeers); Users.sendNewDataToAll(response); } }, Peers.Event.WEIGHT);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray removedKnownPeers = new JSONArray(); JSONObject removedKnownPeer = new JSONObject(); removedKnownPeer.put("index", Users.getIndex(peer)); removedKnownPeers.add(removedKnownPeer); response.put("removedKnownPeers", removedKnownPeers); JSONArray addedActivePeers = new JSONArray(); JSONObject addedActivePeer = new JSONObject(); addedActivePeer.put("index", Users.getIndex(peer)); if(peer.State != Peer.State.CONNECTED) { addedActivePeer.put("disconnected", true); } addedActivePeer.put("address", peer.PeerAddress); addedActivePeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true)); if(peer.WellKnown) { addedActivePeer.put("wellKnown", true); } addedActivePeer.put("weight", peer.Weight); addedActivePeer.put("downloaded", peer.DownloadedVolume); addedActivePeer.put("uploaded", peer.UploadedVolume); addedActivePeer.put("software", peer.Software); addedActivePeers.add(addedActivePeer); response.put("addedActivePeers", addedActivePeers); Users.sendNewDataToAll(response); } }, Peers.Event.ADDED_ACTIVE_PEER);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray changedActivePeers = new JSONArray(); JSONObject changedActivePeer = new JSONObject(); changedActivePeer.put("index", Users.getIndex(peer)); changedActivePeer.put(peer.State == Peer.State.CONNECTED ? "connected" : "disconnected", true); changedActivePeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true)); if(peer.WellKnown) { changedActivePeer.put("wellKnown", true); } changedActivePeers.add(changedActivePeer); response.put("changedActivePeers", changedActivePeers); Users.sendNewDataToAll(response); } }, Peers.Event.CHANGED_ACTIVE_PEER);

				Peers.addListener(new Listener<Peer>() { public void notify(Peer peer) { JSONObject response = new JSONObject(); JSONArray addedKnownPeers = new JSONArray(); JSONObject addedKnownPeer = new JSONObject(); addedKnownPeer.put("index", Users.getIndex(peer)); addedKnownPeer.put("address", peer.PeerAddress); addedKnownPeer.put("announcedAddress", Convert.truncate(peer.AnnouncedAddress, "-", 25, true)); if(peer.WellKnown) { addedKnownPeer.put("wellKnown", true); } addedKnownPeer.put("software", peer.Software); addedKnownPeers.add(addedKnownPeer); response.put("addedKnownPeers", addedKnownPeers); Users.sendNewDataToAll(response); } }, Peers.Event.NEW_PEER);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: Nxt.getTransactionProcessor().addListener(new Listener<List<? extends Transaction>>() { public void notify(List<? extends Transaction> transactions) { JSONObject response = new JSONObject(); JSONArray removedUnconfirmedTransactions = new JSONArray(); for (Transaction transaction : transactions) { JSONObject removedUnconfirmedTransaction = new JSONObject(); removedUnconfirmedTransaction.put("index", Users.getIndex(transaction)); removedUnconfirmedTransactions.add(removedUnconfirmedTransaction); } response.put("removedUnconfirmedTransactions", removedUnconfirmedTransactions); Users.sendNewDataToAll(response); } }, TransactionProcessor.Event.REMOVED_UNCONFIRMED_TRANSACTIONS);
				Nxt.TransactionProcessor.addListener(new Listener<IList<?>>() { public void notify(IList<?> transactions) { JSONObject response = new JSONObject(); JSONArray removedUnconfirmedTransactions = new JSONArray(); for(Transaction transaction : transactions) { JSONObject removedUnconfirmedTransaction = new JSONObject(); removedUnconfirmedTransaction.put("index", Users.getIndex(transaction)); removedUnconfirmedTransactions.add(removedUnconfirmedTransaction); } response.put("removedUnconfirmedTransactions", removedUnconfirmedTransactions); Users.sendNewDataToAll(response); } }, TransactionProcessor.Event.REMOVED_UNCONFIRMED_TRANSACTIONS);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: Nxt.getTransactionProcessor().addListener(new Listener<List<? extends Transaction>>() { public void notify(List<? extends Transaction> transactions) { JSONObject response = new JSONObject(); JSONArray addedUnconfirmedTransactions = new JSONArray(); for (Transaction transaction : transactions) { JSONObject addedUnconfirmedTransaction = new JSONObject(); addedUnconfirmedTransaction.put("index", Users.getIndex(transaction)); addedUnconfirmedTransaction.put("timestamp", transaction.getTimestamp()); addedUnconfirmedTransaction.put("deadline", transaction.getDeadline()); addedUnconfirmedTransaction.put("recipient", Convert.toUnsignedLong(transaction.getRecipientId())); addedUnconfirmedTransaction.put("amountNQT", transaction.getAmountNQT()); addedUnconfirmedTransaction.put("feeNQT", transaction.getFeeNQT()); addedUnconfirmedTransaction.put("sender", Convert.toUnsignedLong(transaction.getSenderId())); addedUnconfirmedTransaction.put("id", transaction.getStringId()); addedUnconfirmedTransactions.add(addedUnconfirmedTransaction); } response.put("addedUnconfirmedTransactions", addedUnconfirmedTransactions); Users.sendNewDataToAll(response); } }, TransactionProcessor.Event.ADDED_UNCONFIRMED_TRANSACTIONS);
				Nxt.TransactionProcessor.addListener(new Listener<IList<?>>() { public void notify(IList<?> transactions) { JSONObject response = new JSONObject(); JSONArray addedUnconfirmedTransactions = new JSONArray(); for(Transaction transaction : transactions) { JSONObject addedUnconfirmedTransaction = new JSONObject(); addedUnconfirmedTransaction.put("index", Users.getIndex(transaction)); addedUnconfirmedTransaction.put("timestamp", transaction.Timestamp); addedUnconfirmedTransaction.put("deadline", transaction.Deadline); addedUnconfirmedTransaction.put("recipient", Convert.toUnsignedLong(transaction.RecipientId)); addedUnconfirmedTransaction.put("amountNQT", transaction.AmountNQT); addedUnconfirmedTransaction.put("feeNQT", transaction.FeeNQT); addedUnconfirmedTransaction.put("sender", Convert.toUnsignedLong(transaction.SenderId)); addedUnconfirmedTransaction.put("id", transaction.StringId); addedUnconfirmedTransactions.add(addedUnconfirmedTransaction); } response.put("addedUnconfirmedTransactions", addedUnconfirmedTransactions); Users.sendNewDataToAll(response); } }, TransactionProcessor.Event.ADDED_UNCONFIRMED_TRANSACTIONS);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: Nxt.getTransactionProcessor().addListener(new Listener<List<? extends Transaction>>() { public void notify(List<? extends Transaction> transactions) { JSONObject response = new JSONObject(); JSONArray addedConfirmedTransactions = new JSONArray(); for (Transaction transaction : transactions) { JSONObject addedConfirmedTransaction = new JSONObject(); addedConfirmedTransaction.put("index", Users.getIndex(transaction)); addedConfirmedTransaction.put("blockTimestamp", transaction.getBlockTimestamp()); addedConfirmedTransaction.put("transactionTimestamp", transaction.getTimestamp()); addedConfirmedTransaction.put("sender", Convert.toUnsignedLong(transaction.getSenderId())); addedConfirmedTransaction.put("recipient", Convert.toUnsignedLong(transaction.getRecipientId())); addedConfirmedTransaction.put("amountNQT", transaction.getAmountNQT()); addedConfirmedTransaction.put("feeNQT", transaction.getFeeNQT()); addedConfirmedTransaction.put("id", transaction.getStringId()); addedConfirmedTransactions.add(addedConfirmedTransaction); } response.put("addedConfirmedTransactions", addedConfirmedTransactions); Users.sendNewDataToAll(response); } }, TransactionProcessor.Event.ADDED_CONFIRMED_TRANSACTIONS);
				Nxt.TransactionProcessor.addListener(new Listener<IList<?>>() { public void notify(IList<?> transactions) { JSONObject response = new JSONObject(); JSONArray addedConfirmedTransactions = new JSONArray(); for(Transaction transaction : transactions) { JSONObject addedConfirmedTransaction = new JSONObject(); addedConfirmedTransaction.put("index", Users.getIndex(transaction)); addedConfirmedTransaction.put("blockTimestamp", transaction.BlockTimestamp); addedConfirmedTransaction.put("transactionTimestamp", transaction.Timestamp); addedConfirmedTransaction.put("sender", Convert.toUnsignedLong(transaction.SenderId)); addedConfirmedTransaction.put("recipient", Convert.toUnsignedLong(transaction.RecipientId)); addedConfirmedTransaction.put("amountNQT", transaction.AmountNQT); addedConfirmedTransaction.put("feeNQT", transaction.FeeNQT); addedConfirmedTransaction.put("id", transaction.StringId); addedConfirmedTransactions.add(addedConfirmedTransaction); } response.put("addedConfirmedTransactions", addedConfirmedTransactions); Users.sendNewDataToAll(response); } }, TransactionProcessor.Event.ADDED_CONFIRMED_TRANSACTIONS);

//JAVA TO VB & C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: Nxt.getTransactionProcessor().addListener(new Listener<List<? extends Transaction>>() { public void notify(List<? extends Transaction> transactions) { JSONObject response = new JSONObject(); JSONArray newTransactions = new JSONArray(); for (Transaction transaction : transactions) { JSONObject newTransaction = new JSONObject(); newTransaction.put("index", Users.getIndex(transaction)); newTransaction.put("timestamp", transaction.getTimestamp()); newTransaction.put("deadline", transaction.getDeadline()); newTransaction.put("recipient", Convert.toUnsignedLong(transaction.getRecipientId())); newTransaction.put("amountNQT", transaction.getAmountNQT()); newTransaction.put("feeNQT", transaction.getFeeNQT()); newTransaction.put("sender", Convert.toUnsignedLong(transaction.getSenderId())); newTransaction.put("id", transaction.getStringId()); newTransactions.add(newTransaction); } response.put("addedDoubleSpendingTransactions", newTransactions); Users.sendNewDataToAll(response); } }, TransactionProcessor.Event.ADDED_DOUBLESPENDING_TRANSACTIONS);
				Nxt.TransactionProcessor.addListener(new Listener<IList<?>>() { public void notify(IList<?> transactions) { JSONObject response = new JSONObject(); JSONArray newTransactions = new JSONArray(); for(Transaction transaction : transactions) { JSONObject newTransaction = new JSONObject(); newTransaction.put("index", Users.getIndex(transaction)); newTransaction.put("timestamp", transaction.Timestamp); newTransaction.put("deadline", transaction.Deadline); newTransaction.put("recipient", Convert.toUnsignedLong(transaction.RecipientId)); newTransaction.put("amountNQT", transaction.AmountNQT); newTransaction.put("feeNQT", transaction.FeeNQT); newTransaction.put("sender", Convert.toUnsignedLong(transaction.SenderId)); newTransaction.put("id", transaction.StringId); newTransactions.add(newTransaction); } response.put("addedDoubleSpendingTransactions", newTransactions); Users.sendNewDataToAll(response); } }, TransactionProcessor.Event.ADDED_DOUBLESPENDING_TRANSACTIONS);

				Nxt.BlockchainProcessor.addListener(new Listener<Block>() { public void notify(Block block) { JSONObject response = new JSONObject(); JSONArray addedOrphanedBlocks = new JSONArray(); JSONObject addedOrphanedBlock = new JSONObject(); addedOrphanedBlock.put("index", Users.getIndex(block)); addedOrphanedBlock.put("timestamp", block.Timestamp); addedOrphanedBlock.put("numberOfTransactions", block.Transactions.size()); addedOrphanedBlock.put("totalAmountNQT", block.TotalAmountNQT); addedOrphanedBlock.put("totalFeeNQT", block.TotalFeeNQT); addedOrphanedBlock.put("payloadLength", block.PayloadLength); addedOrphanedBlock.put("generator", Convert.toUnsignedLong(block.GeneratorId)); addedOrphanedBlock.put("height", block.Height); addedOrphanedBlock.put("version", block.Version); addedOrphanedBlock.put("block", block.StringId); addedOrphanedBlock.put("baseTarget", BigInteger.valueOf(block.BaseTarget).multiply(BigInteger.valueOf(100000)).divide(BigInteger.valueOf(Constants.INITIAL_BASE_TARGET))); addedOrphanedBlocks.add(addedOrphanedBlock); response.put("addedOrphanedBlocks", addedOrphanedBlocks); Users.sendNewDataToAll(response); } }, BlockchainProcessor.Event.BLOCK_POPPED);

				Nxt.BlockchainProcessor.addListener(new Listener<Block>() { public void notify(Block block) { JSONObject response = new JSONObject(); JSONArray addedRecentBlocks = new JSONArray(); JSONObject addedRecentBlock = new JSONObject(); addedRecentBlock.put("index", Users.getIndex(block)); addedRecentBlock.put("timestamp", block.Timestamp); addedRecentBlock.put("numberOfTransactions", block.Transactions.size()); addedRecentBlock.put("totalAmountNQT", block.TotalAmountNQT); addedRecentBlock.put("totalFeeNQT", block.TotalFeeNQT); addedRecentBlock.put("payloadLength", block.PayloadLength); addedRecentBlock.put("generator", Convert.toUnsignedLong(block.GeneratorId)); addedRecentBlock.put("height", block.Height); addedRecentBlock.put("version", block.Version); addedRecentBlock.put("block", block.StringId); addedRecentBlock.put("baseTarget", BigInteger.valueOf(block.BaseTarget).multiply(BigInteger.valueOf(100000)).divide(BigInteger.valueOf(Constants.INITIAL_BASE_TARGET))); addedRecentBlocks.add(addedRecentBlock); response.put("addedRecentBlocks", addedRecentBlocks); Users.sendNewDataToAll(response); } }, BlockchainProcessor.Event.BLOCK_PUSHED);

				Generator.addListener(new Listener<Generator>() { public void notify(Generator generator) { JSONObject response = new JSONObject(); response.put("response", "setBlockGenerationDeadline"); response.put("deadline", generator.Deadline); for(User user : users.values()) { if(Array.Equals(generator.PublicKey, user.PublicKey)) { user.send(response); } } } }, Generator.Event.GENERATION_DEADLINE);
			}

		}



		internal static ICollection<User> AllUsers
		{
			get
			{
				return allUsers;
			}
		}

		internal static User getUser(string userId)
		{
			User user = users.get(userId);
			if(user == null)
			{
				user = new User(userId);
				User oldUser = users.putIfAbsent(userId, user);
				if(oldUser != null)
				{
					user = oldUser;
					user.Inactive = false;
				}
			}
			else
			{
				user.Inactive = false;
			}
			return user;
		}

		internal static User remove(User user)
		{
			return users.remove(user.UserId);
		}

		private static void sendNewDataToAll(JSONObject response)
		{
			response.put("response", "processNewData");
			sendToAll(response);
		}

		private static void sendToAll(JSONStreamAware response)
		{
			foreach (User user in users.values())
			{
				user.send(response);
			}
		}

		internal static int getIndex(Peer peer)
		{
			int? index = peerIndexMap.get(peer.PeerAddress);
			if(index == null)
			{
				index = peerCounter.incrementAndGet();
				peerIndexMap.put(peer.PeerAddress, index);
				peerAddressMap.put(index, peer.PeerAddress);
			}
			return index;
		}

		internal static Peer getPeer(int index)
		{
			string peerAddress = peerAddressMap.get(index);
			if(peerAddress == null)
			{
				return null;
			}
			return Peers.getPeer(peerAddress);
		}

		internal static int getIndex(Block block)
		{
			int? index = blockIndexMap.get(block.Id);
			if(index == null)
			{
				index = blockCounter.incrementAndGet();
				blockIndexMap.put(block.Id, index);
			}
			return index;
		}

		internal static int getIndex(Transaction transaction)
		{
			int? index = transactionIndexMap.get(transaction.Id);
			if(index == null)
			{
				index = transactionCounter.incrementAndGet();
				transactionIndexMap.put(transaction.Id, index);
			}
			return index;
		}

		public static void init()
		{
		}

		public static void shutdown()
		{
			if(userServer != null)
			{
				try
				{
					userServer.stop();
				}
				catch(Exception e)
				{
					Logger.logShutdownMessage("Failed to stop user interface server", e);
				}
			}
		}

		private Users() // never
		{
		}

	}

}