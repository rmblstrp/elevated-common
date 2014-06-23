namespace Elevated.Utils
{
	using System;

	public static class ShortValue
	{
		public static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static ulong Decode(string value)
		{
			var len = value.Length - 1;
			ulong result = 0;

			for (int t = len; t >= 0; t--)
			{
				var bcp = (ulong)BcPow(Alphabet.Length, len - t);
				result += (ulong)Alphabet.IndexOf(value.Substring(t, 1)) * bcp;
			}

			return result;
		}

		public static string Encode(byte[] value, int startIndex = 0)
		{
			return Encode(BitConverter.ToUInt64(value, startIndex));
		}

		public static string Encode(Guid guid)
		{
			var bytes = guid.ToByteArray();

			var first = Encode(bytes, 0);
			var second = Encode(bytes, 8);

			return first + second;
		}

		public static string Encode(ulong value)
		{
			var result = string.Empty;

			for (var t = (value != 0 ? Math.Floor(Math.Log(value, Alphabet.Length)) : 0); t >= 0; t--)
			{
			  var bcp = (ulong)BcPow(Alphabet.Length, t);
			  var a = ((ulong)Math.Floor((decimal)value / (decimal)bcp)) % (ulong)Alphabet.Length;
			  result += Alphabet.Substring((int)a, 1);
			  value  = value - (a * bcp);
			}

			return result;
		}

		private static decimal BcPow(double a, double b)
		{
			return Math.Floor((decimal)Math.Pow(a, b));
		}
	}
}
