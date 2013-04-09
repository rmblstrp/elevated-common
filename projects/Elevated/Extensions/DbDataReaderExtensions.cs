using System;
using System.Data.Common;
using System.Data.SqlClient;

public static class DbDataReaderExtensions
{
	public static T Get<T>(this DbDataReader reader, string name)
	{
		return Get<T>(reader, reader.GetOrdinal(name));
	}

	public static T Get<T>(this DbDataReader reader, int ordinal)
	{
		return reader.IsDBNull(ordinal) ? default(T) : GetField<T>(reader, ordinal);
	}

	public static T? GetNullable<T>(this DbDataReader reader, string name) where T : struct
	{
		return GetNullable<T>(reader, reader.GetOrdinal(name));
	}

	public static T? GetNullable<T>(this DbDataReader reader, int ordinal) where T : struct
	{
		return reader.IsDBNull(ordinal) ? (T?)null : GetField<T>(reader, ordinal);
	}

	private static T GetField<T>(DbDataReader reader, int ordinal)
	{
		var type = typeof(T);
		object value;

		if (type == typeof(string))
		{
			value = reader.GetString(ordinal) ?? string.Empty;
		}
		else if (type == typeof(DateTime))
		{
			value = reader.GetDateTime(ordinal);
		}
		else if (type == typeof(Guid))
		{
			value = reader.GetGuid(ordinal);
		}
		else if (type == typeof(decimal))
		{
			value = reader.GetDecimal(ordinal);
		}
		else if ((reader is SqlDataReader) && (type == typeof(DateTimeOffset)))
		{
			value = ((SqlDataReader)reader).GetDateTimeOffset(ordinal);
		}
		else
		{
			value = reader.GetValue(ordinal);
		}

		return (T)value;
	}
}
