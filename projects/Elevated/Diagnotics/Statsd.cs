namespace Elevated.Diagnotics
{
	using System;
	using System.Linq;
	using System.Net.Sockets;
	using System.Text;

	public static class Statsd
	{
		private static bool initialized = false;
		private static string host;
		private static int port;

		[ThreadStatic]
		private static UdpClient client;

		private static UdpClient Client
		{
			get
			{
				if (!initialized)
				{
					throw new Exception("Statsd must be initialized with the host and port before it can be used.");
				}

				return client ?? (client = new UdpClient(host, port));
			}
		}

		[ThreadStatic]
		private static Random random;

		private static Random Random
		{
			get { return random ?? (random = new Random()); }
		}

		public static void Init(string name)
		{
			var parts = System.Configuration.ConfigurationManager.AppSettings[name].Split(',');

			Init(parts[0], int.Parse(parts[1]));
		}

		public static void Init(string host, int port)
		{
			Statsd.host = host;
			Statsd.port = port;

			initialized = true;
		}

		public static bool Gauge(string key, int value)
		{
			return Gauge(key, value, 1.0);
		}

		public static bool Gauge(string key, int value, double sampleRate)
		{
			return Send(sampleRate, String.Format("{0}:{1:d}|g", key, value));
		}

		public static bool Timing(string key, int value)
		{
			return Timing(key, value, 1.0);
		}

		public static bool Timing(string key, int value, double sampleRate)
		{
			return Send(sampleRate, String.Format("{0}:{1:d}|ms", key, value));
		}

		public static bool Decrement(string key)
		{
			return Increment(key, -1, 1.0);
		}

		public static bool Decrement(string key, int magnitude)
		{
			return Decrement(key, magnitude, 1.0);
		}

		public static bool Decrement(string key, int magnitude, double sampleRate)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(key, magnitude, sampleRate);
		}

		public static bool Decrement(params string[] keys)
		{
			return Increment(-1, 1.0, keys);
		}

		public static bool Decrement(int magnitude, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(magnitude, 1.0, keys);
		}

		public static bool Decrement(int magnitude, double sampleRate, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(magnitude, sampleRate, keys);
		}

		public static bool Increment(string key)
		{
			return Increment(key, 1, 1.0);
		}

		public static bool Increment(string key, int magnitude)
		{
			return Increment(key, magnitude, 1.0);
		}

		public static bool Increment(string key, int magnitude, double sampleRate)
		{
			string stat = String.Format("{0}:{1}|c", key, magnitude);
			return Send(stat, sampleRate);
		}

		public static bool Increment(int magnitude, double sampleRate, params string[] keys)
		{
			return Send(sampleRate, keys.Select(key => String.Format("{0}:{1}|c", key, magnitude)).ToArray());
		}

		protected static bool Send(String stat, double sampleRate)
		{
			return Send(sampleRate, stat);
		}

		protected static bool Send(double sampleRate, params string[] stats)
		{
			var retval = false; // didn't send anything
			if (sampleRate < 1.0)
			{
				foreach (var stat in stats)
				{
					if (Random.NextDouble() <= sampleRate)
					{
						var statFormatted = String.Format("{0}|@{1:f}", stat, sampleRate);
						if (DoSend(statFormatted))
						{
							retval = true;
						}
					}
				}
			}
			else
			{
				foreach (var stat in stats)
				{
					if (DoSend(stat))
					{
						retval = true;
					}
				}
			}

			return retval;
		}

		protected static bool DoSend(string stat)
		{
			var data = Encoding.Default.GetBytes(stat + "\n");

			client.Send(data, data.Length);
			return true;
		}
	}
}
