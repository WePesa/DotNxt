using System.Collections.Generic;
using System.Text;

namespace nxt.http
{

	using Convert = nxt.util.Convert;

	using ServletException = javax.servlet.ServletException;
	using HttpServlet = javax.servlet.http.HttpServlet;
	using HttpServletRequest = javax.servlet.http.HttpServletRequest;
	using HttpServletResponse = javax.servlet.http.HttpServletResponse;

	public class APITestServlet : HttpServlet
	{

		private const string header1 = "<!DOCTYPE html>\n" + "<html>\n" + "<head>\n" + "    <meta charset=\"UTF-8\"/>\n" + "    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" + "    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">" + "    <title>Nxt http API</title>\n" + "    <link href=\"css/bootstrap.min.css\" rel=\"stylesheet\" type=\"text/css\" />" + "    <style type=\"text/css\">\n" + "        table {border-collapse: collapse;}\n" + "        td {padding: 10px;}\n" + "        .result {white-space: pre; font-family: monospace; overflow: auto;}\n" + "    </style>\n" + "    <script type=\"text/javascript\">\n" + "        var apiCalls;\n" + "        function performSearch(searchStr) {\n" + "            if (searchStr == '') {\n" + "              $('.api-call-All').show();\n" + "            } else {\n" + "              $('.api-call-All').hide();\n" + "              $('.topic-link').css('font-weight', 'normal');\n" + "              for(var i=0; i<apiCalls.length; i++) {\n" + "                var apiCall = apiCalls[i];\n" + "                if (new RegExp(searchStr.toLowerCase()).test(apiCall.toLowerCase())) {\n" + "                  $('#api-call-' + apiCall).show();\n" + "                }\n" + "              }\n" + "            }\n" + "        }\n" + "        function submitForm(form) {\n" + "            var url = '/nxt';\n" + "            var params = {};\n" + "            for (i = 0; i < form.elements.length; i++) {\n" + "                if (form.elements[i].type != 'button' && form.elements[i].value && form.elements[i].value != 'submit') {\n" + "                    params[form.elements[i].name] = form.elements[i].value;\n" + "                }\n" + "            }\n" + "            $.ajax({\n" + "                url: url,\n" + "                type: 'POST',\n" + "                data: params\n" + "            })\n" + "            .done(function(result) {\n" + "                var resultStr = JSON.stringify(JSON.parse(result), null, 4);\n" + "                form.getElementsByClassName(\"result\")[0].textContent = resultStr;\n" + "            })\n" + "            .error(function() {\n" + "                alert('API not available, check if Nxt Server is running!');\n" + "            });\n" + "            if ($(form).has('.uri-link').length > 0) {\n" + "                  var uri = '/nxt?' + jQuery.param(params);\n" + "                  var html = '<a href=\"' + uri + '\" target=\"_blank\" style=\"font-size:12px;font-weight:normal;\">Open GET URL</a>';" + "                  form.getElementsByClassName(\"uri-link\")[0].innerHTML = html;\n" + "            }" + "            return false;\n" + "        }\n" + "    </script>\n" + "</head>\n" + "<body>\n" + "<div class=\"navbar navbar-default\" role=\"navigation\">" + "   <div class=\"container\" style=\"min-width: 90%;\">" + "       <div class=\"navbar-header\">" + "           <a class=\"navbar-brand\" href=\"/test\">Nxt http API</a>" + "       </div>" + "       <div class=\"navbar-collapse collapse\">" + "           <ul class=\"nav navbar-nav navbar-right\">" + "               <li><input type=\"text\" class=\"form-control\" id=\"search\" " + "                    placeholder=\"Search\" style=\"margin-top:8px;\"></li>\n" + "               <li><a href=\"https://wiki.nxtcrypto.org/wiki/Nxt_API\" target=\"_blank\" style=\"margin-left:20px;\">Wiki Docs</a></li>" + "           </ul>" + "       </div>" + "   </div>" + "</div>" + "<div class=\"container\" style=\"min-width: 90%;\">" + "<div class=\"row\">" + "  <div class=\"col-xs-12\" style=\"margin-bottom:15px;\">" + "    <div class=\"pull-right\">" + "      <a href=\"#\" id=\"navi-show-open\">Show Open</a>" + "       | " + "      <a href=\"#\" id=\"navi-show-all\" style=\"font-weight:bold;\">Show All</a>" + "    </div>" + "  </div>" + "</div>" + "<div class=\"row\" style=\"margin-bottom:15px;\">" + "  <div class=\"col-xs-4 col-sm-3 col-md-2\">" + "    <ul class=\"nav nav-pills nav-stacked\">";
		private const string header2 = "    </ul>" + "  </div> <!-- col -->" + "  <div  class=\"col-xs-8 col-sm-9 col-md-10\">" + "    <div class=\"panel-group\" id=\"accordion\">";

		private const string footer1 = "    </div> <!-- panel-group -->" + "  </div> <!-- col -->" + "</div> <!-- row -->" + "</div> <!-- container -->" + "<script src=\"js/3rdparty/jquery.js\"></script>" + "<script src=\"js/3rdparty/bootstrap.js\" type=\"text/javascript\"></script>" + "<script>" + "  $(document).ready(function() {" + "    apiCalls = [];\n";

		private const string footer2 = "    $(\".collapse-link\").click(function(event) {" + "       event.preventDefault();" + "    });" + "    $('#search').keyup(function(e) {\n" + "      if (e.keyCode == 13) {\n" + "        performSearch($(this).val());\n" + "      }\n" + "    });\n" + "    $('#navi-show-open').click(function(e) {" + "      $('.api-call-All').each(function() {" + "        if($(this).find('.panel-collapse.in').length != 0) {" + "          $(this).show();" + "        } else {" + "          $(this).hide();" + "        }" + "      });" + "      $('#navi-show-all').css('font-weight', 'normal');" + "      $(this).css('font-weight', 'bold');" + "      e.preventDefault();" + "    });" + "    $('#navi-show-all').click(function(e) {" + "      $('.api-call-All').show();" + "      $('#navi-show-open').css('font-weight', 'normal');" + "      $(this).css('font-weight', 'bold');" + "      e.preventDefault();" + "    });" + "  });" + "</script>" + "</body>\n" + "</html>\n";

		private static readonly IList<string> allRequestTypes = new List<>(APIServlet.apiRequestHandlers.Keys);
		static APITestServlet()
		{
			Collections.sort(allRequestTypes);
//JAVA TO VB & C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'entrySet' method:
			foreach (KeyValuePair<string, APIServlet.APIRequestHandler> entry in APIServlet.apiRequestHandlers.entrySet())
			{
				string requestType = entry.Key;
				Set<APITag> apiTags = entry.Value.APITags;
				foreach (APITag apiTag in apiTags)
				{
					SortedSet<string> set = requestTags.get(apiTag.name());
					if(set == null)
					{
						set = new TreeSet<>();
						requestTags.put(apiTag.name(), set);
					}
					set.add(requestType);
				}
			}
		}

		private static readonly SortedMap<string, SortedSet<string>> requestTags = new SortedDictionary<>();

		private static string buildLinks(HttpServletRequest req)
		{
			StringBuilder buf = new StringBuilder();
			string requestTag = Convert.nullToEmpty(req.getParameter("requestTag"));
			buf.Append("<li");
			if(requestTag.Equals(""))
			{
				buf.Append(" class=\"active\"");
			}
			buf.Append("><a href=\"/test\">All</a></li>");
			foreach (APITag apiTag in APITag.values())
			{
				if(requestTags.get(apiTag.name()) != null)
				{
					buf.Append("<li");
					if(requestTag.Equals(apiTag.name()))
					{
						buf.Append(" class=\"active\"");
					}
					buf.Append("><a href=\"/test?requestTag=").append(apiTag.name()).append("\">");
					buf.Append(apiTag.DisplayName).append("</a></li>").append(" ");
				}
			}
			return buf.ToString();
		}

//JAVA TO VB & C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException
		protected internal virtual void doGet(HttpServletRequest req, HttpServletResponse resp)
		{

			resp.setHeader("Cache-Control", "no-cache, no-store, must-revalidate, private");
			resp.setHeader("Pragma", "no-cache");
			resp.setDateHeader("Expires", 0);
			resp.ContentType = "text/html; charset=UTF-8";

			if(API.allowedBotHosts != null && ! API.allowedBotHosts.contains(req.RemoteHost))
			{
				resp.sendError(HttpServletResponse.SC_FORBIDDEN);
				return;
			}

			using (PrintWriter writer = resp.Writer)
			{
				writer.print(header1);
				writer.print(buildLinks(req));
				writer.print(header2);
				string requestType = Convert.nullToEmpty(req.getParameter("requestType"));
				APIServlet.APIRequestHandler requestHandler = APIServlet.apiRequestHandlers[requestType];
				StringBuilder bufJSCalls = new StringBuilder();
				if(requestHandler != null)
				{
					writer.print(form(requestType, true, requestHandler.GetType().Name, requestHandler.Parameters, requestHandler.requirePost()));
					bufJSCalls.Append("apiCalls.push(\"").append(requestType).append("\");\n");
				}
				else
				{
					string requestTag = Convert.nullToEmpty(req.getParameter("requestTag"));
					Set<string> taggedTypes = requestTags.get(requestTag);
					foreach (string type in (taggedTypes != null ? taggedTypes : allRequestTypes))
					{
						requestHandler = APIServlet.apiRequestHandlers[type];
						writer.print(form(type, false, requestHandler.GetType().Name, APIServlet.apiRequestHandlers[type].Parameters, APIServlet.apiRequestHandlers[type].requirePost()));
						bufJSCalls.Append("apiCalls.push(\"").append(type).append("\");\n");
					}
				}
				writer.print(footer1);
				writer.print(bufJSCalls.ToString());
				writer.print(footer2);
			}

		}

		private static string form(string requestType, bool singleView, string className, IList<string> parameters, bool requirePost)
		{
			StringBuilder buf = new StringBuilder();
			buf.Append("<div class=\"panel panel-default api-call-All\" ");
			buf.Append("id=\"api-call-").append(requestType).append("\">");
			buf.Append("<div class=\"panel-heading\">");
			buf.Append("<h4 class=\"panel-title\">");
			buf.Append("<a data-toggle=\"collapse\" class=\"collapse-link\" data-target=\"#collapse").append(requestType).append("\" href=\"#\">");
			buf.Append(requestType);
			buf.Append("</a>");
			buf.Append("<span style=\"float:right;font-weight:normal;font-size:14px;\">");
			if(!singleView)
			{
				buf.Append("<a href=\"/test?requestType=").append(requestType);
				buf.Append("\" target=\"_blank\" style=\"font-weight:normal;font-size:14px;color:#777;\"><span class=\"glyphicon glyphicon-new-window\"></span></a>");
				buf.Append(" &nbsp;&nbsp;");
			}
			buf.Append("<a style=\"font-weight:normal;font-size:14px;color:#777;\" href=\"/doc/");
			buf.Append(className.Replace('.','/')).append(".html\" target=\"_blank\">javadoc</a>");
			buf.Append("</span>");
			buf.Append("</h4>");
			buf.Append("</div> <!-- panel-heading -->");
			buf.Append("<div id=\"collapse").append(requestType).append("\" class=\"panel-collapse collapse");
			if(singleView)
			{
				buf.Append(" in");
			}
			buf.Append("\">");
			buf.Append("<div class=\"panel-body\">");
			buf.Append("<form action=\"/nxt\" method=\"POST\" onsubmit=\"return submitForm(this);\">");
			buf.Append("<input type=\"hidden\" name=\"requestType\" value=\"").append(requestType).append("\"/>");
			buf.Append("<div class=\"col-xs-12 col-lg-6\" style=\"width: 40%;\">");
			buf.Append("<table class=\"table\">");
			foreach (string parameter in parameters)
			{
				buf.Append("<tr>");
				buf.Append("<td>").append(parameter).append(":</td>");
				buf.Append("<td><input type=\"");
				buf.Append("secretPhrase".Equals(parameter) ? "password" : "text");
				buf.Append("\" name=\"").append(parameter).append("\" style=\"width:100%;min-width:200px;\"/></td>");
				buf.Append("</tr>");
			}
			buf.Append("<tr>");
			buf.Append("<td colspan=\"2\"><input type=\"submit\" class=\"btn btn-default\" value=\"submit\"/></td>");
			buf.Append("</tr>");
			buf.Append("</table>");
			buf.Append("</div>");
			buf.Append("<div class=\"col-xs-12 col-lg-6\" style=\"min-width: 60%;\">");
			buf.Append("<h5 style=\"margin-top:0px;\">");
			if(!requirePost)
			{
				buf.Append("<span style=\"float:right;\" class=\"uri-link\">");
				buf.Append("</span>");
			}
			else
			{
				buf.Append("<span style=\"float:right;font-size:12px;font-weight:normal;\">POST only</span>");
			}
			buf.Append("Response</h5>");
			buf.Append("<pre class=\"result\">JSON response</pre>");
			buf.Append("</div>");
			buf.Append("</form>");
			buf.Append("</div> <!-- panel-body -->");
			buf.Append("</div> <!-- panel-collapse -->");
			buf.Append("</div> <!-- panel -->");
			return buf.ToString();
		}

	}

}