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

		public static JSONStreamAware prepareRequest(JSONObject json)
		{
			json.put("protocol", 1);
			return prepare(json);
		}

	}

}