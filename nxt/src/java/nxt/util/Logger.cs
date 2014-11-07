using System;
using System.Threading;

namespace nxt.util
{

	using Nxt = nxt.Nxt;


///
/// <summary> * Handle logging for the Nxt node server </summary>
/// 
	public sealed class Logger
	{

	/// <summary> Log event types  </summary>
		public enum Event
		{
			MESSAGE,
			EXCEPTION
		}

	/// <summary> Log levels  </summary>
		public enum Level
		{
			DEBUG,
			INFO,
			WARN,
			ERROR
		}

	/// <summary> Message listeners  </summary>
		private static readonly Listeners<string, Event> messageListeners = new Listeners<>();

	/// <summary> Exception listeners  </summary>
		private static readonly Listeners<Exception, Event> exceptionListeners = new Listeners<>();

	/// <summary> Our logger instance  </summary>
		private static readonly org.slf4j.Logger log;

	/// <summary> Enable stack traces  </summary>
		private static readonly bool enableStackTraces;

	/// <summary> Enable log traceback  </summary>
		private static readonly bool enableLogTraceback;

///    
///     <summary> * No constructor </summary>
///     
		private Logger()
		{
		}

///    
///     <summary> * Logger initialization
///     *
///     * The existing Java logging configuration will be used if the Java logger has already
///     * been initialized.  Otherwise, we will configure our own log manager and log handlers.
///     * The nxt/conf/logging-default.properties and nxt/conf/logging.properties configuration
///     * files will be used.  Entries in logging.properties will override entries in
///     * logging-default.properties. </summary>
///     
		static Logger()
		{
			string oldManager = System.getProperty("java.util.logging.manager");
			System.setProperty("java.util.logging.manager", "nxt.util.NxtLogManager");
			if(!(LogManager.LogManager is NxtLogManager))
			{
				System.setProperty("java.util.logging.manager", (oldManager != null ? oldManager : "java.util.logging.LogManager"));
			}
			if(! bool.getBoolean("nxt.doNotConfigureLogging"))
			{
				try
				{
					bool foundProperties = false;
					Properties loggingProperties = new Properties();
					using (InputStream is = ClassLoader.getSystemResourceAsStream("logging-default.properties"))
					{
						if(is != null)
						{
							loggingProperties.load(is);
							foundProperties = true;
						}
					}
					using (InputStream is = ClassLoader.getSystemResourceAsStream("logging.properties"))
					{
						if(is != null)
						{
							loggingProperties.load(is);
							foundProperties = true;
						}
					}
					if(foundProperties)
					{
						ByteArrayOutputStream outStream = new ByteArrayOutputStream();
						loggingProperties.store(outStream, "logging properties");
						ByteArrayInputStream inStream = new ByteArrayInputStream(outStream.toByteArray());
						java.util.logging.LogManager.LogManager.readConfiguration(inStream);
						inStream.close();
						outStream.close();
					}
					BriefLogFormatter.init();
				}
				catch(IOException e)
				{
					throw new Exception("Error loading logging properties", e);
				}
			}
			log = org.slf4j.LoggerFactory.getLogger(typeof(nxt.Nxt));
			enableStackTraces = Nxt.getBooleanProperty("nxt.enableStackTraces");
			enableLogTraceback = Nxt.getBooleanProperty("nxt.enableLogTraceback");
			logInfoMessage("logging enabled");
		}

		public static void init()
		{
		}

///    
///     <summary> * Logger shutdown </summary>
///     
		public static void shutdown()
		{
			if(LogManager.LogManager is NxtLogManager)
			{
				((NxtLogManager) LogManager.LogManager).nxtShutdown();
			}
		}

///    
///     <summary> * Add a message listener
///     * </summary>
///     * <param name="listener">            Listener </param>
///     * <param name="eventType">           Notification event type </param>
///     * <returns>                          TRUE if listener added </returns>
///     
		public static bool addMessageListener(Listener<string> listener, Event eventType)
		{
			return messageListeners.addListener(listener, eventType);
		}

///    
///     <summary> * Add an exception listener
///     * </summary>
///     * <param name="listener">            Listener </param>
///     * <param name="eventType">           Notification event type </param>
///     * <returns>                          TRUE if listener added </returns>
///     
		public static bool addExceptionListener(Listener<Exception> listener, Event eventType)
		{
			return exceptionListeners.addListener(listener, eventType);
		}

///    
///     <summary> * Remove a message listener
///     * </summary>
///     * <param name="listener">            Listener </param>
///     * <param name="eventType">           Notification event type </param>
///     * <returns>                          TRUE if listener removed </returns>
///     
		public static bool removeMessageListener(Listener<string> listener, Event eventType)
		{
			return messageListeners.removeListener(listener, eventType);
		}

///    
///     <summary> * Remove an exception listener
///     * </summary>
///     * <param name="listener">            Listener </param>
///     * <param name="eventType">           Notification event type </param>
///     * <returns>                          TRUE if listener removed </returns>
///     
		public static bool removeExceptionListener(Listener<Exception> listener, Event eventType)
		{
			return exceptionListeners.removeListener(listener, eventType);
		}

///    
///     <summary> * Log a message (map to INFO)
///     * </summary>
///     * <param name="message">             Message </param>
///     
		public static void logMessage(string message)
		{
			doLog(Level.INFO, message, null);
		}

///    
///     <summary> * Log an exception (map to ERROR)
///     * </summary>
///     * <param name="message">             Message </param>
///     * <param name="exc">                 Exception </param>
///     
		public static void logMessage(string message, Exception exc)
		{
			doLog(Level.ERROR, message, exc);
		}

		public static void logShutdownMessage(string message)
		{
			if(LogManager.LogManager is NxtLogManager)
			{
				logMessage(message);
			}
			else
			{
				Console.WriteLine(message);
			}
		}

		public static void logShutdownMessage(string message, Exception e)
		{
			if(LogManager.LogManager is NxtLogManager)
			{
				logMessage(message, e);
			}
			else
			{
				Console.WriteLine(message);
				Console.WriteLine(e.ToString());
			}
		}
///    
///     <summary> * Log an ERROR message
///     * </summary>
///     * <param name="message">             Message </param>
///     
		public static void logErrorMessage(string message)
		{
			doLog(Level.ERROR, message, null);
		}

///    
///     <summary> * Log an ERROR exception
///     * </summary>
///     * <param name="message">             Message </param>
///     * <param name="exc">                 Exception </param>
///     
		public static void logErrorMessage(string message, Exception exc)
		{
			doLog(Level.ERROR, message, exc);
		}

///    
///     <summary> * Log a WARNING message
///     * </summary>
///     * <param name="message">             Message </param>
///     
		public static void logWarningMessage(string message)
		{
			doLog(Level.WARN, message, null);
		}

///    
///     <summary> * Log a WARNING exception
///     * </summary>
///     * <param name="message">             Message </param>
///     * <param name="exc">                 Exception </param>
///     
		public static void logWarningMessage(string message, Exception exc)
		{
			doLog(Level.WARN, message, exc);
		}

///    
///     <summary> * Log an INFO message
///     * </summary>
///     * <param name="message">             Message </param>
///     
		public static void logInfoMessage(string message)
		{
			doLog(Level.INFO, message, null);
		}

///    
///     <summary> * Log an INFO exception
///     * </summary>
///     * <param name="message">             Message </param>
///     * <param name="exc">                 Exception </param>
///     
		public static void logInfoMessage(string message, Exception exc)
		{
			doLog(Level.INFO, message, exc);
		}

///    
///     <summary> * Log a debug message
///     * </summary>
///     * <param name="message">             Message </param>
///     
		public static void logDebugMessage(string message)
		{
			doLog(Level.DEBUG, message, null);
		}

///    
///     <summary> * Log a debug exception
///     * </summary>
///     * <param name="message">             Message </param>
///     * <param name="exc">                 Exception </param>
///     
		public static void logDebugMessage(string message, Exception exc)
		{
			doLog(Level.DEBUG, message, exc);
		}

///    
///     <summary> * Log the event
///     * </summary>
///     * <param name="level">               Level </param>
///     * <param name="message">             Message </param>
///     * <param name="exc">                 Exception </param>
///     
		private static void doLog(Level level, string message, Exception exc)
		{
			string logMessage = message;
			Exception e = exc;
		//
		// Add caller class and method if enabled
		//
			if(enableLogTraceback)
			{
				StackTraceElement caller = Thread.CurrentThread.StackTrace[3];
				string className = caller.ClassName;
				int index = className.LastIndexOf('.');
				if(index != -1)
					className = className.Substring(index+1);
				logMessage = className + "." + caller.MethodName + ": " + logMessage;
			}
		//
		// Format the stack trace if enabled
		//
			if(e != null)
			{
				if(!enableStackTraces)
				{
					logMessage = logMessage + "\n" + exc.ToString();
					e = null;
				}
			}
		//
		// Log the event
		//
			switch (level)
			{
				case Level.DEBUG:
					log.debug(logMessage, e);
					break;
				case Level.INFO:
					log.info(logMessage, e);
					break;
				case Level.WARN:
					log.warn(logMessage, e);
					break;
				case Level.ERROR:
					log.error(logMessage, e);
					break;
			}
		//
		// Notify listeners
		//
			if(exc != null)
				exceptionListeners.notify(exc, Event.EXCEPTION);
			else
				messageListeners.notify(message, Event.MESSAGE);
		}
	}

}