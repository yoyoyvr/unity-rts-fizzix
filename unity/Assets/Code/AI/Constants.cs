using System;

namespace AI
{
	public static class Constants
	{
		public const float TimeStepIntervalSeconds = (1.0f / 50.0f);
		
		public const float MetersPerKM = 1000.0f;
		public const float SecondsPerHour = 60.0f * 60.0f;
		public const float KMH2MetersPerSecond = MetersPerKM / SecondsPerHour;
		public const float MetersPerSecond2KMH = SecondsPerHour / MetersPerKM;
	}
}
