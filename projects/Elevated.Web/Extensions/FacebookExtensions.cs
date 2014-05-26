using System;
using System.IO;
using System.Linq;
using System.Web;
using Facebook;

public static class FacebookExtensions
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

	private static void ClearPersistentData(this FacebookClient client, SupportedKeys key)
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

	private static string ConstructSessionVariableName(this FacebookClient client, SupportedKeys key)
	{
		return string.Format("fb_{0}_{1}", client.AppId, key);
	}

	public static string GetAccessToken(this FacebookClient client)
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

	private static string GetAccessTokenFromCode(this FacebookClient client, string code, string redirectUri = null)
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
		catch { }

		return null;
	}

	private static string GetApplicationAccessToken(this FacebookClient client)
	{
		return string.Format("{0}|{1}", client.AppId, client.AppSecret);
	}

	private static string GetCode(this FacebookClient client)
	{
		return null;
	}

	private static T GetPersistentData<T>(this FacebookClient client, SupportedKeys key)
	{
		var name = ConstructSessionVariableName(client, key);

		var context = HttpContext.Current;

		if (context.Items.Contains(key))
		{
			return context.GetItem<T>(name);
		}

		return default(T);
	}

	public static object GetSignedRequest(this FacebookClient client)
	{
		var context = HttpContext.Current;
		var signed = context.Request.Form["signed_request"];

		if (signed.IsNullOrEmpty())
		{
			signed = context.Request.QueryString["signed_request"];

			if (signed.IsNullOrEmpty())
			{
				var cookie = context.Request.Cookies[client.SignedRequestCookieName()];

				if (cookie == null || cookie.Value.IsNullOrEmpty())
				{
					return null;
				}

				signed = cookie.Value;
			}
		}

		return client.ParseSignedRequest(signed);
	}

	public static long GetUser(this FacebookClient client)
	{
		return GetUserFromAvailableData(client);
	}

	private static string GetUserAccessToken(this FacebookClient client)
	{
		var signed = (dynamic)client.GetSignedRequest();

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

	private static long GetUserFromAvailableData(this FacebookClient client)
	{
		var signed = (dynamic)client.GetSignedRequest();
		long userId;

		if (signed != null)
		{
			if (signed.user_id != null)
			{
				userId = Convert.ToInt64(signed.user_id);

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
			&& !((userId != 0) && persistedAccessToken == accessToken))
		{
			
			userId = client.GetUserFromAccessToken();

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

	private static void SetPersistentData(this FacebookClient client, SupportedKeys key, object value)
	{
		var name = ConstructSessionVariableName(client, key);

		var context = HttpContext.Current;

		context.Items[name] = value;
	}

	private static string SignedRequestCookieName(this FacebookClient client)
	{
		return "fbsr_" + client.AppId;
	}
}
