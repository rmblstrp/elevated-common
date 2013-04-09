using System;
using System.IO;

public static class StreamExtensions
{
	public static byte[] ToBytes(this Stream stream)
	{
		return ToBytes(stream, true);
	}

	public static byte[] ToBytes(this Stream stream, bool entireStream)
	{
		var pos = stream.Position;

		if (entireStream)
		{
			stream.Position = 0;
		}

		var data = new byte[stream.Length - stream.Position];

		stream.Read(data, 0, data.Length);
		stream.Position = pos;
		
		return data;		
	}
}

