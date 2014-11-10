using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace nxt.util
{

	using Nxt = nxt.Nxt;


	public sealed class ThreadPool
	{

		private static ScheduledExecutorService scheduledThreadPool;
		private static IDictionary<Runnable, long?> backgroundJobs = new Dictionary<>();
		private static IList<Runnable> beforeStartJobs = new List<>();
		private static IList<Runnable> lastBeforeStartJobs = new List<>();
		private static IList<Runnable> afterStartJobs = new List<>();

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void runBeforeStart(Runnable runnable, bool runLast)
		{
			if(scheduledThreadPool != null)
			{
				throw new InvalidOperationException("Executor service already started");
			}
			if(runLast)
			{
				lastBeforeStartJobs.Add(runnable);
			}
			else
			{
				beforeStartJobs.Add(runnable);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void runAfterStart(Runnable runnable)
		{
			afterStartJobs.Add(runnable);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void scheduleThread(string name, Runnable runnable, int delay)
		{
			scheduleThread(name, runnable, delay, TimeUnit.SECONDS);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void scheduleThread(string name, Runnable runnable, int delay, TimeUnit timeUnit)
		{
			if(scheduledThreadPool != null)
			{
				throw new InvalidOperationException("Executor service already started, no new jobs accepted");
			}
			if(! Nxt.getBooleanProperty("nxt.disable" + name + "Thread"))
			{
				backgroundJobs.Add(runnable, timeUnit.toMillis(delay));
			}
			else
			{
				Logger.logMessage("Will not run " + name + " thread");
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void start(int timeMultiplier)
		{
			if(scheduledThreadPool != null)
			{
				throw new InvalidOperationException("Executor service already started");
			}

			Logger.logDebugMessage("Running " + beforeStartJobs.Count + " tasks...");
			runAll(beforeStartJobs);
			beforeStartJobs = null;

			Logger.logDebugMessage("Running " + lastBeforeStartJobs.Count + " final tasks...");
			runAll(lastBeforeStartJobs);
			lastBeforeStartJobs = null;

			Logger.logDebugMessage("Starting " + backgroundJobs.Count + " background jobs");
			scheduledThreadPool = Executors.newScheduledThreadPool(backgroundJobs.Count);
//JAVA TO VB & C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'entrySet' method:
			foreach (KeyValuePair<Runnable, long?> entry in backgroundJobs.entrySet())
			{
				scheduledThreadPool.scheduleWithFixedDelay(entry.Key, 0, Math.Max(entry.Value / timeMultiplier, 1), TimeUnit.MILLISECONDS);
			}
			backgroundJobs = null;

			Logger.logDebugMessage("Starting " + afterStartJobs.Count + " delayed tasks");
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//			Thread thread = new Thread()
//		{
//			@Override public void run()
//			{
//				runAll(afterStartJobs);
//				afterStartJobs = null;
//			}
//		};
			thread.Daemon = true;
			thread.start();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void shutdown()
		{
			if(scheduledThreadPool != null)
			{
				Logger.logShutdownMessage("Stopping background jobs...");
				shutdownExecutor(scheduledThreadPool);
				scheduledThreadPool = null;
				Logger.logShutdownMessage("...Done");
			}
		}

		public static void shutdownExecutor(ExecutorService executor)
		{
			executor.shutdown();
			try
			{
				executor.awaitTermination(10, TimeUnit.SECONDS);
			}
			catch(InterruptedException e)
			{
				Thread.CurrentThread.interrupt();
			}
			if(! executor.Terminated)
			{
				Logger.logShutdownMessage("some threads didn't terminate, forcing shutdown");
				executor.shutdownNow();
			}
		}

		private static void runAll(IList<Runnable> jobs)
		{
			IList<Thread> threads = new List<>();
//JAVA TO VB & C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final StringBuffer errors = new StringBuffer();
			StringBuilder errors = new StringBuilder();
			foreach (Runnable runnable in jobs)
			{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//				Thread thread = new Thread()
//			{
//				@Override public void run()
//				{
//					try
//					{
//						runnable.run();
//					}
//					catch (Throwable t)
//					{
//						errors.append(t.getMessage()).append('\n');
//						throw t;
//					}
//				}
//			};
				thread.Daemon = true;
				thread.start();
				threads.Add(thread);
			}
			foreach (Thread thread in threads)
			{
				try
				{
					thread.join();
				}
				catch(InterruptedException e)
				{
					Thread.CurrentThread.interrupt();
				}
			}
			if(errors.Length > 0)
			{
				throw new Exception("Errors running startup tasks:\n" + errors.ToString());
			}
		}

		private ThreadPool() //never
		{
		}

	}

}