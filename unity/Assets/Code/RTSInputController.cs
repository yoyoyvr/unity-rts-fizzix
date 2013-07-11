using System;
using System.Collections.Generic;
using UnityEngine;

// An RTS input controller provides a destination based on user input.
[AddComponentMenu("Character/RTS Input Controller")]
public class RTSInputController : MonoBehaviour
{
	public delegate void AddWaypointHandler(Vector3 waypoint, bool additive);
	public event AddWaypointHandler OnAddWaypoint;
	
	private static List<GameObject> sSelected = new List<GameObject>();
	
	private bool mClickInProgress = false;
	private Vector2 mClickPoint = Vector2.zero;
	
	private void AddWaypoint(Vector3 waypoint, bool additive)
	{
		if (OnAddWaypoint != null)
		{
			OnAddWaypoint(waypoint, additive);
		}
	}
	
	private void OnGUI()
	{
		if (!sSelected.Contains(this.gameObject))
			return;
		
		if (Event.current.type == EventType.MouseDown)
		{
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo))
			{
				// Only treat clicks as navigation orders when clicking on terrain.
				if (hitInfo.collider is TerrainCollider)
				{
					mClickInProgress = true;
					mClickPoint = Event.current.mousePosition;
				}
			}
		}
		else if ((Event.current.type == EventType.MouseUp) && mClickInProgress)
		{
			float mouseMove = (Event.current.mousePosition - mClickPoint).magnitude;
			if (mouseMove < 5.0f)
			{
				Vector3 screenPoint = new Vector3(mClickPoint.x, Camera.main.pixelHeight - mClickPoint.y, 0.0f);
				Ray mouseRay = Camera.main.ScreenPointToRay(screenPoint);
				RaycastHit hitInfo;
				if (Physics.Raycast(mouseRay, out hitInfo))
				{
					bool additive = (Event.current.modifiers == EventModifiers.Shift);
					AddWaypoint(hitInfo.point, additive);
				}
			}
		}
	}
	
	void OnMouseUpAsButton()
	{
		bool multiselect = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
		if (!multiselect)
		{
			sSelected.Clear();
		}
		sSelected.Add(this.gameObject);
	}
}
