using System;

namespace nxt.peer
{

	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	public interface Peer : Comparable<Peer>
	{

//JAVA TO VB & C# CONVERTER TODO TASK: Interfaces cannot contain types in .NET:
//		public static enum State
//	{
//		NON_CONNECTED, CONNECTED, DISCONNECTED
//	}

		string PeerAddress {get;}

		string AnnouncedAddress {get;}

		State State {get;}

		string Version {get;}

		string Application {get;}

		string Platform {get;}

		string Software {get;}

		Hallmark Hallmark {get;}

		int Weight {get;}

		bool shareAddress();

		bool isWellKnown() {get;}

		bool isBlacklisted() {get;}

		void blacklist(Exception cause);

		void blacklist();

		void unBlacklist();

		void deactivate();

		void remove();

		long DownloadedVolume {get;}

		long UploadedVolume {get;}

		int LastUpdated {get;}

		JSONObject send(JSONStreamAware request);

	}

}