namespace nxt.db
{

	public abstract class VersionedValuesDbTable<T, V> : ValuesDbTable<T, V>
	{

		protected internal VersionedValuesDbTable(string table, DbKey.Factory<T> dbKeyFactory) : base(table, dbKeyFactory, true)
		{
		}

		public override void rollback(int height)
		{
			VersionedEntityDbTable.rollback(table, height, dbKeyFactory);
		}

		public override void trim(int height)
		{
			VersionedEntityDbTable.Trim(table, height, dbKeyFactory);
		}

	}

}