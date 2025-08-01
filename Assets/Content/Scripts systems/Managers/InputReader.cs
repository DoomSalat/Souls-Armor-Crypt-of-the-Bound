using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
	private const float SwordInputMagnitudeThreshold = 0.01f;

	[SerializeField, Required] private FixedJoystick _joystick;
	private MainInputSystem _inputActions;
	private Vector2 _swordInputVector;

	public MainInputSystem InputActions { get => _inputActions ??= new MainInputSystem(); }
	public Vector2 JoystickInput => GetJoystickInput();

	private void OnEnable()
	{
		if (_inputActions != null)
		{
			_inputActions.Player.Sword.performed += OnSwordPerformed;
			_inputActions.Player.Sword.canceled += OnSwordCanceled;
		}
		Enable();
	}

	private void OnDisable()
	{
		if (_inputActions != null)
		{
			_inputActions.Player.Sword.performed -= OnSwordPerformed;
			_inputActions.Player.Sword.canceled -= OnSwordCanceled;
		}
		Disable();
	}

	public void Disable()
	{
		InputActions.Disable();
		if (_joystick != null)
			_joystick.gameObject.SetActive(false);
	}

	public void Enable()
	{
		InputActions.Enable();
		_joystick.gameObject.SetActive(true);
	}

	private Vector2 GetJoystickInput()
	{
		if (_swordInputVector.magnitude > SwordInputMagnitudeThreshold)
		{
			return _swordInputVector;
		}

		return new Vector2(_joystick.Horizontal, _joystick.Vertical);
	}

	private void OnSwordPerformed(InputAction.CallbackContext context)
	{
		_swordInputVector = context.ReadValue<Vector2>();
	}

	private void OnSwordCanceled(InputAction.CallbackContext context)
	{
		_swordInputVector = Vector2.zero;
	}
}
