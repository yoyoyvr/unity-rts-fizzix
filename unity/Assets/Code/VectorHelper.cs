using System;
using UnityEngine;

using AI;

public static class VectorHelper
{
	public static Vector2D FromVector3(Vector3 v3)
	{
		return new Vector2D(v3.x, v3.z);
	}
	
	public static Vector3 ToVector3(Vector2D v2)
	{
		return new Vector3(v2.x, 0.0f, v2.y);
	}
	
	public static Vector3 ToVector3(Vector2D v2, float altitude)
	{
		return new Vector3(v2.x, altitude, v2.y);
	}
}
