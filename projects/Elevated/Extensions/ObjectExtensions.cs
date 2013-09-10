using System;
using System.Collections.Generic;
using System.Linq;

public static class ObjectExtensions
{
	public static string NullToString(this object obj)
	{
		return NullToString(obj, string.Empty);
	}

	public static string NullToString(this object obj, string defaultString)
	{
		return obj == null ? defaultString : obj.ToString();
	}
}
