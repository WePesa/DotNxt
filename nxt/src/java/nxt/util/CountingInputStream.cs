namespace nxt.util
{


	public class CountingInputStream : FilterInputStream
	{

		private long count;

		public CountingInputStream(InputStream @in) : base(@in)
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read() throws IOException
		public override int read()
		{
			int read = base.read();
			if(read >= 0)
			{
				count += 1;
			}
			return read;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read(byte[] b, int off, int len) throws IOException
		public override int read(sbyte[] b, int off, int len)
		{
			int read = base.read(b, off, len);
			if(read >= 0)
			{
				count += read;
			}
			return read;
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public long skip(long n) throws IOException
		public override long skip(long n)
		{
			long skipped = base.skip(n);
			if(skipped >= 0)
			{
				count += skipped;
			}
			return skipped;
		}

		public virtual long Count
		{
			get
			{
				return count;
			}
		}

	}

}