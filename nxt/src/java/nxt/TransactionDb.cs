using System;
using System.Collections.Generic;

namespace nxt
{

	using Db = nxt.db.Db;
	using DbUtils = nxt.db.DbUtils;
	using Convert = nxt.util.Convert;


	internal sealed class TransactionDb
	{

		internal static Transaction findTransaction(long transactionId)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM transaction WHERE id = ?"))
			{
				pstmt.setLong(1, transactionId);
				using (ResultSet rs = pstmt.executeQuery())
				{
					if(rs.next())
					{
						return loadTransaction(con, rs);
					}
					return null;
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			catch(NxtException.ValidationException e)
			{
				throw new Exception("Transaction already in database, id = " + transactionId + ", does not pass validation!", e);
			}
		}

		internal static Transaction findTransactionByFullHash(string fullHash)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM transaction WHERE full_hash = ?"))
			{
				pstmt.setBytes(1, Convert.parseHexString(fullHash));
				using (ResultSet rs = pstmt.executeQuery())
				{
					if(rs.next())
					{
						return loadTransaction(con, rs);
					}
					return null;
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			catch(NxtException.ValidationException e)
			{
				throw new Exception("Transaction already in database, full_hash = " + fullHash + ", does not pass validation!", e);
			}
		}

		internal static bool hasTransaction(long transactionId)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT 1 FROM transaction WHERE id = ?"))
			{
				pstmt.setLong(1, transactionId);
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

		internal static bool hasTransactionByFullHash(string fullHash)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT 1 FROM transaction WHERE full_hash = ?"))
			{
				pstmt.setBytes(1, Convert.parseHexString(fullHash));
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

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static TransactionImpl loadTransaction(Connection con, ResultSet rs) throws NxtException.ValidationException
		internal static TransactionImpl loadTransaction(Connection con, ResultSet rs)
		{
			try
			{

				sbyte type = rs.getByte("type");
				sbyte subtype = rs.getByte("subtype");
				int timestamp = rs.getInt("timestamp");
				short deadline = rs.getShort("deadline");
				sbyte[] senderPublicKey = rs.getBytes("sender_public_key");
				long amountNQT = rs.getLong("amount");
				long feeNQT = rs.getLong("fee");
				sbyte[] referencedTransactionFullHash = rs.getBytes("referenced_transaction_full_hash");
				int ecBlockHeight = rs.getInt("ec_block_height");
				long ecBlockId = rs.getLong("ec_block_id");
				sbyte[] signature = rs.getBytes("signature");
				long blockId = rs.getLong("block_id");
				int height = rs.getInt("height");
				long id = rs.getLong("id");
				long senderId = rs.getLong("sender_id");
				sbyte[] attachmentBytes = rs.getBytes("attachment_bytes");
				int blockTimestamp = rs.getInt("block_timestamp");
				sbyte[] fullHash = rs.getBytes("full_hash");
				sbyte version = rs.getByte("version");

				ByteBuffer buffer = null;
				if(attachmentBytes != null)
				{
					buffer = ByteBuffer.wrap(attachmentBytes);
					buffer.order(ByteOrder.LITTLE_ENDIAN);
				}

				TransactionType transactionType = TransactionType.findTransactionType(type, subtype);
				TransactionImpl.BuilderImpl builder = new TransactionImpl.BuilderImpl(version, senderPublicKey, amountNQT, feeNQT, timestamp, deadline, transactionType.parseAttachment(buffer, version)).referencedTransactionFullHash(referencedTransactionFullHash).signature(signature).blockId(blockId).height(height).id(id).senderId(senderId).blockTimestamp(blockTimestamp).fullHash(fullHash);
				if(transactionType.hasRecipient())
				{
					long recipientId = rs.getLong("recipient_id");
					if(! rs.wasNull())
					{
						builder.recipientId(recipientId);
					}
				}
				if(rs.getBoolean("has_message"))
				{
					builder.message(new Appendix.Message(buffer, version));
				}
				if(rs.getBoolean("has_encrypted_message"))
				{
					builder.encryptedMessage(new Appendix.EncryptedMessage(buffer, version));
				}
				if(rs.getBoolean("has_public_key_announcement"))
				{
					builder.publicKeyAnnouncement(new Appendix.PublicKeyAnnouncement(buffer, version));
				}
				if(rs.getBoolean("has_encrypttoself_message"))
				{
					builder.encryptToSelfMessage(new Appendix.EncryptToSelfMessage(buffer, version));
				}
				if(version > 0)
				{
					builder.ecBlockHeight(ecBlockHeight);
					builder.ecBlockId(ecBlockId);
				}

				return builder.build();

			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		internal static IList<TransactionImpl> findBlockTransactions(long blockId)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM transaction WHERE block_id = ? ORDER BY id"))
			{
				pstmt.setLong(1, blockId);
				using (ResultSet rs = pstmt.executeQuery())
				{
					IList<TransactionImpl> list = new List<>();
					while(rs.next())
					{
						list.Add(loadTransaction(con, rs));
					}
					return list;
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
			catch(NxtException.ValidationException e)
			{
				throw new Exception("Transaction already in database for block_id = " + Convert.toUnsignedLong(blockId) + " does not pass validation!", e);
			}
		}

		internal static void saveTransactions(Connection con, IList<TransactionImpl> transactions)
		{
			try
			{
				foreach (TransactionImpl transaction in transactions)
				{
					using (PreparedStatement pstmt = con.prepareStatement("INSERT INTO transaction (id, deadline, sender_public_key, " + "recipient_id, amount, fee, referenced_transaction_full_hash, height, " + "block_id, signature, timestamp, type, subtype, sender_id, attachment_bytes, " + "block_timestamp, full_hash, version, has_message, has_encrypted_message, has_public_key_announcement, " + "has_encrypttoself_message, ec_block_height, ec_block_id) " + "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)"))
					{
						int i = 0;
						pstmt.setLong(++i, transaction.Id);
						pstmt.setShort(++i, transaction.Deadline);
						pstmt.setBytes(++i, transaction.SenderPublicKey);
						DbUtils.setLongZeroToNull(pstmt, ++i, transaction.RecipientId);
						pstmt.setLong(++i, transaction.AmountNQT);
						pstmt.setLong(++i, transaction.FeeNQT);
						DbUtils.setBytes(pstmt, ++i, Convert.parseHexString(transaction.ReferencedTransactionFullHash));
						pstmt.setInt(++i, transaction.Height);
						pstmt.setLong(++i, transaction.BlockId);
						pstmt.setBytes(++i, transaction.Signature);
						pstmt.setInt(++i, transaction.Timestamp);
						pstmt.setByte(++i, transaction.Type.Type);
						pstmt.setByte(++i, transaction.Type.Subtype);
						pstmt.setLong(++i, transaction.SenderId);
						int bytesLength = 0;
						foreach (Appendix appendage in transaction.Appendages)
						{
							bytesLength += appendage.Size;
						}
						if(bytesLength == 0)
						{
							pstmt.setNull(++i, Types.VARBINARY);
						}
						else
						{
							ByteBuffer buffer = ByteBuffer.allocate(bytesLength);
							buffer.order(ByteOrder.LITTLE_ENDIAN);
							foreach (Appendix appendage in transaction.Appendages)
							{
								appendage.putBytes(buffer);
							}
							pstmt.setBytes(++i, buffer.array());
						}
						pstmt.setInt(++i, transaction.BlockTimestamp);
						pstmt.setBytes(++i, Convert.parseHexString(transaction.FullHash));
						pstmt.setByte(++i, transaction.Version);
						pstmt.setBoolean(++i, transaction.Message != null);
						pstmt.setBoolean(++i, transaction.EncryptedMessage != null);
						pstmt.setBoolean(++i, transaction.PublicKeyAnnouncement != null);
						pstmt.setBoolean(++i, transaction.EncryptToSelfMessage != null);
						pstmt.setInt(++i, transaction.ECBlockHeight);
						DbUtils.setLongZeroToNull(pstmt, ++i, transaction.ECBlockId);
						pstmt.executeUpdate();
					}
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

	}

}