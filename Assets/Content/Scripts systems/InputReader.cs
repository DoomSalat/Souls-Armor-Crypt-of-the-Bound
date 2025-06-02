using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
	private MainInputSystem _inputActions;

	public MainInputSystem InputActions { get => _inputActions ??= new MainInputSystem(); }

	private void OnEnable()
	{
		Enable();
	}

	private void OnDisable()
	{
		Disable();
	}

	public void Disable()
	{
		InputActions.Disable();
	}

	public void Enable()
	{
		InputActions.Enable();
	}
}
