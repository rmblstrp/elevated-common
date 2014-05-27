using System;
using System.IO;
using System.Linq;
using System.Web;
using Facebook;

public static class FacebookExtensions
{
	public static string CreateAlbum(this FacebookClient client, ulong facebookUserId, string name, string description)
	{
		var result = (dynamic)client.Post(string.Format("/{0}/albums"), new { name = name, message = description });

		return result.id;
	}

	public static string ExchangeToken(this FacebookClient client, string token)
	{
		dynamic post = client.Post("/oauth/access_token", new { grant_type = "fb_exchange_token", client_id = client.AppId, client_secret = client.AppSecret, fb_exchange_token = token });
		return post.access_token;
	}

	public static object GetAccount(this FacebookClient client, dynamic accounts, ulong accountId)
	{
		foreach (dynamic account in accounts)
		{
			if (account.id == accountId)
			{
				return account;
			}
		}

		return null;
	}

	public static string GetAlbumId(this FacebookClient client, ulong facebookUserId, string name)
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

	public static long GetUserFromAccessToken(this FacebookClient client)
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
		var result = (dynamic)client.Get(string.Format("/{0}/accounts", facebookUserId));

		return result.data;
	}

	public static object ListAlbums(this FacebookClient client, ulong facebookUserId)
	{
		var result = (dynamic)client.Get(string.Format("/{0}/albums", facebookUserId));

		return result.data;
	}

	public static object ListPermissions(this FacebookClient client, ulong facebookUserId)
	{
		var result = (dynamic)client.Get(string.Format("/{0}/permissions", facebookUserId));

		return result.data;
	}

	public static string PostMessage(this FacebookClient client, ulong facebookId, string message)
	{
		return client.PostMessage(facebookId.ToString(), message);
	}

	public static string PostMessage(this FacebookClient client, string facebookId, string message)
	{
		dynamic post = client.Post
		(
			string.Format("/{0}/feed", facebookId),
			new { message = message }
		);

		return post.id;
	}

	public static string PostPhoto(this FacebookClient client, string albumId, string message, string filename, byte[] photo)
	{
		using (var stream = new MemoryStream(photo))
		{
			return client.PostPhoto(albumId, message, filename, stream);
		}
	}

	public static string PostPhoto(this FacebookClient client, string albumId, string message, string filename, Stream photo)
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
}