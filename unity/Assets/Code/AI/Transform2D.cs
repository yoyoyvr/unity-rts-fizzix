using System;

namespace AI
{
	[Serializable]
	public class Transform2D
	{
		public Vector2D Position = new Vector2D(0.0f, 0.0f);
		public float Rotation = 0.0f;
		
		public Transform2D()
		{
		}
		
		public Transform2D(float x, float y, float rotation)
		{
			Position.x = x;
			Position.y = y;
			Rotation = rotation;
		}
		
		public Transform2D(Vector2D position, float rotation)
		{
			Position.x = position.x;
			Position.y = position.y;
			Rotation = rotation;
		}
		
		public Transform2D(Transform2D other)
		{
			CopyFrom(other);
		}
		
		public Transform2D Clone()
		{
			return new Transform2D(this);
		}
		
		public void CopyFrom(Transform2D other)
		{
			Position.x = other.Position.x;
			Position.y = other.Position.y;
			Rotation = other.Rotation;
		}
		
		public void Translate(Vector2D translation)
		{
			Position += translation;
		}
		
		public void LookAt(Vector2D targetPosition)
		{
			Vector2D direction = targetPosition - this.Position;
			
			// x/y, not y/x, because zero is "up" (along the y axis, not x)
			float radians = (float)Math.Atan2(direction.x, direction.y);
			this.Rotation = radians * 180.0f / (float)Math.PI;
		}
	}
}
