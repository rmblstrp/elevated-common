using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

	public static class GuidExtensions
	{
		public static bool IsEmpty(this Guid guid)
		{
			return guid == Guid.Empty;
		}
	}
