using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facebook;

namespace Elevated.Facebook
{
	public class FacebookClientExtended : FacebookClient
	{
		public static FacebookPermissionCollection ParsePermissions(object response)
		{
			var permissions = new FacebookPermissionCollection();

			foreach (var item in (dynamic)response)
			{
				permissions.Add(item.permission, Enum.Parse(typeof(FacebookPermissionStatus), item.status, true));
			}

			return permissions;
		}
	}
}
