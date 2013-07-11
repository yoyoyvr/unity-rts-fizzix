using System;

namespace AI
{
	public static class Constants
	{
		public const float MetersPerKM = 1000.0f;
		public const float SecondsPerHour = 60.0f * 60.0f;
		public const float KmPerHour2MetersPerSecond = MetersPerKM / SecondsPerHour;
		public const float MetersPerSecond2KmPerHour = SecondsPerHour / MetersPerKM;
	}
}
