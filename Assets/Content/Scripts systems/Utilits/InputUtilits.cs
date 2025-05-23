using UnityEngine;
using UnityEngine.InputSystem;

public static class InputUtilits
{
	public static Vector3 GetMouseClampPosition()
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		mousePosition.x = Mathf.Clamp(mousePosition.x, 0, Screen.width);
		mousePosition.y = Mathf.Clamp(mousePosition.y, 0, Screen.height);
		Vector3 mousePosition3D = new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane);

		return mousePosition3D;
	}
}
