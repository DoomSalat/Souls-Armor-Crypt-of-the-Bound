using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class InputMove : MonoBehaviour
{
	[SerializeField, MinValue(0)] private float _speed = 3;
	[SerializeField] private InputReader _inputReader;

	private Rigidbody2D _rigidbody;
	private Vector2 _moveInput;

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	private void OnEnable()
	{
		_inputReader.InputActions.Player.Move.performed += OnMovePerformed;
		_inputReader.InputActions.Player.Move.canceled += OnMoveCanceled;
	}

	private void OnDisable()
	{
		_inputReader.InputActions.Player.Move.performed -= OnMovePerformed;
		_inputReader.InputActions.Player.Move.canceled -= OnMoveCanceled;
	}

	public void Move()
	{
		_rigidbody.linearVelocity = _moveInput * _speed;
	}

	private void OnMovePerformed(InputAction.CallbackContext context)
	{
		_moveInput = context.ReadValue<Vector2>().normalized;
	}

	private void OnMoveCanceled(InputAction.CallbackContext context)
	{
		_moveInput = Vector2.zero;
	}
}
