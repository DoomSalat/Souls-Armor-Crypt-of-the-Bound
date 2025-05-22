using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamagable
{
	[SerializeField, Required] private StepsMove _movement;
	[SerializeField, Required] private InputMove _inputMove;
	[SerializeField, Required] private AbsorptionScopeController _absorptionScope;
	[SerializeField, Required] private SwordController _swordController;

	private InputReader _inputReader;

	private PlayerState _currentState;
	private MovementState _movementState;
	private AbsorptionState _absorptionState;

	private void Awake()
	{
		_inputReader = _inputMove.InputReader;

		InitializeStates();
		SetState(_movementState);
	}

	private void OnEnable()
	{
		_inputReader.InputActions.Player.Mouse.performed += OnMousePerformed;
		_inputReader.InputActions.Player.Mouse.canceled += OnMouseCanceled;

		_absorptionScope.OnActivated += EnterAbsorptionState;
		_absorptionScope.OnDeactivated += EnterMovementState;
	}

	private void OnDisable()
	{
		_inputReader.InputActions.Player.Mouse.performed -= OnMousePerformed;
		_inputReader.InputActions.Player.Mouse.canceled -= OnMouseCanceled;

		_absorptionScope.OnActivated -= EnterAbsorptionState;
		_absorptionScope.OnDeactivated -= EnterMovementState;
	}

	private void OnMousePerformed(InputAction.CallbackContext context)
	{
		_currentState?.OnMousePerformed(context);
	}

	private void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_currentState?.OnMouseCanceled(context);
	}

	private void InitializeStates()
	{
		_movementState = new MovementState(this, _movement, _swordController, _absorptionScope);
		_absorptionState = new AbsorptionState(this, _swordController, _absorptionScope);
	}

	private void Update()
	{
		_currentState?.Update();
	}

	private void FixedUpdate()
	{
		_currentState?.FixedUpdate();
	}

	public void SetState(PlayerState newState)
	{
		_currentState?.Exit();
		_currentState = newState;
		_currentState?.Enter();
	}

	public void TakeDamage(DamageData damageData)
	{
		Debug.Log($"Take damage: {gameObject.name}");
	}

	public void EnterAbsorptionState()
	{
		SetState(_absorptionState);
	}

	public void EnterMovementState()
	{
		SetState(_movementState);
	}
}
