using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
	private MainInputSystem _inputActions;

	public MainInputSystem InputActions { get => _inputActions ??= new MainInputSystem(); }

	private void OnEnable()
	{
		InputActions.Enable();
	}

	private void OnDisable()
	{
		InputActions.Disable();
	}

	public static Vector3 GetMousePosition()
	{
		Vector2 mousePosition = Mouse.current.position.ReadValue();
		mousePosition.x = Mathf.Clamp(mousePosition.x, 0, Screen.width);
		mousePosition.y = Mathf.Clamp(mousePosition.y, 0, Screen.height);
		Vector3 mousePosition3D = new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane);

		return mousePosition3D;
	}
}
