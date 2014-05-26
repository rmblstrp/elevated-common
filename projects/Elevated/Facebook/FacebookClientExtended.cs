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
			var list = response as IEnumerable<object> ?? ((dynamic)response).data as IEnumerable<object>;

			if (list == null)
			{
				throw new ArgumentException("Unable to obtain list of permissions", "permissions");
			}

			var permissions = new FacebookPermissionCollection();

			foreach (dynamic item in list)
			{
				permissions.Add(item.permission, Enum.Parse(typeof(FacebookPermissionStatus), item.status));
			}

			return permissions;
		}
	}
}
