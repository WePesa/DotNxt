using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace nxt
{

	using Db = nxt.db.Db;
	using API = nxt.http.API;
	using Peers = nxt.peer.Peers;
	using Users = nxt.user.Users;
	using Logger = nxt.util.Logger;
	using ThreadPool = nxt.util.ThreadPool;
	using Time = nxt.util.Time;


	public sealed class Nxt
	{

		public const string VERSION = "1.3.2";
		public const string APPLICATION = "NRS";

		private static volatile Time time = new Time.EpochTime();

		private static readonly Properties defaultProperties = new Properties();
		static Nxt()
		{
			Console.WriteLine("Initializing Nxt server version " + Nxt.VERSION);
			using (InputStream is = ClassLoader.getSystemResourceAsStream("nxt-default.properties"))
			{
				if(is != null)
				{
					Nxt.defaultProperties.load(is);
				}
				else
				{
					string configFile = System.getProperty("nxt-default.properties");
					if(configFile != null)
					{
						using (InputStream fis = new FileInputStream(configFile))
						{
							Nxt.defaultProperties.load(fis);
						}
						catch(IOException e)
						{
							throw new Exception("Error loading nxt-default.properties from " + configFile);
						}
					}
					else
					{
						throw new Exception("nxt-default.properties not in classpath and system property nxt-default.properties not defined either");
					}
				}
			}
			catch(IOException e)
			{
				throw new Exception("Error loading nxt-default.properties", e);
			}
			using (InputStream is = ClassLoader.getSystemResourceAsStream("nxt.properties"))
			{
				if(is != null)
				{
					Nxt.properties.load(is);
				} // ignore if missing
			}
			catch(IOException e)
			{
				throw new Exception("Error loading nxt.properties", e);
			}
				try
				{
					long startTime = System.currentTimeMillis();
					Logger.init();
					Db.init();
					TransactionProcessorImpl.Instance;
					BlockchainProcessorImpl.Instance;
					DbVersion.init();
					Account.init();
					Alias.init();
					Asset.init();
					DigitalGoodsStore.init();
					Hub.init();
					Order.init();
					Poll.init();
					Trade.init();
					AssetTransfer.init();
					Vote.init();
					Peers.init();
					Generator.init();
					API.init();
					Users.init();
					DebugTrace.init();
					int timeMultiplier = (Constants.isTestnet && Constants.isOffline) ? Math.Max(Nxt.getIntProperty("nxt.timeMultiplier"), 1) : 1;
					ThreadPool.start(timeMultiplier);
					if(timeMultiplier > 1)
					{
						Time = new Time.FasterTime(Math.Max(EpochTime, Nxt.Blockchain.LastBlock.Timestamp), timeMultiplier);
						Logger.logMessage("TIME WILL FLOW " + timeMultiplier + " TIMES FASTER!");
					}

					long currentTime = System.currentTimeMillis();
					Logger.logMessage("Initialization took " + (currentTime - startTime) / 1000 + " seconds");
					Logger.logMessage("Nxt server " + VERSION + " started successfully.");
					if(Constants.isTestnet)
					{
						Logger.logMessage("RUNNING ON TESTNET - DO NOT USE REAL ACCOUNTS!");
					}
				}
				catch(Exception e)
				{
					Logger.logErrorMessage(e.Message, e);
					Environment.Exit(1);
				}
		}
		private static readonly Properties properties = new Properties(defaultProperties);

		public static int getIntProperty(string name)
		{
			try
			{
				int result = Convert.ToInt32(properties.getProperty(name));
				Logger.logMessage(name + " = \"" + result + "\"");
				return result;
			}
			catch(NumberFormatException e)
			{
				Logger.logMessage(name + " not defined, assuming 0");
				return 0;
			}
		}

		public static string getStringProperty(string name)
		{
			return getStringProperty(name, null);
		}

		public static string getStringProperty(string name, string defaultValue)
		{
			string value = properties.getProperty(name);
			if(value != null && ! "".Equals(value))
			{
				Logger.logMessage(name + " = \"" + value + "\"");
				return value;
			}
			else
			{
				Logger.logMessage(name + " not defined");
				return defaultValue;
			}
		}

		public static IList<string> getStringListProperty(string name)
		{
			string value = getStringProperty(name);
			if(value == null || value.Length == 0)
			{
				return Collections.emptyList();
			}
			IList<string> result = new List<>();
			foreach (string s in StringHelperClass.StringSplit(value, ";", true))
			{
				s = s.Trim();
				if(s.Length > 0)
				{
					result.Add(s);
				}
			}
			return result;
		}

		public static bool? getBooleanProperty(string name)
		{
			string value = properties.getProperty(name);
			if(bool.TRUE.ToString().Equals(value))
			{
				Logger.logMessage(name + " = \"true\"");
				return true;
			}
			else if(bool.FALSE.ToString().Equals(value))
			{
				Logger.logMessage(name + " = \"false\"");
				return false;
			}
			Logger.logMessage(name + " not defined, assuming false");
			return false;
		}

		public static Blockchain Blockchain
		{
			get
			{
				return BlockchainImpl.Instance;
			}
		}

		public static BlockchainProcessor BlockchainProcessor
		{
			get
			{
				return BlockchainProcessorImpl.Instance;
			}
		}

		public static TransactionProcessor TransactionProcessor
		{
			get
			{
				return TransactionProcessorImpl.Instance;
			}
		}

		public static int EpochTime
		{
			get
			{
				return time.Time;
			}
		}

		internal static Time Time
		{
			set
			{
				Nxt.time = value;
			}
		}

		static void Main(string[] args)
		{
			Runtime.Runtime.addShutdownHook(new Thread(new Runnable() { public void run() { Nxt.shutdown(); } }));
			init();
		}

		public static void init(Properties customProperties)
		{
			properties.putAll(customProperties);
			init();
		}

		public static void init()
		{
			Init.init();
		}

		public static void shutdown()
		{
			Logger.logShutdownMessage("Shutting down...");
			API.shutdown();
			Users.shutdown();
			Peers.shutdown();
			ThreadPool.shutdown();
			Db.shutdown();
			Logger.logShutdownMessage("Nxt server " + VERSION + " stopped.");
			Logger.shutdown();
		}

		private class Init
		{


			private static void init()
			{
			}

			private Init() // never
			{
			}

		}

		private Nxt() // never
		{
		}

	}

}