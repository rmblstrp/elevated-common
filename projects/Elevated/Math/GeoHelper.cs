namespace Elevated.Math
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;

	static class GeoHelper
	{
		public static double DistanceBetween(double fromLatitude, double fromLongitude, double toLatutude, double toLongitude, DistanceUnit unit)
		{
			return MathHelper.ConvertDistance(DistanceBetween(fromLatitude, fromLongitude, toLatutude, toLongitude), DistanceUnit.Meters, unit);
		}

		public static double DistanceBetween(double fromLatitude, double fromLongitude, double toLatutude, double toLongitude)
		{
			// we are working in radians and not degrees
			var flatrad = MathHelper.DegreesToRadians(fromLatitude);
			var flonrad = MathHelper.DegreesToRadians(fromLongitude);

			// we are working in radians and not degrees
			var tlatrad = MathHelper.DegreesToRadians(toLatutude);
			var tlonrad = MathHelper.DegreesToRadians(toLongitude);

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
	}
}
