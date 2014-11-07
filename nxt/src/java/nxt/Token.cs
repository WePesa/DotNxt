using System;
using System.Text;

namespace nxt
{

	using Crypto = nxt.crypto.Crypto;
	using Convert = nxt.util.Convert;

	public sealed class Token
	{

		public static string generateToken(string secretPhrase, string websiteString)
		{

			sbyte[] website = Convert.toBytes(websiteString);
			sbyte[] data = new sbyte[website.Length + 32 + 4];
			Array.Copy(website, 0, data, 0, website.Length);
			Array.Copy(Crypto.getPublicKey(secretPhrase), 0, data, website.Length, 32);
			int timestamp = Nxt.EpochTime;
			data[website.Length + 32] = (sbyte)timestamp;
			data[website.Length + 32 + 1] = (sbyte)(timestamp >> 8);
			data[website.Length + 32 + 2] = (sbyte)(timestamp >> 16);
			data[website.Length + 32 + 3] = (sbyte)(timestamp >> 24);

			sbyte[] token = new sbyte[100];
			Array.Copy(data, website.Length, token, 0, 32 + 4);
			Array.Copy(Crypto.sign(data, secretPhrase), 0, token, 32 + 4, 64);

			StringBuilder buf = new StringBuilder();
			for(int ptr = 0; ptr < 100; ptr += 5)
			{

				long number = ((long)(token[ptr] & 0xFF)) | (((long)(token[ptr + 1] & 0xFF)) << 8) | (((long)(token[ptr + 2] & 0xFF)) << 16) | (((long)(token[ptr + 3] & 0xFF)) << 24) | (((long)(token[ptr + 4] & 0xFF)) << 32);

				if(number < 32)
				{
					buf.Append("0000000");
				}
				else if(number < 1024)
				{
					buf.Append("000000");
				}
				else if(number < 32768)
				{
					buf.Append("00000");
				}
				else if(number < 1048576)
				{
					buf.Append("0000");
				}
				else if(number < 33554432)
				{
					buf.Append("000");
				}
				else if(number < 1073741824)
				{
					buf.Append("00");
				}
				else if(number < 34359738368L)
				{
					buf.Append("0");
				}
				buf.Append(long.ToString(number, 32));

			}

			return buf.ToString();

		}

		public static Token parseToken(string tokenString, string website)
		{

			sbyte[] websiteBytes = Convert.toBytes(website);
			sbyte[] tokenBytes = new sbyte[100];
			int i = 0, j = 0;

			for(; i < tokenString.Length; i += 8, j += 5)
			{

				long number = Convert.ToInt64(tokenString.Substring(i, 8), 32);
				tokenBytes[j] = (sbyte)number;
				tokenBytes[j + 1] = (sbyte)(number >> 8);
				tokenBytes[j + 2] = (sbyte)(number >> 16);
				tokenBytes[j + 3] = (sbyte)(number >> 24);
				tokenBytes[j + 4] = (sbyte)(number >> 32);

			}

			if(i != 160)
			{
				throw new System.ArgumentException("Invalid token string: " + tokenString);
			}
			sbyte[] publicKey = new sbyte[32];
			Array.Copy(tokenBytes, 0, publicKey, 0, 32);
			int timestamp = (tokenBytes[32] & 0xFF) | ((tokenBytes[33] & 0xFF) << 8) | ((tokenBytes[34] & 0xFF) << 16) | ((tokenBytes[35] & 0xFF) << 24);
			sbyte[] signature = new sbyte[64];
			Array.Copy(tokenBytes, 36, signature, 0, 64);

			sbyte[] data = new sbyte[websiteBytes.Length + 36];
			Array.Copy(websiteBytes, 0, data, 0, websiteBytes.Length);
			Array.Copy(tokenBytes, 0, data, websiteBytes.Length, 36);
			bool isValid = Crypto.verify(signature, data, publicKey, true);

			return new Token(publicKey, timestamp, isValid);

		}

		private readonly sbyte[] publicKey;
		private readonly int timestamp;
		private readonly bool isValid;

		private Token(sbyte[] publicKey, int timestamp, bool isValid)
		{
			this.publicKey = publicKey;
			this.timestamp = timestamp;
			this.isValid = isValid;
		}

		public sbyte[] PublicKey
		{
			get
			{
				return publicKey;
			}
		}

		public int Timestamp
		{
			get
			{
				return timestamp;
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