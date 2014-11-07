using System;

namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using NxtException = nxt.NxtException;
	using Poll = nxt.Poll;
	using Convert = nxt.util.Convert;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_POLL;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_VOTE;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_POLL;

	public sealed class CastVote : CreateTransaction
	{

		internal static readonly CastVote instance = new CastVote();

		private CastVote() : base(new APITag[] {APITag.VS, APITag.CREATE_TRANSACTION}, "poll", "vote1", "vote2", "vote3") // hardcoded to 3 votes for testing
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string pollValue = req.getParameter("poll");

			if(pollValue == null)
			{
				return MISSING_POLL;
			}

			Poll pollData;
			int numberOfOptions = 0;
			try
			{
				pollData = Poll.getPoll(Convert.parseUnsignedLong(pollValue));
				if(pollData != null)
				{
					numberOfOptions = pollData.Options.Length;
				}
				else
				{
					return INCORRECT_POLL;
				}
			}
			catch(Exception e)
			{
				return INCORRECT_POLL;
			}

			sbyte[] vote = new sbyte[numberOfOptions];
			try
			{
				for(int i = 0; i < numberOfOptions; i++)
				{
					string voteValue = req.getParameter("vote" + i);
					if(voteValue != null)
					{
						vote[i] = Convert.ToByte(voteValue);
					}
				}
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_VOTE;
			}

			Account account = ParameterParser.getSenderAccount(req);

			Attachment attachment = new Attachment.MessagingVoteCasting(pollData.Id, vote);
			return createTransaction(req, account, attachment);

		}

	}

}