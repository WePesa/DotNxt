using System;
using System.Collections.Generic;
using System.Text;

namespace nxt
{

	using Db = nxt.db.Db;
	using DbIterator = nxt.db.DbIterator;
	using DbUtils = nxt.db.DbUtils;


	internal sealed class BlockchainImpl : Blockchain
	{

		private static readonly BlockchainImpl instance = new BlockchainImpl();

		static BlockchainImpl Instance
		{
			get
			{
				return instance;
			}
		}

		private BlockchainImpl()
		{
		}

		private readonly AtomicReference<BlockImpl> lastBlock = new AtomicReference<>();

		public override BlockImpl LastBlock
		{
			get
			{
				return lastBlock.get();
			}
			set
			{
				lastBlock.set(value);
			}
		}


		internal void setLastBlock(BlockImpl previousBlock, BlockImpl block)
		{
			if(! lastBlock.compareAndSet(previousBlock, block))
			{
				throw new InvalidOperationException("Last block is no longer previous block");
			}
		}

		public override int Height
		{
			get
			{
				BlockImpl last = lastBlock.get();
				return last == null ? 0 : last.Height;
			}
		}

		public override BlockImpl getLastBlock(int timestamp)
		{
			BlockImpl block = lastBlock.get();
			if(timestamp >= block.Timestamp)
			{
				return block;
			}
			return BlockDb.findLastBlock(timestamp);
		}

		public override BlockImpl getBlock(long blockId)
		{
			BlockImpl block = lastBlock.get();
			if(block.Id == blockId)
			{
				return block;
			}
			return BlockDb.findBlock(blockId);
		}

		public override bool hasBlock(long blockId)
		{
			return lastBlock.get().Id == blockId || BlockDb.hasBlock(blockId);
		}

		public override DbIterator<BlockImpl> AllBlocks
		{
			get
			{
				Connection con = null;
				try
				{
					con = Db.Connection;
					PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block ORDER BY db_id ASC");
					return getBlocks(con, pstmt);
				}
				catch(SQLException e)
				{
					DbUtils.close(con);
					throw new Exception(e.ToString(), e);
				}
			}
		}

		public override DbIterator<BlockImpl> getBlocks(int from, int to)
		{
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block WHERE height <= ? AND height >= ? ORDER BY height DESC");
				int blockchainHeight = Height;
				pstmt.setInt(1, blockchainHeight - Math.Max(from, 0));
				pstmt.setInt(2, to > 0 ? blockchainHeight - to : 0);
				return getBlocks(con, pstmt);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public override DbIterator<BlockImpl> getBlocks(Account account, int timestamp)
		{
			return getBlocks(account, timestamp, 0, -1);
		}

		public override DbIterator<BlockImpl> getBlocks(Account account, int timestamp, int from, int to)
		{
			Connection con = null;
			try
			{
				con = Db.Connection;
				PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block WHERE generator_id = ? " + (timestamp > 0 ? " AND timestamp >= ? " : " ") + "ORDER BY db_id DESC" + DbUtils.limitsClause(from, to));
				int i = 0;
				pstmt.setLong(++i, account.Id);
				if(timestamp > 0)
				{
					pstmt.setInt(++i, timestamp);
				}
				DbUtils.setLimits(++i, pstmt, from, to);
				return getBlocks(con, pstmt);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public override DbIterator<BlockImpl> getBlocks(Connection con, PreparedStatement pstmt)
		{
			return new DbIterator<>(con, pstmt, new DbIterator.ResultSetReader<BlockImpl>() { public BlockImpl get(Connection con, ResultSet rs) throws NxtException.ValidationException { return BlockDb.loadBlock(con, rs); } });
		}

		public override IList<long?> getBlockIdsAfter(long blockId, int limit)
		{
			if(limit > 1440)
			{
				throw new System.ArgumentException("Can't get more than 1440 blocks at a time");
			}
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT id FROM block WHERE db_id > (SELECT db_id FROM block WHERE id = ?) ORDER BY db_id ASC LIMIT ?"))
			{
				IList<long?> result = new List<>();
				pstmt.setLong(1, blockId);
				pstmt.setInt(2, limit);
				using (ResultSet rs = pstmt.executeQuery())
				{
					while(rs.next())
					{
						result.Add(rs.getLong("id"));
					}
				}
				return result;
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public override IList<BlockImpl> getBlocksAfter(long blockId, int limit)
		{
			if(limit > 1440)
			{
				throw new System.ArgumentException("Can't get more than 1440 blocks at a time");
			}
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM block WHERE db_id > (SELECT db_id FROM block WHERE id = ?) ORDER BY db_id ASC LIMIT ?"))
			{
				IList<BlockImpl> result = new List<>();
				pstmt.setLong(1, blockId);
				pstmt.setInt(2, limit);
				using (ResultSet rs = pstmt.executeQuery())
				{
					while(rs.next())
					{
						result.Add(BlockDb.loadBlock(con, rs));
					}
				}
				return result;
			}
			catch(NxtException.ValidationException|SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		public override long getBlockIdAtHeight(int height)
		{
			Block block = lastBlock.get();
			if(height > block.Height)
			{
				throw new System.ArgumentException("Invalid height " + height + ", current blockchain is at " + block.Height);
			}
			if(height == block.Height)
			{
				return block.Id;
			}
			return BlockDb.findBlockIdAtHeight(height);
		}

		public override BlockImpl getBlockAtHeight(int height)
		{
			BlockImpl block = lastBlock.get();
			if(height > block.Height)
			{
				throw new System.ArgumentException("Invalid height " + height + ", current blockchain is at " + block.Height);
			}
			if(height == block.Height)
			{
				return block;
			}
			return BlockDb.findBlockAtHeight(height);
		}

		public override Transaction getTransaction(long transactionId)
		{
			return TransactionDb.findTransaction(transactionId);
		}

		public override Transaction getTransactionByFullHash(string fullHash)
		{
			return TransactionDb.findTransactionByFullHash(fullHash);
		}

		public override bool hasTransaction(long transactionId)
		{
			return TransactionDb.hasTransaction(transactionId);
		}

		public override bool hasTransactionByFullHash(string fullHash)
		{
			return TransactionDb.hasTransactionByFullHash(fullHash);
		}

		public override int TransactionCount
		{
			get
			{
				using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT COUNT(*) FROM transaction"), ResultSet rs = pstmt.executeQuery())
				{
					rs.next();
					return rs.getInt(1);
				}
				catch(SQLException e)
				{
					throw new Exception(e.ToString(), e);
				}
			}
		}

		public override DbIterator<TransactionImpl> AllTransactions
		{
			get
			{
				Connection con = null;
				try
				{
					con = Db.Connection;
					PreparedStatement pstmt = con.prepareStatement("SELECT * FROM transaction ORDER BY db_id ASC");
					return getTransactions(con, pstmt);
				}
				catch(SQLException e)
				{
					DbUtils.close(con);
					throw new Exception(e.ToString(), e);
				}
			}
		}

		public override DbIterator<TransactionImpl> getTransactions(Account account, sbyte type, sbyte subtype, int blockTimestamp)
		{
			return getTransactions(account, 0, type, subtype, blockTimestamp, 0, -1);
		}

		public override DbIterator<TransactionImpl> getTransactions(Account account, int numberOfConfirmations, sbyte type, sbyte subtype, int blockTimestamp, int from, int to)
		{
			int height = numberOfConfirmations > 0 ? Height - numberOfConfirmations : int.MaxValue;
			if(height < 0)
			{
				throw new System.ArgumentException("Number of confirmations required " + numberOfConfirmations + " exceeds current blockchain height " + Height);
			}
			Connection con = null;
			try
			{
				StringBuilder buf = new StringBuilder();
				buf.Append("SELECT * FROM transaction WHERE recipient_id = ? AND sender_id <> ? ");
				if(blockTimestamp > 0)
				{
					buf.Append("AND block_timestamp >= ? ");
				}
				if(type >= 0)
				{
					buf.Append("AND type = ? ");
					if(subtype >= 0)
					{
						buf.Append("AND subtype = ? ");
					}
				}
				if(height < int.MaxValue)
				{
					buf.Append("AND height <= ? ");
				}
				buf.Append("UNION ALL SELECT * FROM transaction WHERE sender_id = ? ");
				if(blockTimestamp > 0)
				{
					buf.Append("AND block_timestamp >= ? ");
				}
				if(type >= 0)
				{
					buf.Append("AND type = ? ");
					if(subtype >= 0)
					{
						buf.Append("AND subtype = ? ");
					}
				}
				if(height < int.MaxValue)
				{
					buf.Append("AND height <= ? ");
				}
				buf.Append("ORDER BY block_timestamp DESC, id DESC");
				buf.Append(DbUtils.limitsClause(from, to));
				con = Db.Connection;
				PreparedStatement pstmt;
				int i = 0;
				pstmt = con.prepareStatement(buf.ToString());
				pstmt.setLong(++i, account.Id);
				pstmt.setLong(++i, account.Id);
				if(blockTimestamp > 0)
				{
					pstmt.setInt(++i, blockTimestamp);
				}
				if(type >= 0)
				{
					pstmt.setByte(++i, type);
					if(subtype >= 0)
					{
						pstmt.setByte(++i, subtype);
					}
				}
				if(height < int.MaxValue)
				{
					pstmt.setInt(++i, height);
				}
				pstmt.setLong(++i, account.Id);
				if(blockTimestamp > 0)
				{
					pstmt.setInt(++i, blockTimestamp);
				}
				if(type >= 0)
				{
					pstmt.setByte(++i, type);
					if(subtype >= 0)
					{
						pstmt.setByte(++i, subtype);
					}
				}
				if(height < int.MaxValue)
				{
					pstmt.setInt(++i, height);
				}
				DbUtils.setLimits(++i, pstmt, from, to);
				return getTransactions(con, pstmt);
			}
			catch(SQLException e)
			{
				DbUtils.close(con);
				throw new Exception(e.ToString(), e);
			}
		}

		public override DbIterator<TransactionImpl> getTransactions(Connection con, PreparedStatement pstmt)
		{
			return new DbIterator<>(con, pstmt, new DbIterator.ResultSetReader<TransactionImpl>() { public TransactionImpl get(Connection con, ResultSet rs) throws NxtException.ValidationException { return TransactionDb.loadTransaction(con, rs); } });
		}

	}

}