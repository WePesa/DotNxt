using System.IO;

namespace nxt.user
{

	using Generator = nxt.Generator;
	using Crypto = nxt.crypto.Crypto;
	using JSON = nxt.util.JSON;
	using Logger = nxt.util.Logger;
	using JSONArray = org.json.simple.JSONArray;
	using JSONObject = org.json.simple.JSONObject;
	using JSONStreamAware = org.json.simple.JSONStreamAware;

	using AsyncContext = javax.servlet.AsyncContext;
	using AsyncEvent = javax.servlet.AsyncEvent;
	using AsyncListener = javax.servlet.AsyncListener;
	using HttpServletRequest = javax.servlet.http.HttpServletRequest;
	using HttpServletResponse = javax.servlet.http.HttpServletResponse;

	internal sealed class User
	{

		private volatile string secretPhrase;
		private volatile sbyte[] publicKey;
		private volatile bool isInactive;
		private readonly string userId;
		private readonly ConcurrentLinkedQueue<JSONStreamAware> pendingResponses = new ConcurrentLinkedQueue<>();
		private AsyncContext asyncContext;

		internal User(string userId)
		{
			this.userId = userId;
		}

		internal string UserId
		{
			get
			{
				return this.userId;
			}
		}

		internal sbyte[] PublicKey
		{
			get
			{
				return publicKey;
			}
		}

		internal string SecretPhrase
		{
			get
			{
				return secretPhrase;
			}
		}

		internal bool isInactive()
		{
			get
			{
				return isInactive;
			}
			set
			{
				this.isInactive = value;
			}
		}


		internal void enqueue(JSONStreamAware response)
		{
			pendingResponses.offer(response);
		}

		internal void lockAccount()
		{
			Generator.stopForging(secretPhrase);
			secretPhrase = null;
		}

		internal long unlockAccount(string secretPhrase)
		{
			this.publicKey = Crypto.getPublicKey(secretPhrase);
			this.secretPhrase = secretPhrase;
			return Generator.startForging(secretPhrase).AccountId;
		}

		lock void processPendingResponses(HttpServletRequest req, HttpServletResponse resp)
			throws IOException;
			{
			JSONArray responses = new JSONArray();
			JSONStreamAware pendingResponse;
			@while((pendingResponse = pendingResponses.poll()) != null)
			{
				responses.add(pendingResponse);
			}
			@if(responses.size() > 0)
			{
				JSONObject combinedResponse = new JSONObject();
				combinedResponse.put("responses", responses);
				@if(asyncContext != null)
				{
					asyncContext.Response.ContentType = "text/plain; charset=UTF-8";
					using (Writer writer = asyncContext.Response.Writer)
					{
						combinedResponse.writeJSONString(writer);
					}
					asyncContext.complete();
					asyncContext = req.startAsync();
					asyncContext.addListener(new UserAsyncListener());
					asyncContext.Timeout = 5000;
				}
				else
				{
					resp.ContentType = "text/plain; charset=UTF-8";
					using (Writer writer = resp.Writer)
					{
						combinedResponse.writeJSONString(writer);
					}
				}
			}
			else
			{
				@if(asyncContext != null)
				{
					asyncContext.Response.ContentType = "text/plain; charset=UTF-8";
					using (Writer writer = asyncContext.Response.Writer)
					{
						JSON.emptyJSON.writeJSONString(writer);
					}
					asyncContext.complete();
				}
				asyncContext = req.startAsync();
				asyncContext.addListener(new UserAsyncListener());
				asyncContext.Timeout = 5000;
			}
		}

		lock void send(JSONStreamAware response)
		{
			@if(asyncContext == null)
			{

				@if(isInactive)
				{
				// user not seen recently, no responses should be collected
					return;
				}
				@if(pendingResponses.size() > 1000)
				{
					pendingResponses.clear();
				// stop collecting responses for this user
					isInactive = true;
					@if(secretPhrase == null)
					{
					// but only completely remove users that don't have unlocked accounts
						Users.remove(this);
					}
					return;
				}

				pendingResponses.offer(response);

			}
			else
			{

				JSONArray responses = new JSONArray();
				JSONStreamAware pendingResponse;
				@while((pendingResponse = pendingResponses.poll()) != null)
				{

					responses.add(pendingResponse);

				}
				responses.add(response);

				JSONObject combinedResponse = new JSONObject();
				combinedResponse.put("responses", responses);

				asyncContext.Response.ContentType = "text/plain; charset=UTF-8";

				using (Writer writer = asyncContext.Response.Writer)
				{
					combinedResponse.writeJSONString(writer);
				}
				catch(IOException e)
				{
					Logger.logMessage("Error sending response to user", e);
				}

				asyncContext.complete();
				asyncContext = null;

			}

		}


		private final class UserAsyncListener implements AsyncListener
		{

			public void onComplete(AsyncEvent asyncEvent) throws IOException
			{
			}
			public void onError(AsyncEvent asyncEvent) throws IOException
			{

				lock (User.this)
				{
					asyncContext.Response.ContentType = "text/plain; charset=UTF-8";

					using (Writer writer = asyncContext.Response.Writer)
					{
						JSON.emptyJSON.writeJSONString(writer);
					}

					asyncContext.complete();
					asyncContext = null;
				}

			}

			public void onStartAsync(AsyncEvent asyncEvent) throws IOException
			{
			}
			public void onTimeout(AsyncEvent asyncEvent) throws IOException
			{

				lock (User.this)
				{
					asyncContext.Response.ContentType = "text/plain; charset=UTF-8";

					using (Writer writer = asyncContext.Response.Writer)
					{
						JSON.emptyJSON.writeJSONString(writer);
					}

					asyncContext.complete();
					asyncContext = null;
				}

			}

		}

	}

}