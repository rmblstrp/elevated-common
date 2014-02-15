using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CompresionExtensions
{
	public static byte[] Deflate(this string data)
	{
		return Encoding.UTF8.GetBytes(data).Deflate();
	}

	public static byte[] Deflate(this byte[] data)
	{
		using (MemoryStream output = new MemoryStream())
		{
			using (DeflateStream gzip = new DeflateStream(output, CompressionMode.Compress))
			{
				gzip.Write(data, 0, data.Length);
			}

			return output.ToArray();
		}
	}

	public static string InflateString(this byte[] data)
	{
		return Encoding.UTF8.GetString(data.Inflate());
	}

	public static byte[] Inflate(this byte[] data)
	{
		using (MemoryStream input = new MemoryStream(data))
		{
			input.Write(data, 0, data.Length);
			input.Position = 0;

			using (DeflateStream gzip = new DeflateStream(input, CompressionMode.Decompress))
			{
				using (MemoryStream output = new MemoryStream())
				{
					var buffer = new byte[4096]; int count;

					while ((count = gzip.Read(buffer, 0, buffer.Length)) > 0)
					{
						output.Write(buffer, 0, count);
					}

					return output.ToArray();
				}
			}
		}
	}
}