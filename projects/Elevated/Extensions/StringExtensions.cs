using System;
using System.Linq;
using System.Collections.Generic;

// Basic string extensions
public static partial class StringExtensions
{
	public static string EmptyToString(this string str, string defaultString = "")
	{
		return str.IsNullOrEmpty() ? defaultString : str;
	}

	public static bool IsNotNullOrEmpty(this string str)
	{
		return !string.IsNullOrWhiteSpace(str);
	}

	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrWhiteSpace(str);
	}

	public static string GetString(this byte[] str)
	{
		return System.Text.Encoding.UTF8.GetString(str, 0, str.Length);
	}

	public static string Truncate(this string s, int length, bool atWord = false, bool addEllipsis = false)
	{
		// Return if the string is less than or equal to the truncation length
		if (s == null || s.Length <= length)
			return s;

		// Do a simple tuncation at the desired length
		string s2 = s.Substring(0, length);

		// Truncate the string at the word
		if (atWord)
		{
			// List of characters that denote the start or a new word (add to or remove more as necessary)
			List<char> alternativeCutOffs = new List<char>() { ' ', ',', '.', '?', '/', ':', ';', '\'', '\"', '\'', '-' };

			// Get the index of the last space in the truncated string
			int lastSpace = s2.LastIndexOf(' ');

			// If the last space index isn't -1 and also the next character in the original
			// string isn't contained in the alternativeCutOffs List (which means the previous
			// truncation actually truncated at the end of a word),then shorten string to the last space
			if (lastSpace != -1 && (s.Length >= length + 1 && !alternativeCutOffs.Contains(s.ToCharArray()[length])))
				s2 = s2.Remove(lastSpace);
		}

		// Add Ellipsis if desired
		if (addEllipsis)
			s2 += "...";

		return s2;
	}
}