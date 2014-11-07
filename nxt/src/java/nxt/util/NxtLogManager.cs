namespace nxt.util
{


///
/// <summary> * Java LogManager extension for use with Nxt </summary>
/// 
	public class NxtLogManager : LogManager
	{

	/// <summary> Logging reconfiguration in progress  </summary>
		private volatile bool loggingReconfiguration = false;

///    
///     <summary> * Create the Nxt log manager
///     *
///     * We will let the Java LogManager create its shutdown hook so that the
///     * shutdown context will be set up properly.  However, we will intercept
///     * the reset() method so we can delay the actual shutdown until we are
///     * done terminating the Nxt processes. </summary>
///     
		public NxtLogManager() : base()
		{
		}

///    
///     <summary> * Reconfigure logging support using a configuration file
///     * </summary>
///     * <param name="inStream">            Input stream </param>
///     * <exception cref="IOException">         Error reading input stream </exception>
///     * <exception cref="SecurityException">   Caller does not have LoggingPermission("control") </exception>
///     
//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void readConfiguration(InputStream inStream) throws IOException, SecurityException
		public override void readConfiguration(InputStream inStream)
		{
			loggingReconfiguration = true;
			base.readConfiguration(inStream);
			loggingReconfiguration = false;
		}

///    
///     <summary> * Reset the log handlers
///     *
///     * This method is called to reset the log handlers.  We will forward the
///     * call during logging reconfiguration but will ignore it otherwise.
///     * This allows us to continue to use logging facilities during Nxt shutdown. </summary>
///     
		public override void reset()
		{
			if(loggingReconfiguration)
				base.reset();
		}

///    
///     <summary> * Nxt shutdown is now complete, so call LogManager.reset() to terminate
///     * the log handlers. </summary>
///     
		internal virtual void nxtShutdown()
		{
			base.reset();
		}
	}

}