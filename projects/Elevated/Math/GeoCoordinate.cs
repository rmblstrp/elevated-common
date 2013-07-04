namespace Elevated.Math
{
	using Math = System.Math;

	public class GeoCoordinate
	{
		public double Latitude;
		public double Longitude;

		public GeoCoordinate() { }

		public GeoCoordinate(double latitude, double longitude)
		{
			Latitude = latitude;
			Longitude = longitude;
		}

		public static double DistanceBetween(GeoCoordinate from, GeoCoordinate to, DistanceUnit unit)
		{
			return MathHelper.ConvertDistance(DistanceBetween(from, to), DistanceUnit.Meters, unit);
		}
		
		public static double DistanceBetween(GeoCoordinate from, GeoCoordinate to)
		{
			// we are working in radians and not degrees
			var flatrad = MathHelper.DegreesToRadians(from.Latitude);
			var flonrad = MathHelper.DegreesToRadians(from.Longitude);

			// we are working in radians and not degrees
			var tlatrad = MathHelper.DegreesToRadians(to.Latitude);
			var tlonrad = MathHelper.DegreesToRadians(to.Longitude);

			var latRadDelta = tlatrad - flatrad;
			var lonRadDelta = tlonrad - flonrad;

			var h =
				// haversine function
				Math.Pow(Math.Sin(latRadDelta * 0.5), 2)
				+
				Math.Cos(tlatrad)
				*
				Math.Cos(flatrad)
				*
				// haversine function
				Math.Pow(Math.Sin(lonRadDelta * 0.5), 2)
				;

			// inverse haversine function
			var distance =
				Math.Asin(Math.Min(1, Math.Sqrt(h)))
				*
				// Earth radius in meters
				6378137.0
				*
				2;

			return distance;
		}

		public double DistanceBetween(GeoCoordinate coordinate)
		{
			return DistanceBetween(this, coordinate);
		}

		public double DistanceBetween(GeoCoordinate coordinate, DistanceUnit unit)
		{
			return DistanceBetween(this, coordinate, unit);
		}
	}
}
