using System;
using System.Collections.Generic;

namespace AI
{
	[Serializable]	// so details can be seen in debug Inspector
	public class Unit
	{
		[Serializable]
		public class UnitData
		{
			public Transform2D Transform = new Transform2D();
			public float MaxSpeedMetersPerSec = 60.0f;
			public float AccelerateTimeSec = 3.0f;
			public float DecelerateTimeSec = 1.5f;
			public float WaypointArrivalToleranceM = 1f;
			public float MaxAngularSpeedDegreesPerSec = 180.0f;
		}
		
		private enum UnitState
		{
			Stopped,
			Driving,
			Stopping
		}

		private readonly UnitData mData;
		
		private UnitState mState = UnitState.Stopped;
		private List<Vector2D> mWaypoints = new List<Vector2D>();
	
		private float mStoppingFromDistance = 0.0f;
		private float mStoppingFromSpeed = 0.0f;
		
		private Transform2D mPrevTransform = new Transform2D();
		
		//public Unit() : this(new UnitData())
		//{
		//}
		
		public Unit(UnitData data, float x, float y, float rotation)
		{
			mData = data;
			Teleport(new Transform2D(x, y, rotation));
		}
		
		public Vector2D Position
		{
			get
			{
				return mData.Transform.Position;
			}
		}
		
		public float Rotation
		{
			get
			{
				return mData.Transform.Rotation;
			}
		}
		
		public Vector2D Destination
		{
			get
			{
				if (mWaypoints.Count == 0)
				{
					return mData.Transform.Position;
				}
				else
				{
					return mWaypoints[0];
				}
			}
		}
		
		public IEnumerable<Vector2D> Route
		{
			get { return mWaypoints; }
		}
		
		public void Update(float deltaTime)
		{
			Transform2D currentTransform = mData.Transform.Clone();
			
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
			
			mPrevTransform.CopyFrom(currentTransform);
		}
		
		public void AddWaypoint(Vector2D waypoint, bool additive)
		{
			if (!additive)
			{
				mWaypoints.Clear();
			}
			mWaypoints.Add(waypoint);
			if (mWaypoints.Count == 1)
			{
				mState = UnitState.Driving;
			}
		}
		
		private void Stopped(float deltaTime)
		{
		}
		
		private void Stopping(float deltaTime)
		{
			// How far are we from destination?
			Vector2D delta = Destination - mData.Transform.Position;
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
				Vector2D updateDelta = direction * desiredSpeed;
				
				mData.Transform.Translate(updateDelta);
			}
		}
		
		private void Driving(float deltaTime)
		{
			// TODO: clean handling of 2D AI coordinates vs. 3D world
	
			// How far are we from destination?
			Vector2D delta = Destination - mData.Transform.Position;
			float distance = delta.Magnitude;
			
			// How fast are we moving?
			Vector2D currentVelocity = (mData.Transform.Position - mPrevTransform.Position);
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
				float maxAcceleration = mData.MaxSpeedMetersPerSec / timeStepsToMaxSpeed;
				
				// Head directly for destination.
				Vector2D direction = delta.Normalized;
	
				// Apply deltaTime here, so speed is actually speed per frame, not per second.
				float desiredSpeed = currentSpeed + maxAcceleration;
				if (desiredSpeed > mData.MaxSpeedMetersPerSec)
					desiredSpeed = mData.MaxSpeedMetersPerSec;
				Vector2D updateDelta = direction * (desiredSpeed * deltaTime);
				if (updateDelta.Magnitude > distance)
				{
					updateDelta = direction * distance;
				}
				
				mData.Transform.Translate(updateDelta);
				mData.Transform.LookAt(Destination);
			}
		}
		
		private void Teleport(Transform2D transform)
		{
			mData.Transform.CopyFrom(transform);
			mPrevTransform.CopyFrom(transform);
		}
	}
}
