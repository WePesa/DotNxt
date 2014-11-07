namespace nxt.util
{


	public class CountingOutputStream : FilterOutputStream
	{

		private long count;

		public CountingOutputStream(OutputStream @out) : base(@out)
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void write(int b) throws IOException
		public override void write(int b)
		{
			count += 1;
			base.write(b);
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