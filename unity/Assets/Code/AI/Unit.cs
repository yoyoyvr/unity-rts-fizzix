using System;
using System.Collections.Generic;

namespace AI
{
	public class Unit
	{
		[Serializable]
		public class UnitData
		{
			public Vector2D Position;
			public float Rotation;
			public float MaxSpeedKMH = 200.0f;
			public float AccelerateTimeSec = 3.0f;
			public float DecelerateTimeSec = 1.5f;
			public float WaypointArrivalToleranceM = 1f;
		}
		
		private enum UnitState
		{
			Stopped,
			Driving,
			Stopping
		}

		private readonly UnitData mData;
		
		private UnitState mState = UnitState.Stopped;
		private List<Vector2D> mWaypoints = null;
	
		private float mStoppingFromDistance = 0.0f;
		private float mStoppingFromSpeed = 0.0f;
		
		private Vector2D mPrevPosition;
		private float mPrevRotation = 0.0f;
		
		public Vector2D Position
		{
			get
			{
				return mData.Position;
			}
			
			private set
			{
				mData.Position = value;
			}
		}
		
		public float Rotation
		{
			get
			{
				return mData.Rotation;
			}
		}
		
		private Vector2D Destination
		{
			get
			{
				if ((mState == UnitState.Stopped) || (mWaypoints == null) || (mWaypoints.Count == 0))
				{
					return Position;
				}
				else
				{
					return mWaypoints[0];
				}
			}
		}
		
		public Unit() : this(new UnitData())
		{
		}
		
		public Unit(UnitData data)
		{
			mData = data;
			
			mPrevPosition = Position;
			mPrevRotation = Rotation;
		}
		
		public void Update(float deltaTime)
		{
			switch (mState)
			{
				case UnitState.Stopped:
					Stopped(deltaTime);
				break;
				
				case UnitState.Stopping:
					Stopping(deltaTime);
				break;
				
				case UnitState.Driving:
					Driving(deltaTime);
				break;
			}
		}
		
		/**
		private void UpdateWaypoints(List<Vector3> waypoints)
		{
			mWaypoints = waypoints;
			mState = UnitState.Navigate;
		}
		**/
		
		private void Stopped(float deltaTime)
		{
		}
		
		private void Stopping(float deltaTime)
		{
			// How far are we from destination?
			Vector2D delta = Destination - this.Position;
			float distance = delta.Magnitude;
			
			// "Dad, are we there yet?"
			if (distance < mData.WaypointArrivalToleranceM)
			{
				mWaypoints.RemoveAt(0);
				if (mWaypoints.Count > 0)
				{
					mState = UnitState.Driving;
				}
				else
				{
					mState = UnitState.Stopped;
				}
			}
			else
			{
				// Head directly for destination.
				Vector2D direction = delta.Normalized;
				
				float desiredSpeed = (distance / mStoppingFromDistance) * mStoppingFromSpeed * deltaTime;
				Vector2D desiredVelocity = direction * desiredSpeed;
				
				mPrevPosition = Position;
				mPrevRotation = Rotation;
				
				Position += desiredVelocity;
			}
		}
		
		private void Driving(float deltaTime)
		{
			// TODO: clean handling of 2D AI coordinates vs. 3D world
	
			// How far are we from destination?
			Vector2D delta = Destination - this.Position;
			float distance = delta.Magnitude;
			
			// How fast are we moving?
			Vector2D currentVelocity = (Position - mPrevPosition);
			float currentSpeed = currentVelocity.Magnitude / deltaTime;
			
			// Distance to stop assumes constant deceleration, so average speed is half the current speed.
			float distanceToStop = mData.DecelerateTimeSec * currentSpeed / 2.0f;
	
			// Time to start slowing down?
			if (distance <= distanceToStop)
			{
				mState = UnitState.Stopping;
				mStoppingFromSpeed = currentSpeed;
				mStoppingFromDistance = distance;
				Stopping(deltaTime);
			}
			else
			{
				// Compute max acceleration, based on smoothly ramping to max
				// speed over a number of time steps. Assumes deltaTime will be
				// same for each time step.
				float timeStepsToMaxSpeed = mData.AccelerateTimeSec / deltaTime;
				float maxSpeed = mData.MaxSpeedKMH * Constants.KMH2MetersPerSecond;
				float maxAcceleration = maxSpeed / timeStepsToMaxSpeed;
				
				// Head directly for destination.
				Vector2D direction = delta.Normalized;
	
				// Apply deltaTime here, so speed is actually speed per frame, not per second.
				float desiredSpeed = currentSpeed + maxAcceleration;
				if (desiredSpeed > maxSpeed)
					desiredSpeed = maxSpeed;
				desiredSpeed *= deltaTime;
				Vector2D desiredVelocity = direction * desiredSpeed;
				if (desiredVelocity.Magnitude > distance)
				{
					desiredVelocity = direction * distance;
				}
				
				mPrevPosition = Position;
				mPrevRotation = Rotation;
				
				Position += desiredVelocity;
			}
		}
	}
}
