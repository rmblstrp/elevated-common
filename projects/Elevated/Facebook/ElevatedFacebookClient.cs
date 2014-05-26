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
		private FacebookPermissionCollection permissions = new FacebookPermissionCollection();

		public FacebookPermissionCollection Permissions
		{
			get { return permissions; }
		}

		public void ParsePermissions(object response)
		{
			var list = permissions as IEnumerable<object> ?? ((dynamic)permissions).data as IEnumerable<object>;

			if (list == null)
			{
				throw new ArgumentException("Unable to obtain list of permissions", "permissions");
			}

			foreach (dynamic item in list)
			{
				permissions.Add(item.permission, Enum.Parse(typeof(FacebookPermissionStatus), item.status));
			}
		}
	}
}
