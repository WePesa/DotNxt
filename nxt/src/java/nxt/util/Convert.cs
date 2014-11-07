using System;

namespace nxt.util
{

	using Constants = nxt.Constants;
	using NxtException = nxt.NxtException;
	using Crypto = nxt.crypto.Crypto;


	public sealed class Convert
	{

		private static readonly char[] hexChars = { '0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f' };
		private static readonly long[] multipliers = {1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000};

		public static readonly BigInteger two64 = new BigInteger("18446744073709551616");

		private Convert() //never
		{
		}

		public static sbyte[] parseHexString(string hex)
		{
			if(hex == null)
			{
				return null;
			}
			sbyte[] bytes = new sbyte[hex.Length / 2];
			for(int i = 0; i < bytes.Length; i++)
			{
				int char1 = hex[i * 2];
				char1 = char1 > 0x60 ? char1 - 0x57 : char1 - 0x30;
				int char2 = hex[i * 2 + 1];
				char2 = char2 > 0x60 ? char2 - 0x57 : char2 - 0x30;
				if(char1 < 0 || char2 < 0 || char1 > 15 || char2 > 15)
				{
					throw new NumberFormatException("Invalid hex number: " + hex);
				}
				bytes[i] = (sbyte)((char1 << 4) + char2);
			}
			return bytes;
		}

		public static string toHexString(sbyte[] bytes)
		{
			if(bytes == null)
			{
				return null;
			}
			char[] chars = new char[bytes.Length * 2];
			for(int i = 0; i < bytes.Length; i++)
			{
				chars[i * 2] = hexChars[((bytes[i] >> 4) & 0xF)];
				chars[i * 2 + 1] = hexChars[(bytes[i] & 0xF)];
			}
			return Convert.ToString(chars);
		}

		public static string toUnsignedLong(long objectId)
		{
			if(objectId >= 0)
			{
				return Convert.ToString(objectId);
			}
			BigInteger id = BigInteger.valueOf(objectId).add(two64);
			return id.ToString();
		}

		public static long parseUnsignedLong(string number)
		{
			if(number == null)
			{
				return 0;
			}
			BigInteger bigInt = new BigInteger(number.Trim());
			if(bigInt.signum() < 0 || bigInt.CompareTo(two64) != -1)
			{
				throw new System.ArgumentException("overflow: " + number);
			}
			return (long)bigInt;
		}

		public static long parseLong(object o)
		{
			if(o == null)
			{
				return 0;
			}
			else if(o is long?)
			{
				return((long?)o);
			}
			else if(o is string)
			{
				return Convert.ToInt64((string)o);
			}
			else
			{
				throw new System.ArgumentException("Not a long: " + o);
			}
		}

		public static long parseAccountId(string account)
		{
			if(account == null)
			{
				return 0;
			}
			account = account.ToUpper();
			if(account.StartsWith("NXT-"))
			{
				return Crypto.rsDecode(account.Substring(4));
			}
			else
			{
				return parseUnsignedLong(account);
			}
		}

		public static string rsAccount(long accountId)
		{
			return "NXT-" + Crypto.rsEncode(accountId);
		}

		public static long fullHashToId(sbyte[] hash)
		{
			if(hash == null || hash.Length < 8)
			{
				throw new System.ArgumentException("Invalid hash: " + Arrays.ToString(hash));
			}
			BigInteger bigInteger = new BigInteger(1, new sbyte[] {hash[7], hash[6], hash[5], hash[4], hash[3], hash[2], hash[1], hash[0]});
			return (long)bigInteger;
		}

		public static long fullHashToId(string hash)
		{
			if(hash == null)
			{
				return 0;
			}
			return fullHashToId(Convert.parseHexString(hash));
		}

		public static DateTime fromEpochTime(int epochTime)
		{
			return new DateTime(epochTime * 1000L + Constants.EPOCH_BEGINNING - 500L);
		}

		public static string emptyToNull(string s)
		{
			return s == null || s.Length == 0 ? null : s;
		}

		public static string nullToEmpty(string s)
		{
			return s == null ? "" : s;
		}

		public static sbyte[] emptyToNull(sbyte[] bytes)
		{
			if(bytes == null)
			{
				return null;
			}
			foreach (sbyte b in bytes)
			{
				if(b != 0)
				{
					return bytes;
				}
			}
			return null;
		}

		public static sbyte[] toBytes(string s)
		{
			try
			{
				return s.getBytes("UTF-8");
			}
			catch(UnsupportedEncodingException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public static string ToString(sbyte[] bytes)
		{
			try
			{
				return new string(bytes, "UTF-8").Trim();
			}
			catch(UnsupportedEncodingException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String readString(ByteBuffer buffer, int numBytes, int maxLength) throws NxtException.NotValidException
		public static string readString(ByteBuffer buffer, int numBytes, int maxLength)
		{
			if(numBytes > 3 * maxLength)
			{
				throw new NxtException.NotValidException("Max parameter length exceeded");
			}
			sbyte[] bytes = new sbyte[numBytes];
			buffer.get(bytes);
			return Convert.ToString(bytes);
		}

		public static string truncate(string s, string replaceNull, int limit, bool dots)
		{
			return s == null ? replaceNull : s.Length > limit ? (s.Substring(0, dots ? limit - 3 : limit) + (dots ? "..." : "")) : s;
		}

		public static long parseNXT(string nxt)
		{
			return parseStringFraction(nxt, 8, Constants.MAX_BALANCE_NXT);
		}

		private static long parseStringFraction(string value, int decimals, long maxValue)
		{
			string[] s = value.Trim().Split("\\.");
			if(s.Length == 0 || s.Length > 2)
			{
				throw new NumberFormatException("Invalid number: " + value);
			}
			long wholePart = Convert.ToInt64(s[0]);
			if(wholePart > maxValue)
			{
				throw new System.ArgumentException("Whole part of value exceeds maximum possible");
			}
			if(s.Length == 1)
			{
				return wholePart * multipliers[decimals];
			}
			long fractionalPart = Convert.ToInt64(s[1]);
			if(fractionalPart >= multipliers[decimals] || s[1].Length > decimals)
			{
				throw new System.ArgumentException("Fractional part exceeds maximum allowed divisibility");
			}
			for(int i = s[1].Length; i < decimals; i++)
			{
				fractionalPart *= 10;
			}
			return wholePart * multipliers[decimals] + fractionalPart;
		}

	// overflow checking based on https://www.securecoding.cert.org/confluence/display/java/NUM00-J.+Detect+or+prevent+integer+overflow
//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static long safeAdd(long left, long right) throws ArithmeticException
		public static long safeAdd(long left, long right)
		{
			if(right > 0 ? left > long.MaxValue - right : left < long.MinValue - right)
			{
				throw new ArithmeticException("Integer overflow");
			}
			return left + right;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static long safeSubtract(long left, long right) throws ArithmeticException
		public static long safeSubtract(long left, long right)
		{
			if(right > 0 ? left < long.MinValue + right : left > long.MaxValue + right)
			{
				throw new ArithmeticException("Integer overflow");
			}
			return left - right;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static long safeMultiply(long left, long right) throws ArithmeticException
		public static long safeMultiply(long left, long right)
		{
			if(right > 0 ? left > long.MaxValue/right || left < long.MinValue/right : (right < -1 ? left > long.MinValue/right || left < long.MaxValue/right : right == -1 && left == long.MinValue))
			{
				throw new ArithmeticException("Integer overflow");
			}
			return left * right;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static long safeDivide(long left, long right) throws ArithmeticException
		public static long safeDivide(long left, long right)
		{
			if((left == long.MinValue) && (right == -1))
			{
				throw new ArithmeticException("Integer overflow");
			}
			return left / right;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static long safeNegate(long a) throws ArithmeticException
		public static long safeNegate(long a)
		{
			if(a == long.MinValue)
			{
				throw new ArithmeticException("Integer overflow");
			}
			return -a;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static long safeAbs(long a) throws ArithmeticException
		public static long safeAbs(long a)
		{
			if(a == long.MinValue)
			{
				throw new ArithmeticException("Integer overflow");
			}
			return Math.Abs(a);
		}

	}

}