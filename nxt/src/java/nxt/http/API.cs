using System;
using System.Collections.Generic;

namespace nxt.http
{

	using Constants = nxt.Constants;
	using Nxt = nxt.Nxt;
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
	using DefaultServlet = org.eclipse.jetty.servlet.DefaultServlet;
	using FilterHolder = org.eclipse.jetty.servlet.FilterHolder;
	using ServletContextHandler = org.eclipse.jetty.servlet.ServletContextHandler;
	using ServletHolder = org.eclipse.jetty.servlet.ServletHolder;
	using CrossOriginFilter = org.eclipse.jetty.servlets.CrossOriginFilter;
	using GzipFilter = org.eclipse.jetty.servlets.GzipFilter;
	using SslContextFactory = org.eclipse.jetty.util.ssl.SslContextFactory;


	public sealed class API
	{

		private const int TESTNET_API_PORT = 6876;

		internal static readonly Set<string> allowedBotHosts;
		internal const bool enableDebugAPI = Nxt.getBooleanProperty("nxt.enableDebugAPI");

		private static readonly Server apiServer;

		static API()
		{
			IList<string> allowedBotHostsList = Nxt.getStringListProperty("nxt.allowedBotHosts");
			if(! allowedBotHostsList.Contains("*"))
			{
				allowedBotHosts = Collections.unmodifiableSet(new HashSet<>(allowedBotHostsList));
			}
			else
			{
				allowedBotHosts = null;
			}

			bool enableAPIServer = Nxt.getBooleanProperty("nxt.enableAPIServer");
			if(enableAPIServer)
			{
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int port = Constants.isTestnet ? TESTNET_API_PORT : Nxt.getIntProperty("nxt.apiServerPort");
				int port = Constants.isTestnet ? TESTNET_API_PORT : Nxt.getIntProperty("nxt.apiServerPort");
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String host = Nxt.getStringProperty("nxt.apiServerHost");
				string host = Nxt.getStringProperty("nxt.apiServerHost");
				apiServer = new Server();
				ServerConnector connector;

				bool enableSSL = Nxt.getBooleanProperty("nxt.apiSSL");
				if(enableSSL)
				{
					Logger.logMessage("Using SSL (https) for the API server");
					HttpConfiguration https_config = new HttpConfiguration();
					https_config.SecureScheme = "https";
					https_config.SecurePort = port;
					https_config.addCustomizer(new SecureRequestCustomizer());
					SslContextFactory sslContextFactory = new SslContextFactory();
					sslContextFactory.KeyStorePath = Nxt.getStringProperty("nxt.keyStorePath");
					sslContextFactory.KeyStorePassword = Nxt.getStringProperty("nxt.keyStorePassword");
					sslContextFactory.setExcludeCipherSuites("SSL_RSA_WITH_DES_CBC_SHA", "SSL_DHE_RSA_WITH_DES_CBC_SHA", "SSL_DHE_DSS_WITH_DES_CBC_SHA", "SSL_RSA_EXPORT_WITH_RC4_40_MD5", "SSL_RSA_EXPORT_WITH_DES40_CBC_SHA", "SSL_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA", "SSL_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA");
					sslContextFactory.ExcludeProtocols = "SSLv3";
					connector = new ServerConnector(apiServer, new SslConnectionFactory(sslContextFactory, "http/1.1"), new HttpConnectionFactory(https_config));
				}
				else
				{
					connector = new ServerConnector(apiServer);
				}

				connector.Port = port;
				connector.Host = host;
				connector.IdleTimeout = Nxt.getIntProperty("nxt.apiServerIdleTimeout");
				connector.ReuseAddress = true;
				apiServer.addConnector(connector);

				HandlerList apiHandlers = new HandlerList();

				ServletContextHandler apiHandler = new ServletContextHandler();
				string apiResourceBase = Nxt.getStringProperty("nxt.apiResourceBase");
				if(apiResourceBase != null)
				{
					ServletHolder defaultServletHolder = new ServletHolder(new DefaultServlet());
					defaultServletHolder.setInitParameter("dirAllowed", "false");
					defaultServletHolder.setInitParameter("resourceBase", apiResourceBase);
					defaultServletHolder.setInitParameter("welcomeServlets", "true");
					defaultServletHolder.setInitParameter("redirectWelcome", "true");
					defaultServletHolder.setInitParameter("gzip", "true");
					apiHandler.addServlet(defaultServletHolder, "/*");
					apiHandler.WelcomeFiles = new string[]{"index.html"};
				}

				string javadocResourceBase = Nxt.getStringProperty("nxt.javadocResourceBase");
				if(javadocResourceBase != null)
				{
					ContextHandler contextHandler = new ContextHandler("/doc");
					ResourceHandler docFileHandler = new ResourceHandler();
					docFileHandler.DirectoriesListed = false;
					docFileHandler.WelcomeFiles = new string[]{"index.html"};
					docFileHandler.ResourceBase = javadocResourceBase;
					contextHandler.Handler = docFileHandler;
					apiHandlers.addHandler(contextHandler);
				}

				apiHandler.addServlet(typeof(APIServlet), "/nxt");
				if(Nxt.getBooleanProperty("nxt.enableAPIServerGZIPFilter"))
				{
					FilterHolder gzipFilterHolder = apiHandler.addFilter(typeof(GzipFilter), "/nxt", null);
					gzipFilterHolder.setInitParameter("methods", "GET,POST");
					gzipFilterHolder.AsyncSupported = true;
				}

				apiHandler.addServlet(typeof(APITestServlet), "/test");
				if(enableDebugAPI)
				{
					apiHandler.addServlet(typeof(DbShellServlet), "/dbshell");
				}

				if(Nxt.getBooleanProperty("nxt.apiServerCORS"))
				{
					FilterHolder filterHolder = apiHandler.addFilter(typeof(CrossOriginFilter), "/*", null);
					filterHolder.setInitParameter("allowedHeaders", "*");
					filterHolder.AsyncSupported = true;
				}

				apiHandlers.addHandler(apiHandler);
				apiHandlers.addHandler(new DefaultHandler());

				apiServer.Handler = apiHandlers;
				apiServer.StopAtShutdown = true;

				ThreadPool.runBeforeStart(new Runnable() { public void run() { try { apiServer.start(); Logger.logMessage("Started API server at " + host + ":" + port); } catch(Exception e) { Logger.logErrorMessage("Failed to start API server", e); throw new Exception(e.ToString(), e); } } }, true);

			}
			else
			{
				apiServer = null;
				Logger.logMessage("API server not enabled");
			}

		}

		public static void init()
		{
		}

		public static void shutdown()
		{
			if(apiServer != null)
			{
				try
				{
					apiServer.stop();
				}
				catch(Exception e)
				{
					Logger.logShutdownMessage("Failed to stop API server", e);
				}
			}
		}

		private API() // never
		{
		}

	}

}