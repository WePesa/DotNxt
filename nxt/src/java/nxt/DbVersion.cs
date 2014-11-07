using System;

namespace nxt
{

	using Db = nxt.db.Db;
	using Logger = nxt.util.Logger;


	internal sealed class DbVersion
	{

		internal static void init()
		{
			using (Connection con = Db.beginTransaction(), Statement stmt = con.createStatement())
			{
				int nextUpdate = 1;
				try
				{
					ResultSet rs = stmt.executeQuery("SELECT next_update FROM version");
					if(! rs.next())
					{
						throw new Exception("Invalid version table");
					}
					nextUpdate = rs.getInt("next_update");
					if(! rs.Last)
					{
						throw new Exception("Invalid version table");
					}
					rs.close();
					Logger.logMessage("Database update may take a while if needed, current db version " + (nextUpdate - 1) + "...");
				}
				catch(SQLException e)
				{
					Logger.logMessage("Initializing an empty database");
					stmt.executeUpdate("CREATE TABLE version (next_update INT NOT NULL)");
					stmt.executeUpdate("INSERT INTO version VALUES (1)");
					Db.commitTransaction();
				}
				update(nextUpdate);
			}
			catch(SQLException e)
			{
				Db.rollbackTransaction();
				throw new Exception(e.ToString(), e);
			}
			finally
			{
				Db.endTransaction();
			}

		}

		private static void apply(string sql)
		{
			using (Connection con = Db.Connection, Statement stmt = con.createStatement())
			{
				try
				{
					if(sql != null)
					{
						Logger.logDebugMessage("Will apply sql:\n" + sql);
						stmt.executeUpdate(sql);
					}
					stmt.executeUpdate("UPDATE version SET next_update = next_update + 1");
					Db.commitTransaction();
				}
				catch(Exception e)
				{
					Db.rollbackTransaction();
					throw e;
				}
			}
			catch(SQLException e)
			{
				throw new Exception("Database error executing " + sql, e);
			}
		}

		private static void update(int nextUpdate)
		{
			switch (nextUpdate)
			{
				case 1:
					apply("CREATE TABLE IF NOT EXISTS block (db_id IDENTITY, id BIGINT NOT NULL, version INT NOT NULL, " + "timestamp INT NOT NULL, previous_block_id BIGINT, " + "FOREIGN KEY (previous_block_id) REFERENCES block (id) ON DELETE CASCADE, total_amount INT NOT NULL, " + "total_fee INT NOT NULL, payload_length INT NOT NULL, generator_public_key BINARY(32) NOT NULL, " + "previous_block_hash BINARY(32), cumulative_difficulty VARBINARY NOT NULL, base_target BIGINT NOT NULL, " + "next_block_id BIGINT, FOREIGN KEY (next_block_id) REFERENCES block (id) ON DELETE SET NULL, " + "index INT NOT NULL, height INT NOT NULL, generation_signature BINARY(64) NOT NULL, " + "block_signature BINARY(64) NOT NULL, payload_hash BINARY(32) NOT NULL, generator_account_id BIGINT NOT NULL)");
				goto case 2;
				case 2:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS block_id_idx ON block (id)");
				goto case 3;
				case 3:
					apply("CREATE TABLE IF NOT EXISTS transaction (db_id IDENTITY, id BIGINT NOT NULL, " + "deadline SMALLINT NOT NULL, sender_public_key BINARY(32) NOT NULL, recipient_id BIGINT NOT NULL, " + "amount INT NOT NULL, fee INT NOT NULL, referenced_transaction_id BIGINT, index INT NOT NULL, " + "height INT NOT NULL, block_id BIGINT NOT NULL, FOREIGN KEY (block_id) REFERENCES block (id) ON DELETE CASCADE, " + "signature BINARY(64) NOT NULL, timestamp INT NOT NULL, type TINYINT NOT NULL, subtype TINYINT NOT NULL, " + "sender_account_id BIGINT NOT NULL, attachment OTHER)");
				goto case 4;
				case 4:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS transaction_id_idx ON transaction (id)");
				goto case 5;
				case 5:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS block_height_idx ON block (height)");
				goto case 6;
				case 6:
					apply("CREATE INDEX IF NOT EXISTS transaction_timestamp_idx ON transaction (timestamp)");
				goto case 7;
				case 7:
					apply("CREATE INDEX IF NOT EXISTS block_generator_account_id_idx ON block (generator_account_id)");
				goto case 8;
				case 8:
					apply("CREATE INDEX IF NOT EXISTS transaction_sender_account_id_idx ON transaction (sender_account_id)");
				goto case 9;
				case 9:
					apply("CREATE INDEX IF NOT EXISTS transaction_recipient_id_idx ON transaction (recipient_id)");
				goto case 10;
				case 10:
					apply("ALTER TABLE block ALTER COLUMN generator_account_id RENAME TO generator_id");
				goto case 11;
				case 11:
					apply("ALTER TABLE transaction ALTER COLUMN sender_account_id RENAME TO sender_id");
				goto case 12;
				case 12:
					apply("ALTER INDEX block_generator_account_id_idx RENAME TO block_generator_id_idx");
				goto case 13;
				case 13:
					apply("ALTER INDEX transaction_sender_account_id_idx RENAME TO transaction_sender_id_idx");
				goto case 14;
				case 14:
					apply("ALTER TABLE block DROP COLUMN IF EXISTS index");
				goto case 15;
				case 15:
					apply("ALTER TABLE transaction DROP COLUMN IF EXISTS index");
				goto case 16;
				case 16:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS block_timestamp INT");
				goto case 17;
				case 17:
					apply(null);
				goto case 18;
				case 18:
					apply("ALTER TABLE transaction ALTER COLUMN block_timestamp SET NOT NULL");
				goto case 19;
				case 19:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS hash BINARY(32)");
				goto case 20;
				case 20:
					apply(null);
				goto case 21;
				case 21:
					apply(null);
				goto case 22;
				case 22:
					apply("CREATE INDEX IF NOT EXISTS transaction_hash_idx ON transaction (hash)");
				goto case 23;
				case 23:
					apply(null);
				goto case 24;
				case 24:
					apply("ALTER TABLE block ALTER COLUMN total_amount BIGINT");
				goto case 25;
				case 25:
					apply("ALTER TABLE block ALTER COLUMN total_fee BIGINT");
				goto case 26;
				case 26:
					apply("ALTER TABLE transaction ALTER COLUMN amount BIGINT");
				goto case 27;
				case 27:
					apply("ALTER TABLE transaction ALTER COLUMN fee BIGINT");
				goto case 28;
				case 28:
					apply(null);
				goto case 29;
				case 29:
					apply(null);
				goto case 30;
				case 30:
					apply(null);
				goto case 31;
				case 31:
					apply(null);
				goto case 32;
				case 32:
					apply(null);
				goto case 33;
				case 33:
					apply(null);
				goto case 34;
				case 34:
					apply(null);
				goto case 35;
				case 35:
					apply(null);
				goto case 36;
				case 36:
					apply("CREATE TABLE IF NOT EXISTS peer (address VARCHAR PRIMARY KEY)");
				goto case 37;
				case 37:
					if(!Constants.isTestnet)
					{
						apply("INSERT INTO peer (address) VALUES " + "('174.140.167.239'), ('181.165.178.28'), ('dtodorov.asuscomm.com'), ('88.163.78.131'), ('nxt01.now.im'), " + "('89.72.57.246'), ('nxtx.ru'), ('212.47.237.7'), ('79.30.180.223'), ('nacho.damnserver.com'), " + "('node6.mynxtcoin.org'), ('185.12.44.108'), ('gunka.szn.dk'), ('128.199.189.226'), ('23.89.192.151'), " + "('95.24.83.220'), ('188.35.156.10'), ('oldminersnownodes.ddns.net'), ('191.238.101.73'), ('188.226.197.131'), " + "('54.187.153.45'), ('23.88.104.225'), ('178.15.99.67'), ('92.222.168.75'), ('210.188.36.5'), " + "('nxt.phukhew.com'), ('sluni.szn.dk'), ('node4.mynxtcoin.org'), ('cryonet.de'), ('54.194.212.248'), " + "('nxtpi.zapto.org'), ('192.157.226.151'), ('67.212.71.171'), ('107.170.164.129'), ('37.139.6.166'), " + "('37.187.21.28'), ('2.225.88.10'), ('198.211.127.34'), ('85.214.222.82'), ('nxtnode.hopto.org'), " + "('46.109.48.18'), ('87.139.122.48'), ('190.10.9.166'), ('148.251.139.82'), ('23.102.0.45'), ('93.103.20.35'), " + "('212.18.225.173'), ('168.63.232.16'), ('nxs1.hanza.co.id'), ('78.46.92.78'), ('nxt.sx'), " + "('174.140.166.124'), ('54.83.4.11'), ('81.2.216.179'), ('46.237.8.30'), ('77.88.208.12'), ('54.77.63.53'), " + "('37.120.168.131'), ('178.150.207.53'), ('node0.forgenxt.com'), ('46.4.212.230'), ('81.64.77.101'), " + "('87.139.122.157'), ('lan.wow64.net'), ('128.199.160.141'), ('107.170.3.62'), ('212.47.228.0'), " + "('54.200.114.193'), ('84.133.75.209'), ('217.26.24.27'), ('5.196.1.215'), ('67.212.71.173'), " + "('nxt1.achnodes.com'), ('178.32.221.58'), ('188.226.206.41'), ('198.199.95.15'), ('nxt.alkeron.com'), " + "('85.84.67.234'), ('96.251.124.95'), ('woll-e.net'), ('128.199.228.211'), ('109.230.224.65'), " + "('humanoide.thican.net'), ('95.85.31.45'), ('176.9.0.19'), ('91.121.150.75'), ('213.46.57.77'), " + "('178.162.198.109'), ('108.170.40.4'), ('84.128.162.237'), ('54.200.116.75'), ('miasik.no-ip.org'), " + "('nxt.cybermailing.com'), ('23.88.246.117'), ('54.213.222.141'), ('185.21.192.9'), " + "('dorcsforge.cloudapp.net'), ('188.226.245.226'), ('167.206.61.3'), ('107.170.75.92'), ('211.149.213.86'), " + "('5.150.195.208'), ('96.240.128.221')");
					}
					else
					{
						apply("INSERT INTO peer (address) VALUES " + "('nxt.scryptmh.eu'), ('54.186.98.117'), ('178.150.207.53'), ('192.241.223.132'), ('node9.mynxtcoin.org'), " + "('node10.mynxtcoin.org'), ('node3.mynxtcoin.org'), ('109.87.169.253'), ('nxtnet.fr'), ('50.112.241.97'), " + "('2.84.142.149'), ('bug.airdns.org'), ('83.212.103.14'), ('62.210.131.30'), ('104.131.254.22'), " + "('46.28.111.249'), ('94.79.54.205')");
					}
					goto case 38;
				case 38:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS full_hash BINARY(32)");
				goto case 39;
				case 39:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS referenced_transaction_full_hash BINARY(32)");
				goto case 40;
				case 40:
					apply(null);
				goto case 41;
				case 41:
					apply("ALTER TABLE transaction ALTER COLUMN full_hash SET NOT NULL");
				goto case 42;
				case 42:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS transaction_full_hash_idx ON transaction (full_hash)");
				goto case 43;
				case 43:
					apply(null);
				goto case 44;
				case 44:
					apply(null);
				goto case 45;
				case 45:
					apply(null);
				goto case 46;
				case 46:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS attachment_bytes VARBINARY");
				goto case 47;
				case 47:
					apply(null);
				goto case 48;
				case 48:
					apply("ALTER TABLE transaction DROP COLUMN attachment");
				goto case 49;
				case 49:
					apply(null);
				goto case 50;
				case 50:
					apply("ALTER TABLE transaction DROP COLUMN referenced_transaction_id");
				goto case 51;
				case 51:
					apply("ALTER TABLE transaction DROP COLUMN hash");
				goto case 52;
				case 52:
					apply(null);
				goto case 53;
				case 53:
					apply("DROP INDEX transaction_recipient_id_idx");
				goto case 54;
				case 54:
					apply("ALTER TABLE transaction ALTER COLUMN recipient_id SET NULL");
				goto case 55;
				case 55:
					BlockDb.deleteAll();
					apply(null);
				goto case 56;
				case 56:
					apply("CREATE INDEX IF NOT EXISTS transaction_recipient_id_idx ON transaction (recipient_id)");
				goto case 57;
				case 57:
					apply(null);
				goto case 58;
				case 58:
					apply(null);
				goto case 59;
				case 59:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS version TINYINT");
				goto case 60;
				case 60:
					apply("UPDATE transaction SET version = 0");
				goto case 61;
				case 61:
					apply("ALTER TABLE transaction ALTER COLUMN version SET NOT NULL");
				goto case 62;
				case 62:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS has_message BOOLEAN NOT NULL DEFAULT FALSE");
				goto case 63;
				case 63:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS has_encrypted_message BOOLEAN NOT NULL DEFAULT FALSE");
				goto case 64;
				case 64:
					apply("UPDATE transaction SET has_message = TRUE WHERE type = 1 AND subtype = 0");
				goto case 65;
				case 65:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS has_public_key_announcement BOOLEAN NOT NULL DEFAULT FALSE");
				goto case 66;
				case 66:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS ec_block_height INT DEFAULT NULL");
				goto case 67;
				case 67:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS ec_block_id BIGINT DEFAULT NULL");
				goto case 68;
				case 68:
					apply("ALTER TABLE transaction ADD COLUMN IF NOT EXISTS has_encrypttoself_message BOOLEAN NOT NULL DEFAULT FALSE");
				goto case 69;
				case 69:
					apply("CREATE INDEX IF NOT EXISTS transaction_block_timestamp_idx ON transaction (block_timestamp DESC)");
				goto case 70;
				case 70:
					apply("DROP INDEX transaction_timestamp_idx");
				goto case 71;
				case 71:
					apply("CREATE TABLE IF NOT EXISTS alias (db_id IDENTITY, id BIGINT NOT NULL, " + "account_id BIGINT NOT NULL, alias_name VARCHAR NOT NULL, " + "alias_name_lower VARCHAR AS LOWER (alias_name) NOT NULL, " + "alias_uri VARCHAR NOT NULL, timestamp INT NOT NULL, " + "height INT NOT NULL, latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 72;
				case 72:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS alias_id_height_idx ON alias (id, height DESC)");
				goto case 73;
				case 73:
					apply("CREATE INDEX IF NOT EXISTS alias_account_id_idx ON alias (account_id, height DESC)");
				goto case 74;
				case 74:
					apply("CREATE INDEX IF NOT EXISTS alias_name_lower_idx ON alias (alias_name_lower)");
				goto case 75;
				case 75:
					apply("CREATE TABLE IF NOT EXISTS alias_offer (db_id IDENTITY, id BIGINT NOT NULL, " + "price BIGINT NOT NULL, buyer_id BIGINT, " + "height INT NOT NULL, latest BOOLEAN DEFAULT TRUE NOT NULL)");
				goto case 76;
				case 76:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS alias_offer_id_height_idx ON alias_offer (id, height DESC)");
				goto case 77;
				case 77:
					apply("CREATE TABLE IF NOT EXISTS asset (db_id IDENTITY, id BIGINT NOT NULL, account_id BIGINT NOT NULL, " + "name VARCHAR NOT NULL, description VARCHAR, quantity BIGINT NOT NULL, decimals TINYINT NOT NULL, " + "height INT NOT NULL)");
				goto case 78;
				case 78:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS asset_id_idx ON asset (id)");
				goto case 79;
				case 79:
					apply("CREATE INDEX IF NOT EXISTS asset_account_id_idx ON asset (account_id)");
				goto case 80;
				case 80:
					apply("CREATE TABLE IF NOT EXISTS trade (db_id IDENTITY, asset_id BIGINT NOT NULL, block_id BIGINT NOT NULL, " + "ask_order_id BIGINT NOT NULL, bid_order_id BIGINT NOT NULL, ask_order_height INT NOT NULL, " + "bid_order_height INT NOT NULL, seller_id BIGINT NOT NULL, buyer_id BIGINT NOT NULL, " + "quantity BIGINT NOT NULL, price BIGINT NOT NULL, timestamp INT NOT NULL, height INT NOT NULL)");
				goto case 81;
				case 81:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS trade_ask_bid_idx ON trade (ask_order_id, bid_order_id)");
				goto case 82;
				case 82:
					apply("CREATE INDEX IF NOT EXISTS trade_asset_id_idx ON trade (asset_id, height DESC)");
				goto case 83;
				case 83:
					apply("CREATE INDEX IF NOT EXISTS trade_seller_id_idx ON trade (seller_id, height DESC)");
				goto case 84;
				case 84:
					apply("CREATE INDEX IF NOT EXISTS trade_buyer_id_idx ON trade (buyer_id, height DESC)");
				goto case 85;
				case 85:
					apply("CREATE TABLE IF NOT EXISTS ask_order (db_id IDENTITY, id BIGINT NOT NULL, account_id BIGINT NOT NULL, " + "asset_id BIGINT NOT NULL, price BIGINT NOT NULL, " + "quantity BIGINT NOT NULL, creation_height INT NOT NULL, height INT NOT NULL, " + "latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 86;
				case 86:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS ask_order_id_height_idx ON ask_order (id, height DESC)");
				goto case 87;
				case 87:
					apply("CREATE INDEX IF NOT EXISTS ask_order_account_id_idx ON ask_order (account_id, height DESC)");
				goto case 88;
				case 88:
					apply("CREATE INDEX IF NOT EXISTS ask_order_asset_id_price_idx ON ask_order (asset_id, price)");
				goto case 89;
				case 89:
					apply("CREATE TABLE IF NOT EXISTS bid_order (db_id IDENTITY, id BIGINT NOT NULL, account_id BIGINT NOT NULL, " + "asset_id BIGINT NOT NULL, price BIGINT NOT NULL, " + "quantity BIGINT NOT NULL, creation_height INT NOT NULL, height INT NOT NULL, " + "latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 90;
				case 90:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS bid_order_id_height_idx ON bid_order (id, height DESC)");
				goto case 91;
				case 91:
					apply("CREATE INDEX IF NOT EXISTS bid_order_account_id_idx ON bid_order (account_id, height DESC)");
				goto case 92;
				case 92:
					apply("CREATE INDEX IF NOT EXISTS bid_order_asset_id_price_idx ON bid_order (asset_id, price DESC)");
				goto case 93;
				case 93:
					apply("CREATE TABLE IF NOT EXISTS goods (db_id IDENTITY, id BIGINT NOT NULL, seller_id BIGINT NOT NULL, " + "name VARCHAR NOT NULL, description VARCHAR, " + "tags VARCHAR, timestamp INT NOT NULL, quantity INT NOT NULL, price BIGINT NOT NULL, " + "delisted BOOLEAN NOT NULL, height INT NOT NULL, latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 94;
				case 94:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS goods_id_height_idx ON goods (id, height DESC)");
				goto case 95;
				case 95:
					apply("CREATE INDEX IF NOT EXISTS goods_seller_id_name_idx ON goods (seller_id, name)");
				goto case 96;
				case 96:
					apply("CREATE INDEX IF NOT EXISTS goods_timestamp_idx ON goods (timestamp DESC, height DESC)");
				goto case 97;
				case 97:
					apply("CREATE TABLE IF NOT EXISTS purchase (db_id IDENTITY, id BIGINT NOT NULL, buyer_id BIGINT NOT NULL, " + "goods_id BIGINT NOT NULL, " + "seller_id BIGINT NOT NULL, quantity INT NOT NULL, " + "price BIGINT NOT NULL, deadline INT NOT NULL, note VARBINARY, nonce BINARY(32), " + "timestamp INT NOT NULL, pending BOOLEAN NOT NULL, goods VARBINARY, goods_nonce BINARY(32), " + "refund_note VARBINARY, refund_nonce BINARY(32), has_feedback_notes BOOLEAN NOT NULL DEFAULT FALSE, " + "has_public_feedbacks BOOLEAN NOT NULL DEFAULT FALSE, discount BIGINT NOT NULL, refund BIGINT NOT NULL, " + "height INT NOT NULL, latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 98;
				case 98:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS purchase_id_height_idx ON purchase (id, height DESC)");
				goto case 99;
				case 99:
					apply("CREATE INDEX IF NOT EXISTS purchase_buyer_id_height_idx ON purchase (buyer_id, height DESC)");
				goto case 100;
				case 100:
					apply("CREATE INDEX IF NOT EXISTS purchase_seller_id_height_idx ON purchase (seller_id, height DESC)");
				goto case 101;
				case 101:
					apply("CREATE INDEX IF NOT EXISTS purchase_deadline_idx ON purchase (deadline DESC, height DESC)");
				goto case 102;
				case 102:
					apply("CREATE TABLE IF NOT EXISTS account (db_id IDENTITY, id BIGINT NOT NULL, creation_height INT NOT NULL, " + "public_key BINARY(32), key_height INT, balance BIGINT NOT NULL, unconfirmed_balance BIGINT NOT NULL, " + "forged_balance BIGINT NOT NULL, name VARCHAR, description VARCHAR, current_leasing_height_from INT, " + "current_leasing_height_to INT, current_lessee_id BIGINT NULL, next_leasing_height_from INT, " + "next_leasing_height_to INT, next_lessee_id BIGINT NULL, height INT NOT NULL, " + "latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 103;
				case 103:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS account_id_height_idx ON account (id, height DESC)");
				goto case 104;
				case 104:
					apply("CREATE INDEX IF NOT EXISTS account_current_lessee_id_leasing_height_idx ON account (current_lessee_id, " + "current_leasing_height_to DESC)");
				goto case 105;
				case 105:
					apply("CREATE TABLE IF NOT EXISTS account_asset (db_id IDENTITY, account_id BIGINT NOT NULL, " + "asset_id BIGINT NOT NULL, quantity BIGINT NOT NULL, unconfirmed_quantity BIGINT NOT NULL, height INT NOT NULL, " + "latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 106;
				case 106:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS account_asset_id_height_idx ON account_asset (account_id, asset_id, height DESC)");
				goto case 107;
				case 107:
					apply("CREATE TABLE IF NOT EXISTS account_guaranteed_balance (db_id IDENTITY, account_id BIGINT NOT NULL, " + "additions BIGINT NOT NULL, height INT NOT NULL)");
				goto case 108;
				case 108:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS account_guaranteed_balance_id_height_idx ON account_guaranteed_balance " + "(account_id, height DESC)");
				goto case 109;
				case 109:
					apply("CREATE TABLE IF NOT EXISTS purchase_feedback (db_id IDENTITY, id BIGINT NOT NULL, feedback_data VARBINARY NOT NULL, " + "feedback_nonce BINARY(32) NOT NULL, height INT NOT NULL, latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 110;
				case 110:
					apply("CREATE INDEX IF NOT EXISTS purchase_feedback_id_height_idx ON purchase_feedback (id, height DESC)");
				goto case 111;
				case 111:
					apply("CREATE TABLE IF NOT EXISTS purchase_public_feedback (db_id IDENTITY, id BIGINT NOT NULL, public_feedback " + "VARCHAR NOT NULL, height INT NOT NULL, latest BOOLEAN NOT NULL DEFAULT TRUE)");
				goto case 112;
				case 112:
					apply("CREATE INDEX IF NOT EXISTS purchase_public_feedback_id_height_idx ON purchase_public_feedback (id, height DESC)");
				goto case 113;
				case 113:
					apply("CREATE TABLE IF NOT EXISTS unconfirmed_transaction (db_id IDENTITY, id BIGINT NOT NULL, expiration INT NOT NULL, " + "transaction_height INT NOT NULL, fee_per_byte BIGINT NOT NULL, timestamp INT NOT NULL, " + "transaction_bytes VARBINARY NOT NULL, height INT NOT NULL)");
				goto case 114;
				case 114:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS unconfirmed_transaction_id_idx ON unconfirmed_transaction (id)");
				goto case 115;
				case 115:
					apply("CREATE INDEX IF NOT EXISTS unconfirmed_transaction_height_fee_timestamp_idx ON unconfirmed_transaction " + "(transaction_height ASC, fee_per_byte DESC, timestamp ASC)");
				goto case 116;
				case 116:
					apply("CREATE TABLE IF NOT EXISTS asset_transfer (db_id IDENTITY, id BIGINT NOT NULL, asset_id BIGINT NOT NULL, " + "sender_id BIGINT NOT NULL, recipient_id BIGINT NOT NULL, quantity BIGINT NOT NULL, timestamp INT NOT NULL, " + "height INT NOT NULL)");
				goto case 117;
				case 117:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS asset_transfer_id_idx ON asset_transfer (id)");
				goto case 118;
				case 118:
					apply("CREATE INDEX IF NOT EXISTS asset_transfer_asset_id_idx ON asset_transfer (asset_id, height DESC)");
				goto case 119;
				case 119:
					apply("CREATE INDEX IF NOT EXISTS asset_transfer_sender_id_idx ON asset_transfer (sender_id, height DESC)");
				goto case 120;
				case 120:
					apply("CREATE INDEX IF NOT EXISTS asset_transfer_recipient_id_idx ON asset_transfer (recipient_id, height DESC)");
				goto case 121;
				case 121:
					apply(null);
				goto case 122;
				case 122:
					apply("CREATE INDEX IF NOT EXISTS account_asset_quantity_idx ON account_asset (quantity DESC)");
				goto case 123;
				case 123:
					apply("CREATE INDEX IF NOT EXISTS purchase_timestamp_idx ON purchase (timestamp DESC, id)");
				goto case 124;
				case 124:
					apply("CREATE INDEX IF NOT EXISTS ask_order_creation_idx ON ask_order (creation_height DESC)");
				goto case 125;
				case 125:
					apply("CREATE INDEX IF NOT EXISTS bid_order_creation_idx ON bid_order (creation_height DESC)");
				goto case 126;
				case 126:
					BlockchainProcessorImpl.Instance.validateAtNextScan();
					BlockchainProcessorImpl.Instance.forceScanAtStart();
					apply(null);
				goto case 127;
				case 127:
					apply("CREATE UNIQUE INDEX IF NOT EXISTS block_timestamp_idx ON block (timestamp DESC)");
				goto case 128;
				case 128:
					return;
				default:
					throw new Exception("Database inconsistent with code, probably trying to run older code on newer database");
			}
		}

		private DbVersion() //never
		{
		}
	}

}