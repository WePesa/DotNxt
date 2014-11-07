using System;

namespace nxt
{

	using Db = nxt.db.Db;
	using DbUtils = nxt.db.DbUtils;
	using Logger = nxt.util.Logger;


	internal sealed class BlockDb
	{

		internal static BlockImpl findBlock(long blockId)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block WHERE id = ?"))
			{
				pstmt.setLong(1, blockId);
				using (ResultSet rs = pstmt.executeQuery())
				{
					BlockImpl block = null;
					if(rs.next())
					{
						block = loadBlock(con, rs);
					}
					return block;
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			catch(NxtException.ValidationException e)
			{
				throw new Exception("Block already in database, id = " + blockId + ", does not pass validation!", e);
			}
		}

		internal static bool hasBlock(long blockId)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT 1 FROM block WHERE id = ?"))
			{
				pstmt.setLong(1, blockId);
				using (ResultSet rs = pstmt.executeQuery())
				{
					return rs.next();
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		internal static long findBlockIdAtHeight(int height)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT id FROM block WHERE height = ?"))
			{
				pstmt.setInt(1, height);
				using (ResultSet rs = pstmt.executeQuery())
				{
					if(!rs.next())
					{
						throw new Exception("Block at height " + height + " not found in database!");
					}
					return rs.getLong("id");
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		internal static BlockImpl findBlockAtHeight(int height)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block WHERE height = ?"))
			{
				pstmt.setInt(1, height);
				using (ResultSet rs = pstmt.executeQuery())
				{
					BlockImpl block;
					if(rs.next())
					{
						block = loadBlock(con, rs);
					}
					else
					{
						throw new Exception("Block at height " + height + " not found in database!");
					}
					return block;
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			catch(NxtException.ValidationException e)
			{
				throw new Exception("Block already in database at height " + height + ", does not pass validation!", e);
			}
		}

		internal static BlockImpl findLastBlock()
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block ORDER BY db_id DESC LIMIT 1"))
			{
				BlockImpl block = null;
				using (ResultSet rs = pstmt.executeQuery())
				{
					if(rs.next())
					{
						block = loadBlock(con, rs);
					}
				}
				return block;
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			catch(NxtException.ValidationException e)
			{
				throw new Exception("Last block already in database does not pass validation!", e);
			}
		}

		internal static BlockImpl findLastBlock(int timestamp)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block WHERE timestamp <= ? ORDER BY timestamp DESC LIMIT 1"))
			{
				pstmt.setInt(1, timestamp);
				BlockImpl block = null;
				using (ResultSet rs = pstmt.executeQuery())
				{
					if(rs.next())
					{
						block = loadBlock(con, rs);
					}
				}
				return block;
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			catch(NxtException.ValidationException e)
			{
				throw new Exception("Block already in database at timestamp " + timestamp + " does not pass validation!", e);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static BlockImpl loadBlock(Connection con, ResultSet rs) throws NxtException.ValidationException
		internal static BlockImpl loadBlock(Connection con, ResultSet rs)
		{
			try
			{
				int version = rs.getInt("version");
				int timestamp = rs.getInt("timestamp");
				long previousBlockId = rs.getLong("previous_block_id");
				long totalAmountNQT = rs.getLong("total_amount");
				long totalFeeNQT = rs.getLong("total_fee");
				int payloadLength = rs.getInt("payload_length");
				sbyte[] generatorPublicKey = rs.getBytes("generator_public_key");
				sbyte[] previousBlockHash = rs.getBytes("previous_block_hash");
				BigInteger cumulativeDifficulty = new BigInteger(rs.getBytes("cumulative_difficulty"));
				long baseTarget = rs.getLong("base_target");
				long nextBlockId = rs.getLong("next_block_id");
				int height = rs.getInt("height");
				sbyte[] generationSignature = rs.getBytes("generation_signature");
				sbyte[] blockSignature = rs.getBytes("block_signature");
				sbyte[] payloadHash = rs.getBytes("payload_hash");
				long id = rs.getLong("id");
				return new BlockImpl(version, timestamp, previousBlockId, totalAmountNQT, totalFeeNQT, payloadLength, payloadHash, generatorPublicKey, generationSignature, blockSignature, previousBlockHash, cumulativeDifficulty, baseTarget, nextBlockId, height, id);
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		internal static void saveBlock(Connection con, BlockImpl block)
		{
			try
			{
				using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO block (id, version, timestamp, previous_block_id, " + "total_amount, total_fee, payload_length, generator_public_key, previous_block_hash, cumulative_difficulty, " + "base_target, height, generation_signature, block_signature, payload_hash, generator_id) " + " VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"))
				{
					int i = 0;
					pstmt.setLong(++i, block.Id);
					pstmt.setInt(++i, block.Version);
					pstmt.setInt(++i, block.Timestamp);
					DbUtils.setLongZeroToNull(pstmt, ++i, block.PreviousBlockId);
					pstmt.setLong(++i, block.TotalAmountNQT);
					pstmt.setLong(++i, block.TotalFeeNQT);
					pstmt.setInt(++i, block.PayloadLength);
					pstmt.setBytes(++i, block.GeneratorPublicKey);
					pstmt.setBytes(++i, block.PreviousBlockHash);
					pstmt.setBytes(++i, block.CumulativeDifficulty.toByteArray());
					pstmt.setLong(++i, block.BaseTarget);
					pstmt.setInt(++i, block.Height);
					pstmt.setBytes(++i, block.GenerationSignature);
					pstmt.setBytes(++i, block.BlockSignature);
					pstmt.setBytes(++i, block.PayloadHash);
					pstmt.setLong(++i, block.GeneratorId);
					pstmt.executeUpdate();
					TransactionDb.saveTransactions(con, block.Transactions);
				}
				if(block.PreviousBlockId != 0)
				{
					using (PreparedStatement pstmt = con.prepareStatement("UPDATE block SET next_block_id = ? WHERE id = ?"))
					{
						pstmt.setLong(1, block.Id);
						pstmt.setLong(2, block.PreviousBlockId);
						pstmt.executeUpdate();
					}
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

	// relying on cascade triggers in the database to delete the transactions for all deleted blocks
		internal static void deleteBlocksFrom(long blockId)
		{
			if(! Db.InTransaction)
			{
				try
				{
					Db.beginTransaction();
					deleteBlocksFrom(blockId);
					Db.commitTransaction();
				}
				catch(Exception e)
				{
					Db.rollbackTransaction();
					throw e;
				}
				finally
				{
					Db.endTransaction();
				}
				return;
			}
			using (Connection con = Db.Connection, PreparedStatement pstmtSelect = con.prepareStatement("SELECT db_id FROM block WHERE db_id >= " + "(SELECT db_id FROM block WHERE id = ?) ORDER BY db_id DESC"), PreparedStatement pstmtDelete = con.prepareStatement("DELETE FROM block WHERE db_id = ?"))
			{
				try
				{
					pstmtSelect.setLong(1, blockId);
					using (ResultSet rs = pstmtSelect.executeQuery())
					{
						Db.commitTransaction();
						while(rs.next())
						{
							pstmtDelete.setInt(1, rs.getInt("db_id"));
							pstmtDelete.executeUpdate();
							Db.commitTransaction();
						}
					}
				}
				catch(SQLException e)
				{
					Db.rollbackTransaction();
					throw e;
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		internal static void deleteAll()
		{
			if(! Db.InTransaction)
			{
				try
				{
					Db.beginTransaction();
					deleteAll();
					Db.commitTransaction();
				}
				catch(Exception e)
				{
					Db.rollbackTransaction();
					throw e;
				}
				finally
				{
					Db.endTransaction();
				}
				return;
			}
			Logger.logMessage("Deleting blockchain...");
			using (Connection con = Db.Connection, Statement stmt = con.createStatement())
			{
				try
				{
					stmt.executeUpdate("SET REFERENTIAL_INTEGRITY FALSE");
					stmt.executeUpdate("TRUNCATE TABLE transaction");
					stmt.executeUpdate("TRUNCATE TABLE block");
					stmt.executeUpdate("SET REFERENTIAL_INTEGRITY TRUE");
					Db.commitTransaction();
				}
				catch(SQLException e)
				{
					Db.rollbackTransaction();
					throw e;
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

	}

}