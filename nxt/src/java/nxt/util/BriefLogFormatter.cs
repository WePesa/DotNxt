using System;

namespace nxt.util
{


///
/// <summary> * A Java logging formatter that writes more compact output than the default </summary>
/// 
	public class BriefLogFormatter : Formatter
	{

	/// <summary> Format used for log messages  </summary>
		private static final ThreadLocal<MessageFormat> messageFormat = new ThreadLocal<MessageFormat>()
		{
			protected MessageFormat initialValue()
			{
				return new MessageFormat("{0,date,yyyy-MM-dd HH:mm:ss} {1}: {2}\n{3}");
			}
		}

	/// <summary> Logger instance at the top of the name tree  </summary>
		private static final Logger logger = Logger.getLogger("");

	/// <summary> singleton BriefLogFormatter instance  </summary>
		private static final BriefLogFormatter briefLogFormatter = new BriefLogFormatter();

///    
///     <summary> * Configures JDK logging to use this class for everything </summary>
///     
		static void init()
		{
			Handler[] handlers = logger.Handlers;
			foreach (Handler handler in handlers)
				handler.Formatter = briefLogFormatter;
		}

///    
///     <summary> * Format the log record as follows:
///     *
///     *     Date Level Message ExceptionTrace
///     * </summary>
///     * <param name="logRecord">       The log record </param>
///     * <returns>                      The formatted string </returns>
///     
		public string format(LogRecord logRecord)
		{
			object[] arguments = new object[4];
			arguments[0] = new DateTime(logRecord.Millis);
			arguments[1] = logRecord.Level.Name;
			arguments[2] = logRecord.Message;
			Exception exc = logRecord.Thrown;
			if(exc != null)
			{
				Writer result = new StringWriter();
				exc.printStackTrace(new PrintWriter(result));
				arguments[3] = result.ToString();
			}
			else
			{
				arguments[3] = "";
			}
			return messageFormat.get().format(arguments);
		}

		private BriefLogFormatter()
		{
		}

	}

}