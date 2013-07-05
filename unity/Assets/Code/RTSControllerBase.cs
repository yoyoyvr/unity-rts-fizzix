using System;
using System.Collections.Generic;
using UnityEngine;

// An RTS controller provides a destination that will be used by an RTS navigator component.
public abstract class RTSControllerBase : MonoBehaviour
{
	public event Action<List<Vector3>> OnUpdateWaypoints;
	
	private List<Vector3> mWaypoints = new List<Vector3>();

	protected void SetWaypoint(Vector3 waypoint, bool add = false)
	{
		if (!add)
		{
			mWaypoints.Clear();
		}
		mWaypoints.Add(waypoint);
		
		if (OnUpdateWaypoints != null)
		{
			OnUpdateWaypoints(mWaypoints);
		}
	}
	
	void OnDrawGizmos()
	{
		Vector3 prev = transform.position;
		foreach (Vector3 waypoint in mWaypoints)
		{
			Gizmos.DrawLine(prev, waypoint);
			Gizmos.DrawSphere(waypoint, 1.0f);
			prev = waypoint;
		}
	}
}
