using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class DateTimeExtensions
{
	public static byte[] ToBytes(this DateTime value)
	{
		return BitConverter.GetBytes(value.Ticks);
	}

	public static DateTime ToDateTime(this byte[] value, int index = 0)
	{
		return new DateTime(BitConverter.ToInt64(value, index));
	}

	public static DateTime ToDateTime(this byte[] value, DateTimeKind kind = DateTimeKind.Unspecified)
	{
		return value.ToDateTime(0, kind);
	}

	public static DateTime ToDateTime(this byte[] value, int index, DateTimeKind kind = DateTimeKind.Unspecified)
	{
		return new DateTime(BitConverter.ToInt64(value, index), kind);
	}
}