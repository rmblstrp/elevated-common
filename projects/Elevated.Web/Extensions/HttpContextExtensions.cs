using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public static class HttpContextExtensions
{
	public static T GetItem<T>(this HttpContext context, string key)
	{
		return (T)context.Items[key];
	}

	public static T GetItem<T>(this HttpContext context, string key, T defaultValue)
	{
		var item = context.Items[key];

		if (item == null)
		{
			return defaultValue;
		}

		return (T)item;
	}

	public static T GetItem<T>(this HttpContext context, string key, bool returnDefault = true)
	{
		T item = (T)context.Items[key];

		if (item == null)
		{
			if (returnDefault)
			{
				return default(T);
			}
			else
			{
				throw new KeyNotFoundException(string.Format("Unable to locate '{0}' in the current context.", key));
			}
		}

		return item;
	}

	public static bool IsSecureConnection(this HttpContext context)
	{
		string httpHttps = context.Request.ServerVariables["HTTP_HTTPS"];

		return context.Request.IsSecureConnection || (httpHttps != null && httpHttps.Equals("on", StringComparison.CurrentCultureIgnoreCase));
	}
}
