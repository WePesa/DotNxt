namespace nxt.http
{

	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	internal sealed class ParameterException : NxtException
	{

		private readonly JSONStreamAware errorResponse;

		internal ParameterException(JSONStreamAware errorResponse)
		{
			this.errorResponse = errorResponse;
		}

		internal JSONStreamAware ErrorResponse
		{
			get
			{
				return errorResponse;
			}
		}

	}

}