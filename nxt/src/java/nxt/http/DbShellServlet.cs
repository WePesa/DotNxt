namespace nxt.http
{

	using Db = nxt.db.Db;
	using Convert = nxt.util.Convert;
	using Shell = org.h2.tools.Shell;

	using ServletException = javax.servlet.ServletException;
	using HttpServlet = javax.servlet.http.HttpServlet;
	using HttpServletRequest = javax.servlet.http.HttpServletRequest;
	using HttpServletResponse = javax.servlet.http.HttpServletResponse;

	public sealed class DbShellServlet : HttpServlet
	{

		private const string header = "<!DOCTYPE html>\n" + "<html>\n" + "<head>\n" + "    <meta charset=\"UTF-8\"/>\n" + "    <title>Nxt H2 Database Shell</title>\n" + "    <script type=\"text/javascript\">\n" + "        function submitForm(form) {\n" + "            var url = '/dbshell';\n" + "            var params = '';\n" + "            for (i = 0; i < form.elements.length; i++) {\n" + "                if (! form.elements[i].name) {\n" + "                    continue;\n" + "                }\n" + "                if (i > 0) {\n" + "                    params += '&';\n" + "                }\n" + "                params += encodeURIComponent(form.elements[i].name);\n" + "                params += '=';\n" + "                params += encodeURIComponent(form.elements[i].value);\n" + "            }\n" + "            var request = new XMLHttpRequest();\n" + "            request.open(\"POST\", url, false);\n" + "            request.setRequestHeader(\"Content-type\", \"application/x-www-form-urlencoded\");\n" + "            request.send(params);\n" + "            form.getElementsByClassName(\"result\")[0].textContent += request.responseText;\n" + "            return false;\n" + "        }\n" + "    </script>\n" + "</head>\n" + "<body>\n";

		private const string footer = "</body>\n" + "</html>\n";

		private const string form = "<form action=\"/dbshell\" method=\"POST\" onsubmit=\"return submitForm(this);\">" + "<table class=\"table\" style=\"width:90%;\">" + "<tr><td><pre class=\"result\" style=\"float:top;width:90%;\">" + "This is a database shell. Enter SQL to be evaluated, or \"help\" for help:" + "</pre></td></tr>" + "<tr><td><b>&gt;</b> <input type=\"text\" name=\"line\" style=\"width:90%;\"/></td></tr>" + "</table>" + "</form>";

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException
		protected internal override void doGet(HttpServletRequest req, HttpServletResponse resp)
		{
			resp.setHeader("Cache-Control", "no-cache, no-store, must-revalidate, private");
			resp.setHeader("Pragma", "no-cache");
			resp.setDateHeader("Expires", 0);
			if(API.allowedBotHosts != null && ! API.allowedBotHosts.contains(req.RemoteHost))
			{
				resp.sendError(HttpServletResponse.SC_FORBIDDEN);
				return;
			}

			using (PrintStream out = new PrintStream(resp.OutputStream))
			{
				out.print(header);
				out.print(form);
				out.print(footer);
			}
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException
		protected internal override void doPost(HttpServletRequest req, HttpServletResponse resp)
		{
			resp.setHeader("Cache-Control", "no-cache, no-store, must-revalidate, private");
			resp.setHeader("Pragma", "no-cache");
			resp.setDateHeader("Expires", 0);
			if(API.allowedBotHosts != null && ! API.allowedBotHosts.contains(req.RemoteHost))
			{
				resp.sendError(HttpServletResponse.SC_FORBIDDEN);
				return;
			}

			string line = Convert.nullToEmpty(req.getParameter("line"));
			using (PrintStream out = new PrintStream(resp.OutputStream))
			{
				out.println("\n> " + line);
				try
				{
					Shell shell = new Shell();
					shell.Err = out;
					shell.Out = out;
					shell.runTool(Db.Connection, "-sql", line);
				}
				catch(SQLException e)
				{
					out.println(e.ToString());
				}
			}
		}

	}

}