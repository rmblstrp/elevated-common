namespace Elevated.Security
{
	using System;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	public static class Password
	{
		public enum HashingStrategy
		{
			SHA256 = 0,
			SHA512 = 1,
			PBKDF2 = 2
		}

		public static byte[] GenerateSalt(HashingStrategy strategy)
		{
			int length = 0;

			switch (strategy)
			{
				case HashingStrategy.SHA256: length = 32; break;
				case HashingStrategy.SHA512: length = 64; break;
				case HashingStrategy.PBKDF2: length = 64; break;
			}

			using (var random = new RNGCryptoServiceProvider())
			{
				var bytes = new byte[length];
				random.GetBytes(bytes);
				return bytes;
			}
		}

		public static byte[] GenerateHash(HashingStrategy strategy, string password, byte[] salt)
		{
			var bytes = Encoding.UTF8.GetBytes(password).Concat(salt);

			switch (strategy)
			{
				case HashingStrategy.SHA256:
					using (var sha256 = new SHA256Managed())
					{
						return sha256.ComputeHash(bytes.ToArray());
					}

				case HashingStrategy.SHA512:
					using (var sha512 = new SHA512Managed())
					{
						return sha512.ComputeHash(bytes.ToArray());
					}
				case HashingStrategy.PBKDF2:
					using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt) { IterationCount = 1000 })
					{
						return pbkdf2.GetBytes(salt.Length);
					}
			}

			throw new NotImplementedException();
		}

		public static bool AreEqual(byte[] password1, byte[] password2)
		{
			var diff = (uint)password1.Length ^ (uint)password2.Length;

			for (int index = 0; index < password1.Length && index < password2.Length; index++)
			{
				diff |= (uint)(password1[index] ^ password2[index]);
			}

			return diff == 0;
		}
	}
}
