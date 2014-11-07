using System;

namespace nxt
{

	public abstract class NxtException : Exception
	{

		protected internal NxtException() : base()
		{
		}

		protected internal NxtException(string message) : base(message)
		{
		}

		protected internal NxtException(string message, Exception cause) : base(message, cause)
		{
		}

		protected internal NxtException(Exception cause) : base(cause)
		{
		}

		public abstract class ValidationException : NxtException
		{

			private ValidationException(string message) : base(message)
			{
			}

			private ValidationException(string message, Exception cause) : base(message, cause)
			{
			}

		}

		public class NotCurrentlyValidException : ValidationException
		{

			public NotCurrentlyValidException(string message) : base(message)
			{
			}

			public NotCurrentlyValidException(string message, Exception cause) : base(message, cause)
			{
			}

		}

		public sealed class NotYetEnabledException : NotCurrentlyValidException
		{

			public NotYetEnabledException(string message) : base(message)
			{
			}

			public NotYetEnabledException(string message, Exception throwable) : base(message, throwable)
			{
			}

		}

		public sealed class NotValidException : ValidationException
		{

			public NotValidException(string message) : base(message)
			{
			}

			public NotValidException(string message, Exception cause) : base(message, cause)
			{
			}

		}

		public sealed class StopException : Exception
		{

			public StopException(string message) : base(message)
			{
			}

			public StopException(string message, Exception cause) : base(message, cause)
			{
			}

		}

	}

}