namespace Elevated.Math
{
	using System;

	[Flags]
	public enum DistanceUnit : uint
	{
		Millimeters	= 0x0001,
		Centimeters	= 0x0002,
		Decimeters	= 0x0004,
		Meters		= 0x0008,
		Decameters	= 0x0010,
		Hectometers	= 0x0020,
		Kilometers	= 0x0040,

		Inches	= 0x00010000,
		Feet	= 0x00020000,
		Yards	= 0x00040000,
		Miles	= 0x00080000,

		SI =
			  Millimeters
			| Centimeters
			| Decimeters
			| Meters
			| Decameters
			| Hectometers
			| Kilometers,

		Imperial =
			  Inches
			| Feet
			| Yards
			| Miles,
	}
}
