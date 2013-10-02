namespace Elevated.Utils
{
	using System;

	public static class ShortValue
	{
		public static readonly string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static string Encode(ulong number)
		{
			var result = string.Empty;

			for (int count = (int)Math.Floor(Math.Log(number) / Math.Log(Alphabet.Length)); count >= 0; count--)
			{
				result += Alphabet[(int)(Math.Floor(number / BcPow(Alphabet.Length, count)) % Alphabet.Length)];
			}

			return ReverseString(result);
		}

		private static double BcPow(double a, double b)
		{
			return Math.Floor(Math.Pow(a, b));
		}

		private static string ReverseString(string s)
		{
			var arr = s.ToCharArray();

			Array.Reverse(arr);

			return new string(arr);
		}
	}
}
