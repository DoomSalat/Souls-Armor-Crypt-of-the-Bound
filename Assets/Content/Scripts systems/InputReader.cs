using UnityEngine;

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
}
