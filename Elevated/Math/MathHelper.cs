namespace Elevated.Math
{
	using System;

	public static class MathHelper
	{
		public static double DegreesToRadians(this double degrees)
		{
			return (degrees * Math.PI / 180);
		}

		public static double RadiansToDegrees(this double radians)
		{
			return (radians * 57.295779513082323);
		}

		public static double ConvertDistance(double value, DistanceUnit from, DistanceUnit to)
		{
			double multiplier = 1;
			double conversion = 1;

			if (DistanceUnit.Imperial.ContainsFlag(from))
			{
				switch (from)
				{
					case DistanceUnit.Inches: conversion = 0.0254; break;
					case DistanceUnit.Feet: conversion = 0.3048; break;
					case DistanceUnit.Yards: conversion = 0.9143999986; break;
					case DistanceUnit.Miles: conversion = 1609.3440006146; break;
				}

				value *= conversion;

				from = DistanceUnit.Meters;
				conversion = 1;
			}

			switch (from)
			{
				case DistanceUnit.Millimeters: multiplier = 10; break;
				case DistanceUnit.Centimeters: multiplier = 100; break;
				case DistanceUnit.Decimeters: multiplier = 1000; break;

				case DistanceUnit.Decameters: multiplier = 0.1; break;
				case DistanceUnit.Hectometers: multiplier = 0.01; break;
				case DistanceUnit.Kilometers: multiplier = 0.001; break;
			}

			switch (to)
			{
				case DistanceUnit.Millimeters: multiplier = 0.1; break;
				case DistanceUnit.Centimeters: multiplier = 0.01; break;
				case DistanceUnit.Decimeters: multiplier = 0.001; break;

				case DistanceUnit.Decameters: multiplier = 10; break;
				case DistanceUnit.Hectometers: multiplier = 100; break;
				case DistanceUnit.Kilometers: multiplier = 1000; break;

				case DistanceUnit.Inches: conversion = 39.3700787; break;
				case DistanceUnit.Feet: conversion = 3.280839895; break;
				case DistanceUnit.Yards: conversion = 1.0936133; break;
				case DistanceUnit.Miles: conversion = 0.000621371192; break;
			}

			return value * multiplier * conversion;
		}

		public static double SemicirclesToDegrees(double value)
		{
			return System.Convert.ToDouble((decimal)value * 0.00000008381903171539306640625M);
		}
	}
}
