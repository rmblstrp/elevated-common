using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class IEnumberableExtensions
{
	public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TValue> enumerable, System.Func<TValue , TKey> expression)
	{
		var result = new Dictionary<TKey, TValue>(enumerable.Count());

		foreach (var item in enumerable)
		{
			result.Add(expression(item), item);
		}

		return result;
	}
}
