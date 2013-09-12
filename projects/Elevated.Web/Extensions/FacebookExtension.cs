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

	public static object GetSignedRequest(this FacebookClient client)
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
		dynamic signed = client.GetSignedRequest();
		long userId;

		if (signed != null)
		{
			if (signed.user_id != null)
			{
				userId = signed.user_id;

				if (userId != GetPersistentData<long>(client, SupportedKeys.UserId))
				{
					ClearAllPersistentData(client);
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
				ClearAllPersistentData(client);
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

	private static void ClearAllPersistentData(FacebookClient client)
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
