using System;

namespace nxt
{


	public sealed class Constants
	{

		public const int BLOCK_HEADER_LENGTH = 232;
		public const int MAX_NUMBER_OF_TRANSACTIONS = 255;
		public const int MAX_PAYLOAD_LENGTH = MAX_NUMBER_OF_TRANSACTIONS * 176;
		public const long MAX_BALANCE_NXT = 1000000000;
		public const long ONE_NXT = 100000000;
		public const long MAX_BALANCE_NQT = MAX_BALANCE_NXT * ONE_NXT;
		public const long INITIAL_BASE_TARGET = 153722867;
		public const long MAX_BASE_TARGET = MAX_BALANCE_NXT * INITIAL_BASE_TARGET;
		public const int MAX_ROLLBACK = Nxt.getIntProperty("nxt.maxRollback");
		static Constants()
		{
			if(MAX_ROLLBACK < 1440)
			{
				throw new Exception("nxt.maxRollback must be at least 1440");
			}
			Calendar calendar = Calendar.getInstance(TimeZone.getTimeZone("UTC"));
			calendar.set(Calendar.YEAR, 2013);
			calendar.set(Calendar.MONTH, Calendar.NOVEMBER);
			calendar.set(Calendar.DAY_OF_MONTH, 24);
			calendar.set(Calendar.HOUR_OF_DAY, 12);
			calendar.set(Calendar.MINUTE, 0);
			calendar.set(Calendar.SECOND, 0);
			calendar.set(Calendar.MILLISECOND, 0);
			EPOCH_BEGINNING = calendar.TimeInMillis;
		}

		public const int MAX_ALIAS_URI_LENGTH = 1000;
		public const int MAX_ALIAS_LENGTH = 100;

		public const int MAX_ARBITRARY_MESSAGE_LENGTH = 1000;
		public const int MAX_ENCRYPTED_MESSAGE_LENGTH = 1000;

		public const int MAX_ACCOUNT_NAME_LENGTH = 100;
		public const int MAX_ACCOUNT_DESCRIPTION_LENGTH = 1000;

		public const long MAX_ASSET_QUANTITY_QNT = 1000000000L * 100000000L;
		public const int MIN_ASSET_NAME_LENGTH = 3;
		public const int MAX_ASSET_NAME_LENGTH = 10;
		public const int MAX_ASSET_DESCRIPTION_LENGTH = 1000;
		public const int MAX_ASSET_TRANSFER_COMMENT_LENGTH = 1000;

		public const int MAX_POLL_NAME_LENGTH = 100;
		public const int MAX_POLL_DESCRIPTION_LENGTH = 1000;
		public const int MAX_POLL_OPTION_LENGTH = 100;
		public const int MAX_POLL_OPTION_COUNT = 100;

		public const int MAX_DGS_LISTING_QUANTITY = 1000000000;
		public const int MAX_DGS_LISTING_NAME_LENGTH = 100;
		public const int MAX_DGS_LISTING_DESCRIPTION_LENGTH = 1000;
		public const int MAX_DGS_LISTING_TAGS_LENGTH = 100;
		public const int MAX_DGS_GOODS_LENGTH = 10240;

		public const int MAX_HUB_ANNOUNCEMENT_URIS = 100;
		public const int MAX_HUB_ANNOUNCEMENT_URI_LENGTH = 1000;
		public const long MIN_HUB_EFFECTIVE_BALANCE = 100000;

		public const bool isTestnet = Nxt.getBooleanProperty("nxt.isTestnet");
		public const bool isOffline = Nxt.getBooleanProperty("nxt.isOffline");

		public const int ALIAS_SYSTEM_BLOCK = 22000;
		public const int TRANSPARENT_FORGING_BLOCK = 30000;
		public const int ARBITRARY_MESSAGES_BLOCK = 40000;
		public const int TRANSPARENT_FORGING_BLOCK_2 = 47000;
		public const int TRANSPARENT_FORGING_BLOCK_3 = 51000;
		public const int TRANSPARENT_FORGING_BLOCK_4 = 64000;
		public const int TRANSPARENT_FORGING_BLOCK_5 = 67000;
		public const int TRANSPARENT_FORGING_BLOCK_6 = isTestnet ? 75000 : 130000;
		public const int TRANSPARENT_FORGING_BLOCK_7 = int.MAX_VALUE;
		public const int TRANSPARENT_FORGING_BLOCK_8 = isTestnet ? 78000 : 215000;
		public const int NQT_BLOCK = isTestnet ? 76500 : 132000;
		public const int FRACTIONAL_BLOCK = isTestnet ? NQT_BLOCK : 134000;
		public const int ASSET_EXCHANGE_BLOCK = isTestnet ? NQT_BLOCK : 135000;
		public const int REFERENCED_TRANSACTION_FULL_HASH_BLOCK = isTestnet ? NQT_BLOCK : 140000;
		public const int REFERENCED_TRANSACTION_FULL_HASH_BLOCK_TIMESTAMP = isTestnet ? 13031352 : 15134204;
		public const int VOTING_SYSTEM_BLOCK = int.MAX_VALUE;
		public const int DIGITAL_GOODS_STORE_BLOCK = isTestnet ? 77341 : 213000;
		public const int PUBLIC_KEY_ANNOUNCEMENT_BLOCK = isTestnet ? 77341 : 215000;
		public const int LAST_KNOWN_BLOCK = isTestnet ? 80000 : 271000;

		internal const long UNCONFIRMED_POOL_DEPOSIT_NQT = (isTestnet ? 50 : 100) * ONE_NXT;

		public static readonly long EPOCH_BEGINNING;

		public const string ALPHABET = "0123456789abcdefghijklmnopqrstuvwxyz";

		public const int EC_RULE_TERMINATOR = 600; // cfb: This constant defines a straight edge when "longest chain"
//                                                        rule is outweighed by "economic majority" rule; the terminator
//                                                        is set as number of seconds before the current time. 

		public const int EC_BLOCK_DISTANCE_LIMIT = 60;

		private Constants() // never
		{
		}

	}

}