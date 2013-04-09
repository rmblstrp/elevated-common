using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class DictionaryExtensions
{
	public static TValue Get<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
	{
		if (dictionary.ContainsKey(key))
		{
			return dictionary[key];
		}

		return default(TValue);
	}
}