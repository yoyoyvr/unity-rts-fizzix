using System;
using System.Collections.Generic;
using UnityEngine;

using AI;

// A unit navigator wraps a unit.
[AddComponentMenu("Character/Unit Navigator")]
public class UnitNavigator : MonoBehaviour
{
	[SerializeField]private Unit.UnitData m_UnitData;

	private Unit mUnit = null;

	void Awake()
	{
		mUnit = new Unit(m_UnitData, VectorHelper.FromVector3(transform.position));
	}
	
	void Start()
	{
		RTSInputController controller = GetComponent<RTSInputController>();
		if (controller == null)
		{
			Debug.LogError(name + ": no RTS controller attached, disabling", this);
			this.enabled = false;
		}
		else
		{
			controller.OnAddWaypoint += AddWaypoint;
		}
	}
	
	private void AddWaypoint(Vector3 waypoint, bool additive)
	{
		mUnit.AddWaypoint(VectorHelper.FromVector3(waypoint), additive);
	}
	
	void FixedUpdate()
	{
		mUnit.Update(Time.deltaTime);
		transform.position = VectorHelper.ToVector3(mUnit.Position, transform.position.y);
	}
	
	void OnDrawGizmos()
	{
		if (mUnit == null)
			return;
		
		Vector3 prev = transform.position;
		foreach (Vector2D waypoint2D in mUnit.Route)
		{
			Vector3 waypoint = VectorHelper.ToVector3(waypoint2D);
			Gizmos.DrawLine(prev, waypoint);
			Gizmos.DrawSphere(waypoint, 1.0f);
			prev = waypoint;
		}
	}
}
