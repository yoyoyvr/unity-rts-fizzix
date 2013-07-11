using System;
using System.Collections.Generic;
using UnityEngine;

// An RTS navigator follows a series of waypoints.
[AddComponentMenu("Character/RTS Navigator")]
public class RTSNavigator : MonoBehaviour
{
	private enum RTSNavigatorState
	{
		Stopped,
		Stopping,
		Navigate
	}
	
	[Serializable]
	public class NavigationConstraints
	{
		public Vector3 MaxEulerAngles;
		public Vector3 MaxAngularVelocity;
	}

	[SerializeField]private float m_MaxSpeed = 10.0f;
	[SerializeField]private float m_AccelerateTime = 1.0f;
	[SerializeField]private float m_DecelerateTime = 1.0f;
	[SerializeField]private float m_ArrivalDistance = 0.1f;
	[SerializeField]private Vector3 m_Center = Vector3.zero;
	[SerializeField]private float m_Height = 0.0f;
	[SerializeField]private NavigationConstraints Constraints;
	
	private RTSNavigatorState mState = RTSNavigatorState.Stopped;
	private List<Vector3> mWaypoints = new List<Vector3>();

	private float mTimeStepsToStop = 0.0f;
	private float mMaxAcceleration = 0.0f;
	
	private float mStoppingFromDistance = 0.0f;
	private float mStoppingFromSpeed = 0.0f;
	
	private Vector3 Position
	{
		get
		{
			Vector3 pos = transform.position - m_Center;
			pos.y -= m_Height / 2.0f;
			return pos;
		}
	}
	
	private Vector3 Destination
	{
		get
		{
			if ((mState == RTSNavigatorState.Stopped) || (mWaypoints == null) || (mWaypoints.Count == 0))
			{
				return Position;
			}
			else
			{
				return mWaypoints[0];
			}
		}
	}
	
	void Start()
	{
		RTSInputController controller = GetComponent<RTSInputController>();
		if (controller == null)
		{
			Debug.LogError(name + ": no RTS controller attached, disabling", this);
			this.enabled = false;
			return;
		}
		
		controller.OnAddWaypoint += AddWaypoint;

		// Compute max acceleration, based on smoothly ramping to max
		// speed over a number of fixed time steps.
		// Note that there is no equivalent max deceleration, as we may
		// need to stop abruptly to hit a nearby waypoint.
		mTimeStepsToStop = m_AccelerateTime / Time.fixedDeltaTime;
		mMaxAcceleration = m_MaxSpeed / mTimeStepsToStop;
	}
	
	private void AddWaypoint(Vector3 waypoint, bool additive)
	{
		if (!additive)
		{
			mWaypoints.Clear();
		}
		mWaypoints.Add(waypoint);
		if (mWaypoints.Count == 1)
		{
			mState = RTSNavigatorState.Navigate;
		}
	}
	
	void FixedUpdate()
	{
		// Prevent the rigid body from sleeping.
		// TODO: rigid bodies don't wake up automatically when a collider hits them, and (worse) the collider doesn't get an OnCollisionEnter message for a sleeping rigid body
		// SOLUTION: use a kinematic (non-physics-controlled) rigidbody on the collider that should wake this guy up
		//rigidbody.WakeUp();
		
		switch (mState)
		{
			case RTSNavigatorState.Stopped:
				Stopped();
			break;
			
			case RTSNavigatorState.Stopping:
				Stopping();
			break;
			
			case RTSNavigatorState.Navigate:
				Navigate();
			break;
		}
		
		ApplyConstraints();
	}
	
	private void ApplyConstraints()
	{
		Vector3 constrainedAngularVelocity;
		if (Constrain(rigidbody.angularVelocity, Constraints.MaxAngularVelocity, out constrainedAngularVelocity))
		{
			rigidbody.angularVelocity = constrainedAngularVelocity;
		}
		
		
		Vector3 constrainedEulerAngles;
		if (ConstrainAngles(transform.eulerAngles, Constraints.MaxEulerAngles, out constrainedEulerAngles))
		{
			transform.eulerAngles = constrainedEulerAngles;
		}
	}
			
	private bool Constrain(Vector3 val, Vector3 limit, out Vector3 newval)
	{
		float x, y, z;
		bool constrained = Constrain(val.x, limit.x, out x);
		constrained = Constrain(val.y, limit.y, out y) || constrained;
		constrained = Constrain(val.z, limit.z, out z) || constrained;
		newval = (constrained ? new Vector3(x, y, z) : val);
		
		return constrained;
	}
	
	private bool Constrain(float val, float limit, out float newval)
	{
		if ((limit == 0) || (Mathf.Abs(val) <= limit))
		{
			newval = val;
			return false;
		}
		else
		{
			newval = Mathf.Clamp(val, -limit, limit);
			return true;
		}
	}
	
	private bool ConstrainAngles(Vector3 val, Vector3 limit, out Vector3 newval)
	{
		float x, y, z;
		bool constrained = ConstrainAngle(val.x, limit.x, out x);
		constrained = ConstrainAngle(val.y, limit.y, out y) || constrained;
		constrained = ConstrainAngle(val.z, limit.z, out z) || constrained;
		
		newval = (constrained ? new Vector3(x, y, z) : val);
		
		return constrained;
	}
	
	private bool ConstrainAngle(float val, float limit, out float newval)
	{
		if (limit == 0)
		{
			newval = val;
			return false;
		}
		else
		{
			float min = 360.0f - limit;
			if ((val < limit) || (val > min))
			{
				newval = val;
				return false;
			}
			else
			{
				if (val > 180.0f)
				{
					newval = min;
					return true;
				}
				else
				{
					newval = limit;
					return true;
				}
			}
		}
	}
	
	private void UpdateWaypoints(List<Vector3> waypoints)
	{
		mWaypoints = waypoints;
		mState = RTSNavigatorState.Navigate;
	}
	
	private void Stopped()
	{
	}
	
	private void Stopping()
	{
		// How far are we from destination?
		Vector3 delta = Destination - this.Position;
		delta.y = 0.0f;
		float distance = delta.magnitude;
		
		// "Dad, are we there yet?"
		if (distance < m_ArrivalDistance)
		{
			mWaypoints.RemoveAt(0);
			if (mWaypoints.Count > 0)
			{
				mState = RTSNavigatorState.Navigate;
			}
			else
			{
				mState = RTSNavigatorState.Stopped;
			}
		}
		else
		{
			// Head directly for destination.
			Vector3 direction = delta.normalized;
			
			float desiredSpeed = (distance / mStoppingFromDistance) * mStoppingFromSpeed;
			Vector3 desiredVelocity = new Vector3(direction.x * desiredSpeed, rigidbody.velocity.y, direction.z * desiredSpeed);
			//Vector3 desiredVelocity = direction * desiredSpeed;
			rigidbody.velocity = desiredVelocity;
		}
	}
	
	private void Navigate()
	{
		// TODO: clean handling of 2D AI coordinates vs. 3D world

		// How far are we from destination?
		Vector3 delta = Destination - this.Position;
		delta.y = 0.0f;
		float distance = delta.magnitude;
		
		// How fast are we moving?
		Vector3 currentVelocity = rigidbody.velocity;
		currentVelocity.y = 0.0f;
		float currentSpeed = currentVelocity.magnitude;
		
		// Distance to stop assumes constate deceleration, so average speed is half the current speed.
		float distanceToStop = m_DecelerateTime * currentSpeed / 2.0f;

		// Time to start slowing down?
		if (distance <= distanceToStop)
		{
			mState = RTSNavigatorState.Stopping;
			mStoppingFromSpeed = currentSpeed;
			mStoppingFromDistance = distance;
		}
		else
		{
			// Head directly for destination.
			Vector3 direction = delta.normalized;
			
			float desiredSpeed = Mathf.Clamp(currentSpeed + mMaxAcceleration, 0.0f, m_MaxSpeed);
			Vector3 desiredVelocity = new Vector3(direction.x * desiredSpeed, rigidbody.velocity.y, direction.z * desiredSpeed);
			rigidbody.velocity = desiredVelocity;
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
