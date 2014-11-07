using System;
using System.Collections.Generic;

namespace nxt.http
{

	using Account = nxt.Account;
	using Attachment = nxt.Attachment;
	using Constants = nxt.Constants;
	using NxtException = nxt.NxtException;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using HttpServletRequest = javax.servlet.http.HttpServletRequest;

//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_MAXNUMBEROFOPTIONS;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_MINNUMBEROFOPTIONS;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_OPTIONSAREBINARY;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_POLL_DESCRIPTION_LENGTH;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_POLL_NAME_LENGTH;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.INCORRECT_POLL_OPTION_LENGTH;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_DESCRIPTION;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_MAXNUMBEROFOPTIONS;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_MINNUMBEROFOPTIONS;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_NAME;
//JAVA TO VB & C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to .NET:
	import static nxt.http.JSONResponses.MISSING_OPTIONSAREBINARY;

	public sealed class CreatePoll : CreateTransaction
	{

		internal static readonly CreatePoll instance = new CreatePoll();

		private CreatePoll() : base(new APITag[] {APITag.VS, APITag.CREATE_TRANSACTION}, "name", "description", "minNumberOfOptions", "maxNumberOfOptions", "optionsAreBinary", "option1", "option2", "option3") // hardcoded to 3 options for testing
		{
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: JSONStreamAware processRequest(HttpServletRequest req) throws NxtException
		internal override JSONStreamAware processRequest(HttpServletRequest req)
		{

			string nameValue = req.getParameter("name");
			string descriptionValue = req.getParameter("description");
			string minNumberOfOptionsValue = req.getParameter("minNumberOfOptions");
			string maxNumberOfOptionsValue = req.getParameter("maxNumberOfOptions");
			string optionsAreBinaryValue = req.getParameter("optionsAreBinary");

			if(nameValue == null)
			{
				return MISSING_NAME;
			}
			else if(descriptionValue == null)
			{
				return MISSING_DESCRIPTION;
			}
			else if(minNumberOfOptionsValue == null)
			{
				return MISSING_MINNUMBEROFOPTIONS;
			}
			else if(maxNumberOfOptionsValue == null)
			{
				return MISSING_MAXNUMBEROFOPTIONS;
			}
			else if(optionsAreBinaryValue == null)
			{
				return MISSING_OPTIONSAREBINARY;
			}

			if(nameValue.Length > Constants.MAX_POLL_NAME_LENGTH)
			{
				return INCORRECT_POLL_NAME_LENGTH;
			}

			if(descriptionValue.Length > Constants.MAX_POLL_DESCRIPTION_LENGTH)
			{
				return INCORRECT_POLL_DESCRIPTION_LENGTH;
			}

			IList<string> options = new List<>();
			while(options.Count < 100)
			{
				string optionValue = req.getParameter("option" + options.Count);
				if(optionValue == null)
				{
					break;
				}
				if(optionValue.Length > Constants.MAX_POLL_OPTION_LENGTH)
				{
					return INCORRECT_POLL_OPTION_LENGTH;
				}
				options.Add(optionValue.Trim());
			}

			sbyte minNumberOfOptions;
			try
			{
				minNumberOfOptions = Convert.ToByte(minNumberOfOptionsValue);
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_MINNUMBEROFOPTIONS;
			}

			sbyte maxNumberOfOptions;
			try
			{
				maxNumberOfOptions = Convert.ToByte(maxNumberOfOptionsValue);
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_MAXNUMBEROFOPTIONS;
			}

			bool optionsAreBinary;
			try
			{
				optionsAreBinary = Convert.ToBoolean(optionsAreBinaryValue);
			}
			catch(NumberFormatException e)
			{
				return INCORRECT_OPTIONSAREBINARY;
			}

			Account account = ParameterParser.getSenderAccount(req);

			Attachment attachment = new Attachment.MessagingPollCreation(nameValue.Trim(), descriptionValue.Trim(), options.ToArray(), minNumberOfOptions, maxNumberOfOptions, optionsAreBinary);
			return createTransaction(req, account, attachment);

		}

	}

}