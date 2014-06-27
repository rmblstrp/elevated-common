namespace Elevated.Utils
{
	using System;

	public static class ShortValue
	{
		public static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		private static decimal BcPow(double a, double b)
		{
			return (decimal)Math.Floor(Math.Pow(a, b));
		}

		public static ulong Decode(string value, int pad = 0)
		{
			value = value.ReverseString();
			var len = value.Length - 1;
			ulong result = 0;

			for (int t = len; t >= 0; t--)
			{
				var bcp = (ulong)BcPow(Alphabet.Length, len - t);
				result += (ulong)Alphabet.IndexOf(value[t]) * bcp;
			}

			if (pad > 0)
			{
				result -= (ulong)BcPow(Alphabet.Length, pad);
			}

			return result;
		}

		public static string Encode(byte[] value, int startIndex = 0, int pad = 0)
		{
			return Encode(BitConverter.ToUInt64(value, startIndex), pad);
		}

		public static string Encode(Guid guid, int pad = 0)
		{
			var bytes = guid.ToByteArray();

			var first = Encode(bytes, 0, pad);
			var second = Encode(bytes, 8, pad);

			return first + second;
		}

		public static string Encode(ulong value, int pad = 0)
		{
			var result = string.Empty;

			if (pad > 0)
			{
				value += (ulong)BcPow(Alphabet.Length, pad);
			}

			for (var t = (value != 0 ? Math.Floor(Math.Log(value, Alphabet.Length)) : 0); t >= 0; t--)
			{
			  var bcp = (ulong)BcPow(Alphabet.Length, t);
			  var a = ((ulong)Math.Floor(Convert.ToDouble((decimal)value / (decimal)bcp))) % (ulong)Alphabet.Length;
			  result += Alphabet[(int)a];
			  value  = value - (a * bcp);
			}

			return result.ReverseString();
		}		

		private static string ReverseString(this string value)
		{
			char[] arr = value.ToCharArray();
			Array.Reverse(arr);
			return new string(arr);
		}
	}
}
