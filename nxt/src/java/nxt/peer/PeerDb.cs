using System;
using System.Collections.Generic;

namespace nxt.peer
{

	using Db = nxt.db.Db;


	internal sealed class PeerDb
	{

		internal static IList<string> loadPeers()
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("SELECT * FROM peer"))
			{
				IList<string> peers = new List<>();
				using (ResultSet rs = pstmt.executeQuery())
				{
					while(rs.next())
					{
						peers.Add(rs.getString("address"));
					}
				}
				return peers;
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		internal static void deletePeers(ICollection<string> peers)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("DELETE FROM peer WHERE address = ?"))
			{
				foreach (string peer in peers)
				{
					pstmt.setString(1, peer);
					pstmt.executeUpdate();
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

		internal static void addPeers(ICollection<string> peers)
		{
			using (Connection con = Db.Connection, PreparedStatement pstmt = con.prepareStatement("INSERT INTO peer (address) values (?)"))
			{
				foreach (string peer in peers)
				{
					pstmt.setString(1, peer);
					pstmt.executeUpdate();
				}
			}
			catch(SQLException e)
			{
				throw new Exception(e.ToString(), e);
			}
		}

	}

}