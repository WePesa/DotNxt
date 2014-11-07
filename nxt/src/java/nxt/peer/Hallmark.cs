using System;

namespace nxt.peer
{

	using Account = nxt.Account;
	using Constants = nxt.Constants;
	using Crypto = nxt.crypto.Crypto;
	using Convert = nxt.util.Convert;


	public sealed class Hallmark
	{

		public static int parseDate(string dateValue)
		{
			return Convert.ToInt32(dateValue.Substring(0, 4)) * 10000 + Convert.ToInt32(dateValue.Substring(5, 2)) * 100 + Convert.ToInt32(dateValue.Substring(8, 2));
		}

		public static string formatDate(int date)
		{
			int year = date / 10000;
			int month = (date % 10000) / 100;
			int day = date % 100;
			return(year < 10 ? "000" : (year < 100 ? "00" : (year < 1000 ? "0" : ""))) + year + "-" + (month < 10 ? "0" : "") + month + "-" + (day < 10 ? "0" : "") + day;
		}

		public static string generateHallmark(string secretPhrase, string host, int weight, int date)
		{

			if(host.Length == 0 || host.Length > 100)
			{
				throw new System.ArgumentException("Hostname length should be between 1 and 100");
			}
			if(weight <= 0 || weight > Constants.MAX_BALANCE_NXT)
			{
				throw new System.ArgumentException("Weight should be between 1 and " + Constants.MAX_BALANCE_NXT);
			}

			sbyte[] publicKey = Crypto.getPublicKey(secretPhrase);
			sbyte[] hostBytes = Convert.toBytes(host);

			ByteBuffer buffer = ByteBuffer.allocate(32 + 2 + hostBytes.Length + 4 + 4 + 1);
			buffer.order(ByteOrder.LITTLE_ENDIAN);
			buffer.put(publicKey);
			buffer.putShort((short)hostBytes.Length);
			buffer.put(hostBytes);
			buffer.putInt(weight);
			buffer.putInt(date);

			sbyte[] data = buffer.array();
			data[data.Length - 1] = (sbyte) ThreadLocalRandom.current().Next();
			sbyte[] signature = Crypto.sign(data, secretPhrase);

			return Convert.toHexString(data) + Convert.toHexString(signature);

		}

		public static Hallmark parseHallmark(string hallmarkString)
		{

			sbyte[] hallmarkBytes = Convert.parseHexString(hallmarkString);

			ByteBuffer buffer = ByteBuffer.wrap(hallmarkBytes);
			buffer.order(ByteOrder.LITTLE_ENDIAN);

			sbyte[] publicKey = new sbyte[32];
			buffer.get(publicKey);
			int hostLength = buffer.Short;
			if(hostLength > 300)
			{
				throw new System.ArgumentException("Invalid host length");
			}
			sbyte[] hostBytes = new sbyte[hostLength];
			buffer.get(hostBytes);
			string host = Convert.ToString(hostBytes);
			int weight = buffer.Int;
			int date = buffer.Int;
			buffer.get();
			sbyte[] signature = new sbyte[64];
			buffer.get(signature);

			sbyte[] data = new sbyte[hallmarkBytes.Length - 64];
			Array.Copy(hallmarkBytes, 0, data, 0, data.Length);

			bool isValid = host.Length < 100 && weight > 0 && weight <= Constants.MAX_BALANCE_NXT && Crypto.verify(signature, data, publicKey, true);

			return new Hallmark(hallmarkString, publicKey, signature, host, weight, date, isValid);

		}

		private readonly string hallmarkString;
		private readonly string host;
		private readonly int weight;
		private readonly int date;
		private readonly sbyte[] publicKey;
		private readonly long accountId;
		private readonly sbyte[] signature;
		private readonly bool isValid;

		private Hallmark(string hallmarkString, sbyte[] publicKey, sbyte[] signature, string host, int weight, int date, bool isValid)
		{
			this.hallmarkString = hallmarkString;
			this.host = host;
			this.publicKey = publicKey;
			this.accountId = Account.getId(publicKey);
			this.signature = signature;
			this.weight = weight;
			this.date = date;
			this.isValid = isValid;
		}

		public string HallmarkString
		{
			get
			{
				return hallmarkString;
			}
		}

		public string Host
		{
			get
			{
				return host;
			}
		}

		public int Weight
		{
			get
			{
				return weight;
			}
		}

		public int Date
		{
			get
			{
				return date;
			}
		}

		public sbyte[] Signature
		{
			get
			{
				return signature;
			}
		}

		public sbyte[] PublicKey
		{
			get
			{
				return publicKey;
			}
		}

		public long AccountId
		{
			get
			{
				return accountId;
			}
		}

		public bool isValid()
		{
			get
			{
				return isValid;
			}
		}

	}

}