using System;
using System.Linq;
using System.Web;
using Facebook;

public static class FacebookExtension
{
	private enum SupportedKeys
	{
		State,
		Code,
		AccessToken,
		UserId
	}

	private static string AccessToken
	{
		get { return HttpContext.Current.GetItem<string>("FacebookExtension.AccessToken", true); }
		set { HttpContext.Current.Items["FacebookExtension.AccessToken"] = value; }
	}

	public static string PostMessageToWall(this FacebookClient client, uint facebookUserId, string message)
	{
		dynamic post = client.Post
		(
			string.Format("/{0}/feed", facebookUserId),
			new { message = message }
		);

		return post.id;
	}

	public static string PostMessageToPage(this FacebookClient client, uint facebookUserId, uint facebookPageId, string message)
	{
		var token = client.AccessToken;		

		try
		{
			client.AccessToken = client.GetPageAccessToken(facebookUserId, facebookPageId);
			
			return client.PostMessageToWall(facebookUserId, message);
		}
		finally
		{
			client.AccessToken = token;
		}		
	}

	public static string GetPageAccessToken(this FacebookClient client, uint facebookUserId, uint facebookPageId)
	{
		var accounts = client.ListAccounts(facebookUserId);

		foreach (dynamic page in accounts.data)
		{
			if (page.id == facebookPageId)
			{
				return page.access_token;
			}
		}

		throw new ArgumentException(string.Format("Facebook page id ({0}) was not found", facebookPageId));
	}

	public static dynamic ListAccounts(this FacebookClient client, uint facebookUserId)
	{
		return client.Get(string.Format("/{0}/accounts", facebookUserId));
	}

	public static long GetUser(this FacebookClient client)
	{
		return GetUserFromAvailableData(client);
	}

	public static string GetAccessToken(FacebookClient client)
	{
		var token = AccessToken;

		if (token.IsNotNullOrEmpty())
		{
			return token;
		}

		AccessToken = GetApplicationAccessToken(client);
		token = GetUserAccessToken(client);
		if (token.IsNotNullOrEmpty())
		{
			AccessToken = token;
		}

		return AccessToken;
	}

	public static dynamic GetSignedRequest(this FacebookClient client)
	{
		var context = HttpContext.Current;
		var signed = context.Request.Form["signed_request"];

		if (signed.IsNullOrEmpty())
		{
			signed = context.Request.QueryString["signed_request"];

			if (signed.IsNullOrEmpty())
			{
				signed = context.Request.Cookies[client.SignedRequestCookieName()].Value;

				if (signed.IsNullOrEmpty())
				{
					return null;
				}
			}
		}

		return client.ParseSignedRequest(signed);
	}

	private static long GetUserFromAvailableData(FacebookClient client)
	{
		var signed = client.GetSignedRequest();
		long userId;

		if (signed != null)
		{
			if (signed.user_id != null)
			{
				userId = signed.user_id;

				if (userId != GetPersistentData<long>(client, SupportedKeys.UserId))
				{
					ClearAllPersistentData();
				}

				SetPersistentData(client, SupportedKeys.UserId, userId);

				return userId;
			}
		}

		userId = GetPersistentData<long>(client, SupportedKeys.UserId);
		var persistedAccessToken = GetPersistentData<string>(client, SupportedKeys.AccessToken);

		var accessToken = GetAccessToken(client);

		if (accessToken.IsNotNullOrEmpty()
			&& accessToken != GetApplicationAccessToken(client)
			&& !(userId != long.MinValue && persistedAccessToken == accessToken))
		{
			userId = GetUserFromAccessToken(client);

			if (userId != long.MinValue)
			{
				SetPersistentData(client, SupportedKeys.UserId, userId);
			}
			else
			{
				ClearAllPersistentData();
			}
		}

		return userId;
	}

	private static long GetUserFromAccessToken(FacebookClient client)
	{
		try
		{
			dynamic result = client.Get("/me");
			
			return result.id;
		}
		catch { }

		return long.MinValue;
	}

	private static string GetUserAccessToken(FacebookClient client)
	{
		var signed = GetSignedRequest(client);
		
		string token, code;

		if (signed != null)
		{
			if (signed.oauth_token != null)
			{
				token = signed.oauth_token;

				SetPersistentData(client, SupportedKeys.AccessToken, token);

				return token;
			}

			if (signed.code != null)
			{
				code = signed.code;

				if (code != null && code == GetPersistentData<string>(client, SupportedKeys.Code))
				{
					return GetPersistentData<string>(client, SupportedKeys.AccessToken);
				}

				token = GetAccessTokenFromCode(client, code, string.Empty);

				if (token.IsNotNullOrEmpty())
				{
					SetPersistentData(client, SupportedKeys.Code, code);
					SetPersistentData(client, SupportedKeys.AccessToken, token);

					return token;
				}
			}

			ClearAllPersistentData();

			return null;
		}

		code = GetCode(client);

		if (code != null && code != GetPersistentData<string>(client, SupportedKeys.Code))
		{
			token = GetAccessTokenFromCode(client, code);

			if (token.IsNotNullOrEmpty())
			{
				SetPersistentData(client, SupportedKeys.Code, code);
				SetPersistentData(client, SupportedKeys.AccessToken, token);

				return token;
			}

			ClearAllPersistentData();

			return null;
		}

		return GetPersistentData<string>(client, SupportedKeys.AccessToken);
	}

	private static string GetCode(FacebookClient client)
	{
		return null;
	}

	private static string GetAccessTokenFromCode(FacebookClient client, string code, string redirectUri = null)
	{
		if (code.IsNullOrEmpty())
		{
			return null;
		}

		if (redirectUri == null)
		{
			redirectUri = HttpContext.Current.Request.RawUrl;
		}

		try
		{
			dynamic response = client.Get("/oauth/access_token", new { redirect_uri = redirectUri, code = code });

			if (response != null && response.access_token != null)
			{
				return response.access_token;
			}
		}
		catch {  }

		return null;
	}

	private static string SignedRequestCookieName(this FacebookClient client)
	{
		return "fbsr_" + client.AppId;
	}

	private static string GetApplicationAccessToken(FacebookClient client)
	{
		return string.Format("{0}|{1}", client.AppId, client.AppSecret);
	}

	private static void SetPersistentData(FacebookClient client, SupportedKeys key, object value)
	{
		var name = ConstructSessionVariableName(client, key);

		var context = HttpContext.Current;

		context.Items[name] = value;
	}

	private static T GetPersistentData<T>(FacebookClient client, SupportedKeys key)
	{
		var name = ConstructSessionVariableName(client, key);

		var context = HttpContext.Current;

		if (context.Items.Contains(key))
		{
			return context.GetItem<T>(name);
		}

		return default(T);
	}

	private static void ClearPersistentData(FacebookClient client, SupportedKeys key)
	{
		var name = ConstructSessionVariableName(client, key);

		var context = HttpContext.Current;

		if (context.Items.Contains(name))
		{
			context.Items.Remove(name);
		}
	}

	private static void ClearAllPersistentData()
	{
		string[] supportedKeys = { "State", "Code", "AccessToken", "UserId" };

		foreach (var key in supportedKeys)
		{
			var context = HttpContext.Current;

			if (context.Items.Contains(key))
			{
				context.Items.Remove(key);
			}
		}
	}

	private static string ConstructSessionVariableName(FacebookClient client, SupportedKeys key)
	{
		return string.Format("fb_{0}_{1}", client.AppId, key);
	}
}
