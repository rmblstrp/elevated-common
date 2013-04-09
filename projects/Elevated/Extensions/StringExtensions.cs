using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

// Basic string extensions
public static partial class StringExtensions
{
	public static string EmptyToString(this string str, string defaultString)
	{
		return str.IsNullOrEmpty() ? defaultString : str;
	}

	public static bool IsNotNullOrEmpty(this string str)
	{
		return !string.IsNullOrEmpty(str);
	}

	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrEmpty(str);
	}
}