using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

public static class BitFlagExtensions
{
	private const string AssertionMessage = "Flag operations are only allowed on the following types: enum, byte, ushort, uint";

	public static bool ContainsFlag<T>(this T bits, T flag) where T : struct
	{
		AssertIsTypeValid<T>();
		return (bits.ToMask() & flag.ToMask()) == flag.ToMask();
	}

	public static T DisableFlag<T>(this T bits, T flag) where T : struct
	{
		AssertIsTypeValid<T>();
		return (bits.ToMask() & ~flag.ToMask()).ToType<T>();
	}

	public static T EnableFlag<T>(this T bits, T flag) where T : struct
	{
		AssertIsTypeValid<T>();
		return (bits.ToMask() | flag.ToMask()).ToType<T>();
	}

	public static T ToggleFlag<T>(this T bits, T flag) where T : struct
	{
		AssertIsTypeValid<T>();
		return (bits.ToMask() ^ flag.ToMask()).ToType<T>();
	}

	private static T ToType<T>(this uint bits)
	{
		Type type = typeof(T);
		object obj;

		if (type.BaseType == typeof(Enum))
		{
			obj = Enum.ToObject(type, bits);
		}
		else
		{
			obj = Convert.ChangeType(bits, type, Thread.CurrentThread.CurrentCulture);
		}

		return (T)obj;
	}

	private static UInt32 ToMask<T>(this T bits) where T : struct
	{
		return Convert.ToUInt32(bits);
	}

	[Conditional("Debug")]
	private static void AssertIsTypeValid<T>()
	{
		Type type = typeof(T);
		bool isValid = type == typeof(byte)
			|| type == typeof(ushort)
			|| type == typeof(uint)
			|| type.BaseType == typeof(Enum);

		Debug.Assert(isValid, AssertionMessage);
	}
}

