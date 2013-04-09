using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

public static class EnumExtensions
{
	public static string Description(this Enum value)
	{
		FieldInfo field = value.GetType().GetField(value.ToString());

		if (field != null)
		{
			DescriptionAttribute[] descriptions =
				(DescriptionAttribute[])field.GetCustomAttributes
					(
						typeof(DescriptionAttribute),
						false
					);

			if (descriptions != null && descriptions.Length > 0)
			{
				return descriptions[0].Description;
			}
		}

		return value.ToString();
	}
}
