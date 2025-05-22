using UnityEngine;
using UnityEngine.InputSystem;

public class MouseFollower : MonoBehaviour
{
	private Camera _camera;

	private void Awake()
	{
		_camera = Camera.main;
	}

	private void Update()
	{
		if (Mouse.current.leftButton.isPressed)
		{
			Vector3 mouseScreenPos = InputReader.GetMousePosition();
			Vector3 worldPos = _camera.ScreenToWorldPoint(mouseScreenPos);
			transform.position = worldPos;
		}
	}
}
