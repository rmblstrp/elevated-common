namespace Elevated
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Linq;

	public static class Configuration
	{
		public static T EnsureSetting<T>(string key, ref T obj)
		{
			if (obj == null)
			{
				if (!ConfigurationManager.AppSettings.AllKeys.Contains(key))
				{
					throw new KeyNotFoundException(string.Format("'{0}' was not found", key));
				}

				var generic = typeof(T);
				var type = generic.FullName.StartsWith("System.Nullable") ? generic.GetGenericArguments()[0] : typeof(T);

				var setting = ConfigurationManager.AppSettings[key];
				obj = (T)Convert.ChangeType(setting, type);
			}

			return obj;
		}
	}
}
