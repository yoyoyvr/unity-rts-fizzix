using System;

namespace AI
{
	[Serializable]
	public class Vector2D
	{
		public float x;
		public float y;
		
		public static readonly Vector2D Zero = new Vector2D(0.0f, 0.0f);
		
		public Vector2D(float x, float y)
		{
			this.x = x;
			this.y = y;
		}
		
		public Vector2D Normalized
		{
			get
			{
				float num = this.Magnitude;
				if (num > 1E-05f)
				{
					return this / num;
				}
				return Vector2D.Zero;
			}
		}
		
		public float Magnitude
		{
			get
			{
				return (float)Math.Sqrt(x * x + y * y);
			}
		}

		public static Vector2D operator +(Vector2D a, Vector2D b)
		{
			return new Vector2D(a.x + b.x, a.y + b.y);
		}

		public static Vector2D operator -(Vector2D a, Vector2D b)
		{
			return new Vector2D(a.x - b.x, a.y - b.y);
		}

		public static Vector2D operator *(Vector2D a, float b)
		{
			return new Vector2D(a.x * b, a.y * b);
		}

		public static Vector2D operator /(Vector2D a, float b)
		{
			return new Vector2D(a.x / b, a.y / b);
		}
	}
}
