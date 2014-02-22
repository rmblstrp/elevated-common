using System;
using System.Linq;

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

	public static string GetString(this byte[] str)
	{
		return System.Text.Encoding.UTF8.GetString(str, 0, str.Length);
	}
}