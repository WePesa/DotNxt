namespace nxt.util
{

	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;


	public sealed class JSON
	{

		private JSON() //never
		{
		}

		public static readonly JSONStreamAware emptyJSON = prepare(new JSONObject());

//JAVA TO VB & C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: public static JSONStreamAware prepare(final JSONObject json)
		public static JSONStreamAware prepare(JSONObject json)
		{
//JAVA TO VB & C# CONVERTER TODO TASK: Anonymous inner classes are not converted to .NET:
//			return new JSONStreamAware()
//		{
//			private final char[] jsonChars = json.toJSONString().toCharArray();
//			@Override public void writeJSONString(Writer out) throws IOException
//			{
//				out.write(jsonChars);
//			}
//		};
		}

//JAVA TO VB & C# CONVERTER WARNING: 'final' parameters are not allowed in .NET:
//ORIGINAL LINE: public static JSONStreamAware prepareRequest(final JSONObject json)
		public static JSONStreamAware prepareRequest(JSONObject json)
		{
			json.put("protocol", 1);
			return prepare(json);
		}

	}

}