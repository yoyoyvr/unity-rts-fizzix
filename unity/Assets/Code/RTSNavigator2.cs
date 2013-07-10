using System;
using System.Collections.Generic;
using UnityEngine;

// An RTS navigator follows a series of waypoints.
[AddComponentMenu("Character/RTS Navigator 2")]
public class RTSNavigator2 : MonoBehaviour
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
	private List<Vector3> mWaypoints = null;

	private float mMaxAcceleration = 0.0f;
	
	private float mStoppingFromDistance = 0.0f;
	private float mStoppingFromSpeed = 0.0f;
	
	private Vector3 mPrevPosition = Vector3.zero;
	private Vector3 mPrevEulerAngles = Vector3.zero;
	
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
		RTSControllerBase controller = GetComponent<RTSControllerBase>();
		if (controller == null)
		{
			Debug.LogError(name + ": no RTS controller attached, disabling", this);
			this.enabled = false;
			return;
		}
		
		controller.OnUpdateWaypoints += UpdateWaypoints;

		// Compute max acceleration, based on smoothly ramping to max
		// speed over a number of fixed time steps.
		// Note that there is no equivalent max deceleration, as we may
		// need to stop abruptly to hit a nearby waypoint.
		float timeStepsToMaxSpeed = m_AccelerateTime / Time.fixedDeltaTime;
		mMaxAcceleration = m_MaxSpeed / timeStepsToMaxSpeed;
		
		mPrevPosition = transform.position;
		mPrevEulerAngles = transform.eulerAngles;
	}
	
	void FixedUpdate()
	{
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
			
			Vector3 currentVelocity = transform.position - mPrevPosition;
			float desiredSpeed = (distance / mStoppingFromDistance) * mStoppingFromSpeed * Time.deltaTime;
			Vector3 desiredVelocity = new Vector3(direction.x * desiredSpeed, currentVelocity.y, direction.z * desiredSpeed);
			
			mPrevPosition = transform.position;
			mPrevEulerAngles = transform.eulerAngles;
			
			transform.Translate(desiredVelocity);
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
		Vector3 currentVelocity = (transform.position - mPrevPosition);
		float currentVelocityY = currentVelocity.y;
		currentVelocity.y = 0.0f;
		float currentSpeed = currentVelocity.magnitude / Time.deltaTime;
		
		// Distance to stop assumes constant deceleration, so average speed is half the current speed.
		float distanceToStop = m_DecelerateTime * currentSpeed / 2.0f;

		// Time to start slowing down?
		if (distance <= distanceToStop)
		{
			mState = RTSNavigatorState.Stopping;
			mStoppingFromSpeed = currentSpeed;
			mStoppingFromDistance = distance;
			Stopping();
		}
		else
		{
			// Head directly for destination.
			Vector3 direction = delta.normalized;

			// Apply deltaTime here, so speed is actually speed per frame, not per second.
			float desiredSpeed = Mathf.Clamp(currentSpeed + mMaxAcceleration, 0.0f, m_MaxSpeed) * Time.deltaTime;
			Vector3 desiredVelocity = new Vector3(direction.x * desiredSpeed, currentVelocityY, direction.z * desiredSpeed);
			if (desiredVelocity.magnitude > distance)
			{
				desiredVelocity *= distance / desiredVelocity.magnitude;
			}
			
			mPrevPosition = transform.position;
			mPrevEulerAngles = transform.eulerAngles;
			
			transform.Translate(desiredVelocity);
		}
	}
}
