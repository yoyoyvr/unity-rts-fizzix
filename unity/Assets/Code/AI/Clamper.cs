using System;

namespace AI
{
	public static class Clamper
	{
		// Smooth clamp a value within a range. As value approaches limits the slope will be
		// smoothed to create an asymptotic approach. Larger values of smoothTime will cause
		// more smoothing, and make it take longer to reach the limits.
		public static float SmoothClampMinMax(float v0, float v1, float min, float max, float dt, float smoothTime, out bool clamped)
		{
			clamped = false;
			float slope = (v1 - v0) / dt;
			float vpredicted = v0 + slope * smoothTime;
			if (vpredicted > max)
			{
				slope = (max - v0) / smoothTime;
				v1 = v0 + slope * dt;
				clamped = true;
			}
			else if (vpredicted < min)
			{
				slope = (min - v0) / smoothTime;
				v1 = v0 + slope * dt;
				clamped = true;
			}
			
			return v1;
		}

		// Smooth clamp upper limit, hard clamp lower limit.
		public static float SmoothClampMax(float v0, float v1, float min, float max, float dt, float smoothTime, out bool clamped)
		{
			clamped = false;
			float slope = (v1 - v0) / dt;
			float vpredicted = v0 + slope * smoothTime;
			if (vpredicted > max)
			{
				slope = (max - v0) / smoothTime;
				v1 = v0 + slope * dt;
				clamped = true;
			}
			else if (v1 < min)
			{
				v1 = min;
				clamped = true;
			}
			
			return v1;
		}

		// Smooth clamp an angle in degrees to a range.
		public static float SmoothClampAngleMinMax(float v0, float v1, float min, float max, float dt, float smoothTime, out bool clamped)
		{
			// Make sure angles are +/-180, not 0 to 360.
			if (v0 > 180.0f)
				v0 -= 360.0f;
			if (v1 > 180.0f)
				v1 -= 360.0f;
			return SmoothClampMinMax(v0, v1, min, max, dt, smoothTime, out clamped);
		}
	}
}
