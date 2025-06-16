using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateMachine))]
public class Player : MonoBehaviour, IDamagable
{
	[SerializeField, Required] private InputMove _inputMove;
	[SerializeField, Required] private AbsorptionScopeController _absorptionScopeController;
	[SerializeField, Required] private AbsorptionScope _absorptionScope;
	[SerializeField, Required] private SwordController _swordController;
	[SerializeField, Required] private PlayerLimbs _limbsState;
	[SerializeField, Required] private PlayerKnightAnimator _playerKnightAnimator;

	private InputReader _inputReader;
	private PlayerStateMachine _stateMachine;

	private void Awake()
	{
		_inputReader = _inputMove.InputReader;
		_stateMachine = GetComponent<PlayerStateMachine>();
		_stateMachine.InitializeStates(_inputMove, _swordController, _absorptionScopeController, _absorptionScope, _inputReader, _playerKnightAnimator, _limbsState);
	}

	private void Start()
	{
		EnterMovementState();
	}

	private void OnEnable()
	{
		_inputReader.InputActions.Player.Mouse.performed += OnMousePerformed;
		_inputReader.InputActions.Player.Mouse.canceled += OnMouseCanceled;

		_absorptionScopeController.Activated += EnterAbsorptionState;
		_stateMachine.GetState<AbsorptionState>().AbsorptionCompleted += EnterMovementState;

		_limbsState.Dead += HandleDeath;
		_limbsState.BodyLosted += HandleBodyLoss;
	}

	private void OnDisable()
	{
		_inputReader.InputActions.Player.Mouse.performed -= OnMousePerformed;
		_inputReader.InputActions.Player.Mouse.canceled -= OnMouseCanceled;

		_absorptionScopeController.Activated -= EnterAbsorptionState;
		_stateMachine.GetState<AbsorptionState>().AbsorptionCompleted -= EnterMovementState;

		_limbsState.Dead -= HandleDeath;
		_limbsState.BodyLosted -= HandleBodyLoss;
	}

	public void TakeDamage(DamageData damageData)
	{
		Debug.Log($"Take damage: {gameObject.name}");
		_limbsState.TakeDamage();
	}

	public void EnterAbsorptionState()
	{
		_stateMachine.EnterAbsorptionState();
	}

	public void EnterMovementState()
	{
		_stateMachine.EnterMovementState();
	}

	private void OnMousePerformed(InputAction.CallbackContext context)
	{
		ChooseCurrentState();
		_stateMachine.OnMousePerformed(context);
	}

	private void ChooseCurrentState()
	{
		if (_absorptionScopeController.IsPointInActivationZone())
			EnterAbsorptionState();
		else
			EnterMovementState();
	}

	private void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_stateMachine.OnMouseCanceled(context);
	}

	private void HandleDeath()
	{
		Debug.Log("Игрок умер!");
	}

	private void HandleBodyLoss()
	{

	}
}
