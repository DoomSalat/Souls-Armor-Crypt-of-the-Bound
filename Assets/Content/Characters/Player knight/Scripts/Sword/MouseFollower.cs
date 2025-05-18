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
			Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
			Vector3 worldPos = _camera.ScreenToWorldPoint(mouseScreenPos);
			worldPos.z = 0f;
			transform.position = worldPos;
		}
	}
}
