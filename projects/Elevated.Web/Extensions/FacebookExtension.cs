using System;
using System.Linq;
using System.Web;
using Facebook;

public static class FacebookExtension
{
	public static long GetUser(this FacebookClient client)
	{
		dynamic signed = client.GetSignedRequest();

		if (signed != null)
		{
			return signed.user_id == null ? 0 : Convert.ToUInt64(signed.user_id);
		}

		return 0;
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

	public static string SignedRequestCookieName(this FacebookClient client)
	{
		return "fbsr_" + client.AppId;
	}

	public static string ApplicationAccessToken(this FacebookClient client)
	{
		return string.Format("{0}|{1}", client.AppId, client.AppSecret);
	}
}
