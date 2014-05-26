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

	public static string CreateAlbum(this FacebookClient client, ulong facebookUserId, string name, string description)
	{
		var result = (dynamic)client.Post(string.Format("/{0}/albums"), new { name = name, message = description });

		return result.id;
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

	public static string ExchangeToken(this FacebookClient client, string token)
	{
		dynamic post = client.Post("/oauth/access_token", new { grant_type = "fb_exchange_token", client_id = client.AppId, client_secret = client.AppSecret, fb_exchange_token = token });
		return post.access_token;
	}

	public static string GetAlbumId(this FacebookClient client, string name, ulong facebookUserId)
	{
		var list = (dynamic)client.Get(string.Format("/{0}/albums?fields=name,id", facebookUserId));

		name = name.ToLower();

		foreach (var album in list.data)
		{
			if (name == ((string)album.name).ToLower())
			{
				return album.id;
			}
		}

		return null;
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

	public static string GetPageAccessToken(this FacebookClient client, ulong facebookUserId, ulong facebookPageId)
	{
		var accounts = (dynamic)client.ListAccounts(facebookUserId);

		foreach (dynamic page in accounts.data)
		{
			if (page.id == facebookPageId)
			{
				return page.access_token;
			}
		}

		throw new ArgumentException(string.Format("Facebook page id ({0}) was not found", facebookPageId));
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

	private static long GetUserFromAccessToken(this FacebookClient client)
	{
		try
		{
			dynamic result = client.Get("/me");

			return result.id;
		}
		catch { }

		return long.MinValue;
	}

	public static object ListAccounts(this FacebookClient client, ulong facebookUserId)
	{
		return client.Get(string.Format("/{0}/accounts", facebookUserId));
	}

	public static object ListAlbums(this FacebookClient client, ulong facebookUserId)
	{
		return client.Get(string.Format("/{0}/albums", facebookUserId));
	}

	public static object ListPermissions(this FacebookClient client, ulong facebookUserId)
	{
		return client.Get(string.Format("/{0}/permissions", facebookUserId));
	}

	public static string PostMessageToWall(this FacebookClient client, ulong facebookUserId, string message)
	{
		dynamic post = client.Post
		(
			string.Format("/{0}/feed", facebookUserId),
			new { message = message }
		);


		return post.id;
	}

	public static string PostMessageToPage(this FacebookClient client, ulong facebookUserId, ulong facebookPageId, string message)
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

	public static string PostPhotoToAlbum(this FacebookClient client, string albumId, string message, string filename, byte[] photo)
	{
		using (var stream = new MemoryStream(photo))
		{
			return client.PostPhotoToAlbum(albumId, message, filename, stream);
		}
	}

	public static string PostPhotoToAlbum(this FacebookClient client, string albumId, string message, string filename, Stream photo)
	{
		var parameters = new
		{
			message = message,
			file = new FacebookMediaStream()
			{
				ContentType = "image/jpeg",
				FileName = filename
			}.SetValue(photo)
		};

		dynamic post = client.Post(string.Format("/{0}/photos", albumId), parameters);

		return post.id;
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
