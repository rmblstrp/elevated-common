using System;
using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
	public static IList<T> ConvertItems<TItem, T>(this IList<TItem> list)
		where TItem : class
		where T : class
	{
		var result = new List<T>(list.Count);

		for (int index = 0; index < list.Count; index++)
		{
			var convert = list[index] as T;

			if (convert == null)
			{
				throw new InvalidCastException();
			}

			result.Add(convert);
		}

		return result;
	}
}
