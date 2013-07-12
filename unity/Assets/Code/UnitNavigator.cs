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
	private Vector3 mPreviousEulerAngles;

	void Awake()
	{
		mUnit = new Unit(m_UnitData, transform.position.x, transform.position.z, transform.eulerAngles.y);
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
		
		mPreviousEulerAngles = transform.eulerAngles;
	}
	
	private void AddWaypoint(Vector3 waypoint, bool additive)
	{
		mUnit.AddWaypoint(VectorHelper.FromVector3(waypoint), additive);
	}
	
	void FixedUpdate()
	{
		mUnit.Update(Time.deltaTime);
		
		// TODO: constraint pitch and roll
		// TODO: constrain altitude (jump height)
		// TODO: lerp (rapidly) to unit position and rotation
		
		//transform.position = VectorHelper.ToVector3(mUnit.Position, transform.position.y);
		Vector3 newPosition = VectorHelper.ToVector3(mUnit.Position, rigidbody.position.y);
		rigidbody.position = Vector3.Lerp(rigidbody.position, newPosition, 10.0f * Time.deltaTime);
		
		// Update orientation. Get y rotation from the unit; constrain x/z rotation within limits.
		Vector3 angles = transform.eulerAngles;
		bool xclamped, zclamped;
		float xrot = Clamper.SmoothClampAngleMinMax(mPreviousEulerAngles.x, angles.x, -35.0f, 35.0f, Time.deltaTime, 0.25f, out xclamped);
		float yrot = mUnit.Rotation;
		float zrot = Clamper.SmoothClampAngleMinMax(mPreviousEulerAngles.z, angles.z, -35.0f, 35.0f, Time.deltaTime, 0.25f, out zclamped);
		Vector3 newAngles = new Vector3(xrot, yrot, zrot);
		if (xclamped || zclamped)
		{
			//rigidbody.angularVelocity = Vector3.zero;
			//rigidbody.Sleep();
		}
		
		//transform.eulerAngles = newAngles;
		rigidbody.rotation = Quaternion.Lerp(rigidbody.rotation, Quaternion.Euler(newAngles), 10.0f * Time.deltaTime);
		mPreviousEulerAngles = rigidbody.rotation.eulerAngles;
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
