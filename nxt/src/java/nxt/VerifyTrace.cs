using System;
using System.Collections.Generic;
using System.IO;

namespace nxt
{

	using Convert = nxt.util.Convert;
	using Logger = nxt.util.Logger;


	public sealed class VerifyTrace
	{

		private static readonly IList<string> balanceHeaders = Arrays.asList("balance", "unconfirmed balance");
		private static readonly IList<string> deltaHeaders = Arrays.asList("transaction amount", "transaction fee", "generation fee", "trade cost", "purchase cost", "discount", "refund");
		private static readonly IList<string> assetQuantityHeaders = Arrays.asList("asset balance", "unconfirmed asset balance");
		private static readonly IList<string> deltaAssetQuantityHeaders = Arrays.asList("asset quantity", "trade quantity");

		private static bool isBalance(string header)
		{
			return balanceHeaders.Contains(header);
		}

		private static bool isDelta(string header)
		{
			return deltaHeaders.Contains(header);
		}

		private static bool isAssetQuantity(string header)
		{
			return assetQuantityHeaders.Contains(header);
		}

		private static bool isDeltaAssetQuantity(string header)
		{
			return deltaAssetQuantityHeaders.Contains(header);
		}

		static void Main(string[] args)
		{
			string fileName = args.Length == 1 ? args[0] : "nxt-trace.csv";
			using (BufferedReader reader = new BufferedReader(new FileReader(fileName)))
			{
				string line = reader.readLine();
				string[] headers = unquote(StringHelperClass.StringSplit(line, DebugTrace.SEPARATOR, true));

				IDictionary<string, IDictionary<string, long?>> totals = new Dictionary<>();
				IDictionary<string, IDictionary<string, IDictionary<string, long?>>> accountAssetTotals = new Dictionary<>();
				IDictionary<string, long?> issuedAssetQuantities = new Dictionary<>();
				IDictionary<string, long?> accountAssetQuantities = new Dictionary<>();

				while((line = reader.readLine()) != null)
				{
					string[] values = unquote(StringHelperClass.StringSplit(line, DebugTrace.SEPARATOR, true));
					IDictionary<string, string> valueMap = new Dictionary<>();
					for(int i = 0; i < headers.Length; i++)
					{
						valueMap.Add(headers[i], values[i]);
					}
					string accountId = valueMap["account"];
					IDictionary<string, long?> accountTotals = totals[accountId];
					if(accountTotals == null)
					{
						accountTotals = new Dictionary<>();
						totals.Add(accountId, accountTotals);
					}
					IDictionary<string, IDictionary<string, long?>> accountAssetMap = accountAssetTotals[accountId];
					if(accountAssetMap == null)
					{
						accountAssetMap = new Dictionary<>();
						accountAssetTotals.Add(accountId, accountAssetMap);
					}
					if("asset issuance".Equals(valueMap["event"]))
					{
						string assetId = valueMap["asset"];
						issuedAssetQuantities.Add(assetId, Convert.ToInt64(valueMap["asset quantity"]));
					}
//JAVA TO VB & C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'entrySet' method:
					foreach (KeyValuePair<string, string> mapEntry in valueMap.entrySet())
					{
						string header = mapEntry.Key;
						string value = mapEntry.Value;
						if(value == null || "".Equals(value.Trim()))
						{
							continue;
						}
						if(isBalance(header))
						{
							accountTotals.Add(header, Convert.ToInt64(value));
						}
						else if(isDelta(header))
						{
							long previousValue = nullToZero(accountTotals[header]);
							accountTotals.Add(header, Convert.safeAdd(previousValue, Convert.ToInt64(value)));
						}
						else if(isAssetQuantity(header))
						{
							string assetId = valueMap["asset"];
							IDictionary<string, long?> assetTotals = accountAssetMap[assetId];
							if(assetTotals == null)
							{
								assetTotals = new Dictionary<>();
								accountAssetMap.Add(assetId, assetTotals);
							}
							assetTotals.Add(header, Convert.ToInt64(value));
						}
						else if(isDeltaAssetQuantity(header))
						{
							string assetId = valueMap["asset"];
							IDictionary<string, long?> assetTotals = accountAssetMap[assetId];
							if(assetTotals == null)
							{
								assetTotals = new Dictionary<>();
								accountAssetMap.Add(assetId, assetTotals);
							}
							long previousValue = nullToZero(assetTotals[header]);
							assetTotals.Add(header, Convert.safeAdd(previousValue, Convert.ToInt64(value)));
						}
					}
				}

				Set<string> failed = new HashSet<>();
//JAVA TO VB & C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'entrySet' method:
				foreach (KeyValuePair<string, IDictionary<string, long?>> mapEntry in totals.entrySet())
				{
					string accountId = mapEntry.Key;
					IDictionary<string, long?> accountValues = mapEntry.Value;
					Console.WriteLine("account: " + accountId);
					foreach (string balanceHeader in balanceHeaders)
					{
						Console.WriteLine(balanceHeader + ": " + nullToZero(accountValues[balanceHeader]));
					}
					Console.WriteLine("totals:");
					long totalDelta = 0;
					foreach (string header in deltaHeaders)
					{
						long delta = nullToZero(accountValues[header]);
						totalDelta = Convert.safeAdd(totalDelta, delta);
						Console.WriteLine(header + ": " + delta);
					}
					Console.WriteLine("total confirmed balance change: " + totalDelta);
					long balance = nullToZero(accountValues["balance"]);
					if(balance != totalDelta)
					{
						Console.WriteLine("ERROR: balance doesn't match total change!!!");
						failed.add(accountId);
					}
					IDictionary<string, IDictionary<string, long?>> accountAssetMap = accountAssetTotals[accountId];
//JAVA TO VB & C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'entrySet' method:
					foreach (KeyValuePair<string, IDictionary<string, long?>> assetMapEntry in accountAssetMap.entrySet())
					{
						string assetId = assetMapEntry.Key;
						IDictionary<string, long?> assetValues = assetMapEntry.Value;
						Console.WriteLine("asset: " + assetId);
//JAVA TO VB & C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'entrySet' method:
						foreach (KeyValuePair<string, long?> assetValueEntry in assetValues.entrySet())
						{
							Console.WriteLine(assetValueEntry.Key + ": " + assetValueEntry.Value);
						}
						long totalAssetDelta = 0;
						foreach (string header in deltaAssetQuantityHeaders)
						{
							long delta = nullToZero(assetValues[header]);
							totalAssetDelta = Convert.safeAdd(totalAssetDelta, delta);
						}
						Console.WriteLine("total confirmed asset quantity change: " + totalAssetDelta);
						long assetBalance = assetValues["asset balance"];
						if(assetBalance != totalAssetDelta)
						{
							Console.WriteLine("ERROR: asset balance doesn't match total asset quantity change!!!");
							failed.add(accountId);
						}
						long previousAssetQuantity = nullToZero(accountAssetQuantities[assetId]);
						accountAssetQuantities.Add(assetId, Convert.safeAdd(previousAssetQuantity, assetBalance));
					}
					Console.WriteLine();
				}
				Set<string> failedAssets = new HashSet<>();
//JAVA TO VB & C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'entrySet' method:
				foreach (KeyValuePair<string, long?> assetEntry in issuedAssetQuantities.entrySet())
				{
					string assetId = assetEntry.Key;
					long issuedAssetQuantity = assetEntry.Value;
					if(issuedAssetQuantity != nullToZero(accountAssetQuantities[assetId]))
					{
						Console.WriteLine("ERROR: asset " + assetId + " balances don't match, issued: " + issuedAssetQuantity + ", total of account balances: " + accountAssetQuantities[assetId]);
						failedAssets.add(assetId);
					}
				}
				if(failed.size() > 0)
				{
					Console.WriteLine("ERROR: " + failed.size() + " accounts have incorrect balances");
					Console.WriteLine(failed);
				}
				else
				{
					Console.WriteLine("SUCCESS: all " + totals.Count + " account balances and asset balances match the transaction and trade totals!");
				}
				if(failedAssets.size() > 0)
				{
					Console.WriteLine("ERROR: " + failedAssets.size() + " assets have incorrect balances");
					Console.WriteLine(failedAssets);
				}
				else
				{
					Console.WriteLine("SUCCESS: all " + issuedAssetQuantities.Count + " assets quantities are correct!");
				}

			}
			catch(IOException e)
			{
				Console.WriteLine(e.ToString());
				throw new Exception(e);
			}
		}

		static VerifyTrace()
		{
			Logger.init();
		}

		private const string beginQuote = "^" + DebugTrace.QUOTE;
		private const string endQuote = DebugTrace.QUOTE + "$";

		private static string[] unquote(string[] values)
		{
			string[] result = new string[values.Length];
			for(int i = 0; i < values.Length; i++)
			{
				result[i] = values[i].replaceFirst(beginQuote, "").replaceFirst(endQuote, "");
			}
			return result;
		}

		private static long nullToZero(long? l)
		{
			return l == null ? 0 : l;
		}

	}

}