using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
	private AbsorptionScopeController _absorptionScopeController;
	private PlayerStateMachine _stateMachine;
	private InputReader _inputReader;

	public event Action<InputAction.CallbackContext> MousePerformed;
	public event Action<InputAction.CallbackContext> MouseCanceled;
	public event Action AbsorptionActivated;

	private bool _isInitialized = false;

	private void OnEnable()
	{
		if (_isInitialized == false)
			return;

		_inputReader.InputActions.Player.Mouse.performed += OnMousePerformed;
		_inputReader.InputActions.Player.Mouse.canceled += OnMouseCanceled;

		_absorptionScopeController.Activated += OnAbsorptionActivated;
	}

	private void OnDisable()
	{
		_inputReader.InputActions.Player.Mouse.performed -= OnMousePerformed;
		_inputReader.InputActions.Player.Mouse.canceled -= OnMouseCanceled;

		_absorptionScopeController.Activated -= OnAbsorptionActivated;
	}

	public void Initialize(PlayerStateMachine stateMachine, InputMove inputMove, AbsorptionScopeController absorptionScopeController)
	{
		_stateMachine = stateMachine;
		_inputReader = inputMove.InputReader;
		_absorptionScopeController = absorptionScopeController;

		_isInitialized = true;
		OnEnable();
	}

	private void OnMousePerformed(InputAction.CallbackContext context)
	{
		if (_stateMachine.IsCurrentState<MovementHeadState>())
			return;

		MousePerformed?.Invoke(context);
	}

	private void OnMouseCanceled(InputAction.CallbackContext context)
	{
		MouseCanceled?.Invoke(context);
	}

	private void OnAbsorptionActivated()
	{
		if (_stateMachine.IsCurrentState<MovementHeadState>())
			return;

		AbsorptionActivated?.Invoke();
	}
}