namespace Elevated.Diagnotics
{
	using System;
	using System.Linq;
	using System.Net.Sockets;
	using System.Text;

	public class Statsd : IDisposable
	{
		private Random random = new Random();
		private UdpClient client;

		private double sampleRate;
		public double SampleRate
		{
			get { return sampleRate; }
			set { sampleRate = value; }
		}

		public Statsd(string name)
		{
			var parts = System.Configuration.ConfigurationManager.AppSettings[name].Split(',');

			Init(parts[0], int.Parse(parts[1]));
		}

		public Statsd(string host, int port)
		{
			Init(host, port);
		}

		private void Init(string host, int port)
		{
			client = new UdpClient(host, port);
		}

		public bool Gauge(string key, int value)
		{
			return Gauge(key, value, sampleRate);
		}

		public bool Gauge(string key, int value, double sampleRate)
		{
			return Send(sampleRate, String.Format("{0}:{1:d}|g", key, value));
		}

		public bool Timing(string key, int value)
		{
			return Timing(key, value, sampleRate);
		}

		public bool Timing(string key, int value, double sampleRate)
		{
			return Send(sampleRate, String.Format("{0}:{1:d}|ms", key, value));
		}

		public bool Decrement(string key)
		{
			return Increment(key, -1, sampleRate);
		}

		public bool Decrement(string key, int magnitude)
		{
			return Decrement(key, magnitude, sampleRate);
		}

		public bool Decrement(string key, int magnitude, double sampleRate)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(key, magnitude, sampleRate);
		}

		public bool Decrement(params string[] keys)
		{
			return Increment(-1, sampleRate, keys);
		}

		public bool Decrement(int magnitude, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(magnitude, sampleRate, keys);
		}

		public bool Decrement(int magnitude, double sampleRate, params string[] keys)
		{
			magnitude = magnitude < 0 ? magnitude : -magnitude;
			return Increment(magnitude, sampleRate, keys);
		}

		public bool Increment(string key)
		{
			return Increment(key, 1, sampleRate);
		}

		public bool Increment(string key, int magnitude)
		{
			return Increment(key, magnitude, sampleRate);
		}

		public bool Increment(string key, int magnitude, double sampleRate)
		{
			string stat = String.Format("{0}:{1}|c", key, magnitude);
			return Send(stat, sampleRate);
		}

		public bool Increment(int magnitude, double sampleRate, params string[] keys)
		{
			return Send(sampleRate, keys.Select(key => String.Format("{0}:{1}|c", key, magnitude)).ToArray());
		}

		private bool Send(String stat, double sampleRate)
		{
			return Send(sampleRate, stat);
		}

		private bool Send(double sampleRate, params string[] stats)
		{
			var retval = false; // didn't send anything
			if (sampleRate < 1.0)
			{
				foreach (var stat in stats)
				{
					if (random.NextDouble() <= sampleRate)
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

		private bool DoSend(string stat)
		{
			var data = Encoding.Default.GetBytes(stat + "\n");

			client.Send(data, data.Length);
			return true;
		}

		public void Dispose()
		{
			if (client != null)
			{
				client.Close();
			}
		}
	}
}
