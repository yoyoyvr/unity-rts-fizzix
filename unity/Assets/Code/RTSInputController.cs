using UnityEngine;

// An RTS input controller provides a destination based on user input.
[AddComponentMenu("Character/RTS Input Controller")]
public class RTSInputController : RTSControllerBase
{
	private Vector2 mClickPoint = Vector2.zero;
	
	private void OnGUI()
	{
		if (Event.current.type == EventType.MouseDown)
		{
			mClickPoint = Event.current.mousePosition;
		}
		else if (Event.current.type == EventType.MouseUp)
		{
			float mouseMove = (Event.current.mousePosition - mClickPoint).magnitude;
			if (mouseMove < 5.0f)
			{
				Vector3 screenPoint = new Vector3(mClickPoint.x, Camera.main.pixelHeight - mClickPoint.y, 0.0f);
				Ray mouseRay = Camera.main.ScreenPointToRay(screenPoint);
				RaycastHit hitInfo;
				if (Physics.Raycast(mouseRay, out hitInfo))
				{
					bool addPoint = (Event.current.modifiers == EventModifiers.Shift);
					SetWaypoint(hitInfo.point, addPoint);
				}
			}
		}
	}
}
