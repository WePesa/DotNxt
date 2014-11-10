using System;
using System.Collections.Generic;
using System.Text;

namespace nxt.peer
{

	using Account = nxt.Account;
	using Block = nxt.Block;
	using BlockchainProcessor = nxt.BlockchainProcessor;
	using Constants = nxt.Constants;
	using Nxt = nxt.Nxt;
	using NxtException = nxt.NxtException;
	using Convert = nxt.util.Convert;
	using CountingInputStream = nxt.util.CountingInputStream;
	using CountingOutputStream = nxt.util.CountingOutputStream;
	using Listener = nxt.util.Listener;
	using Logger = nxt.util.Logger;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;
	using JSONValue = org.json.simple.JSONValue;


	internal sealed class PeerImpl : Peer
	{

		private static readonly ConcurrentMap<long?, long?> hallmarkBalances = new ConcurrentHashMap<>();

		static PeerImpl()
		{
			Nxt.BlockchainProcessor.addListener(new Listener<Block>() { public void notify(Block block) { hallmarkBalances.clear(); } }, BlockchainProcessor.Event.AFTER_BLOCK_APPLY);
		}

		private readonly string peerAddress;
		private volatile string announcedAddress;
		private volatile int port;
		private volatile bool shareAddress;
		private volatile Hallmark hallmark;
		private volatile string platform;
		private volatile string application;
		private volatile string version;
		private volatile long adjustedWeight;
		private volatile long blacklistingTime;
		private volatile State state;
		private volatile long downloadedVolume;
		private volatile long uploadedVolume;
		private volatile int lastUpdated;

		internal PeerImpl(string peerAddress, string announcedAddress)
		{
			this.peerAddress = peerAddress;
			this.announcedAddress = announcedAddress;
			try
			{
				this.port = new URL("http://" + announcedAddress).Port;
			}
			catch(MalformedURLException ignore)
			{
			}
			this.state = State.NON_CONNECTED;
			this.shareAddress = true;
		}

		public override string PeerAddress
		{
			get
			{
				return peerAddress;
			}
		}

		public override State State
		{
			get
			{
				return state;
			}
			set
			{
				if(this.state == value)
				{
					return;
				}
				if(this.state == State.NON_CONNECTED)
				{
					this.state = value;
					Peers.notifyListeners(this, Peers.Event.ADDED_ACTIVE_PEER);
				}
				else if(value != State.NON_CONNECTED)
				{
					this.state = value;
					Peers.notifyListeners(this, Peers.Event.CHANGED_ACTIVE_PEER);
				}
			}
		}


		public override long DownloadedVolume
		{
			get
			{
				return downloadedVolume;
			}
		}

		internal void updateDownloadedVolume(long volume)
		{
			lock (this)
			{
				downloadedVolume += volume;
			}
			Peers.notifyListeners(this, Peers.Event.DOWNLOADED_VOLUME);
		}

		public override long UploadedVolume
		{
			get
			{
				return uploadedVolume;
			}
		}

		internal void updateUploadedVolume(long volume)
		{
			lock (this)
			{
				uploadedVolume += volume;
			}
			Peers.notifyListeners(this, Peers.Event.UPLOADED_VOLUME);
		}

		public override string Version
		{
			get
			{
				return version;
			}
			set
			{
				this.version = value;
			}
		}


		public override string Application
		{
			get
			{
				return application;
			}
			set
			{
				this.application = value;
			}
		}


		public override string Platform
		{
			get
			{
				return platform;
			}
			set
			{
				this.platform = value;
			}
		}


		public override string Software
		{
			get
			{
				return Convert.truncate(application, "?", 10, false) + " (" + Convert.truncate(version, "?", 10, false) + ")" + " @ " + Convert.truncate(platform, "?", 10, false);
			}
		}

		public override bool shareAddress()
		{
			return shareAddress;
		}

		internal bool ShareAddress
		{
			set
			{
				this.shareAddress = value;
			}
		}

		public override string AnnouncedAddress
		{
			get
			{
				return announcedAddress;
			}
			set
			{
				string announcedPeerAddress = Peers.normalizeHostAndPort(value);
				if(announcedPeerAddress != null)
				{
					this.announcedAddress = announcedPeerAddress;
					try
					{
						this.port = new URL("http://" + announcedPeerAddress).Port;
					}
					catch(MalformedURLException ignore)
					{
					}
				}
			}
		}


		internal int Port
		{
			get
			{
				return port;
			}
		}

		public override bool isWellKnown()
		{
			get
			{
				return announcedAddress != null && Peers.wellKnownPeers.contains(announcedAddress);
			}
		}

		public override Hallmark Hallmark
		{
			get
			{
				return hallmark;
			}
		}

		public override int Weight
		{
			get
			{
				if(hallmark == null)
				{
					return 0;
				}
				long accountId = hallmark.AccountId;
				long? hallmarkBalance = hallmarkBalances.get(accountId);
				if(hallmarkBalance == null)
				{
					Account account = Account.getAccount(accountId);
					hallmarkBalance = account == null ? 0 : account.BalanceNQT;
					hallmarkBalances.put(accountId, hallmarkBalance);
				}
				return(int)(adjustedWeight * (hallmarkBalance / Constants.ONE_NXT) / Constants.MAX_BALANCE_NXT);
			}
		}

		public override bool isBlacklisted()
		{
			get
			{
				return blacklistingTime > 0 || Peers.knownBlacklistedPeers.contains(peerAddress);
			}
		}

		public override void blacklist(Exception cause)
		{
			if(cause is NxtException.NotCurrentlyValidException || cause is BlockchainProcessor.BlockOutOfOrderException)
			{
			// don't blacklist peers just because a feature is not yet enabled
			// prevents erroneous blacklisting during loading of blockchain from scratch
				return;
			}
			if(! Blacklisted && ! (cause is IOException))
			{
				Logger.logDebugMessage("Blacklisting " + peerAddress + " because of: " + cause.ToString(), cause);
			}
			blacklist();
		}

		public override void blacklist()
		{
			blacklistingTime = System.currentTimeMillis();
			State = State.NON_CONNECTED;
			Peers.notifyListeners(this, Peers.Event.BLACKLIST);
		}

		public override void unBlacklist()
		{
			State = State.NON_CONNECTED;
			blacklistingTime = 0;
			Peers.notifyListeners(this, Peers.Event.UNBLACKLIST);
		}

		internal void updateBlacklistedStatus(long curTime)
		{
			if(blacklistingTime > 0 && blacklistingTime + Peers.blacklistingPeriod <= curTime)
			{
				unBlacklist();
			}
		}

		public override void deactivate()
		{
			State = State.NON_CONNECTED;
			Peers.notifyListeners(this, Peers.Event.DEACTIVATE);
		}

		public override void remove()
		{
			Peers.removePeer(this);
			Peers.notifyListeners(this, Peers.Event.REMOVE);
		}

		public override int LastUpdated
		{
			get
			{
				return lastUpdated;
			}
			set
			{
				this.lastUpdated = value;
			}
		}

		public override JSONObject send(JSONStreamAware request)
		{

			JSONObject response;

			string log = null;
			bool showLog = false;
			HttpURLConnection connection = null;

			try
			{

				string address = announcedAddress != null ? announcedAddress : peerAddress;
				StringBuilder buf = new StringBuilder("http://");
				buf.Append(address);
				if(port <= 0)
				{
					buf.Append(':');
					buf.Append(Constants.isTestnet ? Peers.TESTNET_PEER_PORT : Peers.DEFAULT_PEER_PORT);
				}
				buf.Append("/nxt");
				URL url = new URL(buf.ToString());

				if(Peers.communicationLoggingMask != 0)
				{
					StringWriter stringWriter = new StringWriter();
					request.writeJSONString(stringWriter);
					log = "\"" + url.ToString() + "\": " + stringWriter.ToString();
				}

				connection = (HttpURLConnection)url.openConnection();
				connection.RequestMethod = "POST";
				connection.DoOutput = true;
				connection.ConnectTimeout = Peers.connectTimeout;
				connection.ReadTimeout = Peers.readTimeout;
				connection.setRequestProperty("Accept-Encoding", "gzip");

				CountingOutputStream cos = new CountingOutputStream(connection.OutputStream);
				using (Writer writer = new BufferedWriter(new OutputStreamWriter(cos, "UTF-8")))
				{
					request.writeJSONString(writer);
				}
				updateUploadedVolume(cos.Count);

				if(connection.ResponseCode == HttpURLConnection.HTTP_OK)
				{
					CountingInputStream cis = new CountingInputStream(connection.InputStream);
					InputStream responseStream = cis;
					if("gzip".Equals(connection.getHeaderField("Content-Encoding")))
					{
						responseStream = new GZIPInputStream(cis);
					}
					if((Peers.communicationLoggingMask & Peers.LOGGING_MASK_200_RESPONSES) != 0)
					{
						ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
						sbyte[] buffer = new sbyte[1024];
						int numberOfBytes;
						using (InputStream inputStream = responseStream)
						{
							while((numberOfBytes = inputStream.read(buffer, 0, buffer.Length)) > 0)
							{
								byteArrayOutputStream.write(buffer, 0, numberOfBytes);
							}
						}
						string responseValue = byteArrayOutputStream.ToString("UTF-8");
						if(responseValue.Length > 0 && responseStream is GZIPInputStream)
						{
							log += string.Format("[length: {0:D}, compression ratio: {1:F2}]", cis.Count, (double)cis.Count / (double)responseValue.Length);
						}
						log += " >>> " + responseValue;
						showLog = true;
						response = (JSONObject) JSONValue.parse(responseValue);
					}
					else
					{
						using (Reader reader = new BufferedReader(new InputStreamReader(responseStream, "UTF-8")))
						{
							response = (JSONObject)JSONValue.parse(reader);
						}
					}
					updateDownloadedVolume(cis.Count);
				}
				else
				{

					if((Peers.communicationLoggingMask & Peers.LOGGING_MASK_NON200_RESPONSES) != 0)
					{
						log += " >>> Peer responded with HTTP " + connection.ResponseCode + " code!";
						showLog = true;
					}
					if(state == State.CONNECTED)
					{
						State = State.DISCONNECTED;
					}
					else
					{
						State = State.NON_CONNECTED;
					}
					response = null;

				}

			}
			catch(Exception|IOException e)
			{
				if(! (e is UnknownHostException || e is SocketTimeoutException || e is SocketException))
				{
					Logger.logDebugMessage("Error sending JSON request", e);
				}
				if((Peers.communicationLoggingMask & Peers.LOGGING_MASK_EXCEPTIONS) != 0)
				{
					log += " >>> " + e.ToString();
					showLog = true;
				}
				if(state == State.CONNECTED)
				{
					State = State.DISCONNECTED;
				}
				response = null;
			}

			if(showLog)
			{
				Logger.logMessage(log + "\n");
			}

			if(connection != null)
			{
				connection.disconnect();
			}

			return response;

		}

		public override int compareTo(Peer o)
		{
			if(Weight > o.Weight)
			{
				return -1;
			}
			else if(Weight < o.Weight)
			{
				return 1;
			}
			return 0;
		}

		internal void connect()
		{
			JSONObject response = send(Peers.myPeerInfoRequest);
			if(response != null)
			{
				application = (string)response.get("application");
				version = (string)response.get("version");
				platform = (string)response.get("platform");
				shareAddress = bool.TRUE.Equals(response.get("shareAddress"));
				string newAnnouncedAddress = Convert.emptyToNull((string)response.get("announcedAddress"));
				if(newAnnouncedAddress != null && ! newAnnouncedAddress.Equals(announcedAddress))
				{
				// force verification of changed announced address
					State = Peer.State.NON_CONNECTED;
					AnnouncedAddress = newAnnouncedAddress;
					return;
				}
				if(announcedAddress == null)
				{
					AnnouncedAddress = peerAddress;
				//Logger.logDebugMessage("Connected to peer without announced address, setting to " + peerAddress);
				}
				if(analyzeHallmark(announcedAddress, (string)response.get("hallmark")))
				{
					State = State.CONNECTED;
					Peers.updateAddress(this);
				}
				else
				{
					blacklist();
				}
				lastUpdated = Nxt.EpochTime;
			}
			else
			{
				State = State.NON_CONNECTED;
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: boolean analyzeHallmark(String address, final String hallmarkString)
		internal bool analyzeHallmark(string address, string hallmarkString)
		{

			if(hallmarkString == null && this.hallmark == null)
			{
				return true;
			}

			if(this.hallmark != null && this.hallmark.HallmarkString.Equals(hallmarkString))
			{
				return true;
			}

			if(hallmarkString == null)
			{
				this.hallmark = null;
				return true;
			}

			try
			{
				URI uri = new URI("http://" + address.Trim());
				string host = uri.Host;

				Hallmark hallmark = Hallmark.parseHallmark(hallmarkString);
				if(!hallmark.Valid || !(hallmark.Host.Equals(host) || InetAddress.getByName(host).Equals(InetAddress.getByName(hallmark.Host))))
				{
				//Logger.logDebugMessage("Invalid hallmark for " + host + ", hallmark host is " + hallmark.getHost());
					return false;
				}
				this.hallmark = hallmark;
				long accountId = Account.getId(hallmark.PublicKey);
				IList<PeerImpl> groupedPeers = new List<>();
				int mostRecentDate = 0;
				long totalWeight = 0;
				foreach (PeerImpl peer in Peers.allPeers)
				{
					if(peer.hallmark == null)
					{
						continue;
					}
					if(accountId == peer.hallmark.AccountId)
					{
						groupedPeers.Add(peer);
						if(peer.hallmark.Date > mostRecentDate)
						{
							mostRecentDate = peer.hallmark.Date;
							totalWeight = peer.getHallmarkWeight(mostRecentDate);
						}
						else
						{
							totalWeight += peer.getHallmarkWeight(mostRecentDate);
						}
					}
				}

				foreach (PeerImpl peer in groupedPeers)
				{
					peer.adjustedWeight = Constants.MAX_BALANCE_NXT * peer.getHallmarkWeight(mostRecentDate) / totalWeight;
					Peers.notifyListeners(peer, Peers.Event.WEIGHT);
				}

				return true;

			}
			catch(UnknownHostException ignore)
			{
			}
			catch(URISyntaxException | Exception e)
			{
				Logger.logDebugMessage("Failed to analyze hallmark for peer " + address + ", " + e.ToString(), e);
			}
			return false;

		}

		private int getHallmarkWeight(int date)
		{
			if(hallmark == null || ! hallmark.Valid || hallmark.Date != date)
			{
				return 0;
			}
			return hallmark.Weight;
		}

	}

}