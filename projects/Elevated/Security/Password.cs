namespace Elevated.Security
{
	using System;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;

	public static class Password
	{
		public enum HashingStrategy : byte
		{
			SHA256 = 0,
			SHA512 = 1
		}

		public static byte[] GenerateSalt(HashingStrategy strategy)
		{
			int length = 0;

			switch (strategy)
			{
				case HashingStrategy.SHA256: length = 32; break;
				case HashingStrategy.SHA512: length = 64; break;
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
			}

			throw new NotImplementedException();
		}
	}
}
