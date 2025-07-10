using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateMachine))]
public class Player : MonoBehaviour, IDamageable
{
	[SerializeField, Required] private InputMove _inputMove;
	[SerializeField, Required] private AbsorptionScopeController _absorptionScopeController;
	[SerializeField, Required] private AbsorptionScope _absorptionScope;
	[SerializeField, Required] private AbsorptionCooldown _absorptionCooldown;
	[SerializeField, Required] private SwordController _swordController;
	[SerializeField, Required] private PlayerLimbs _limbsState;
	[SerializeField, Required] private PlayerKnightAnimator _playerKnightAnimator;
	[SerializeField, Required] private PlayerHandsTarget _playerHandsTarget;
	[Space]
	[SerializeField, Required] private Transform _soulAbsorptionTarget;

	private InputReader _inputReader;
	private PlayerStateMachine _stateMachine;

	private bool _isHead;

	private void Awake()
	{
		_inputReader = _inputMove.InputReader;
		_stateMachine = GetComponent<PlayerStateMachine>();
		_stateMachine.InitializeStates(_inputMove,
										_swordController,
										_absorptionScopeController,
										_absorptionScope,
										_inputReader,
										_playerKnightAnimator,
										_limbsState,
										_soulAbsorptionTarget,
										_absorptionCooldown,
										_playerHandsTarget);
	}

	private void Start()
	{
		EnterMovementState();
	}

	private void OnEnable()
	{
		_inputReader.InputActions.Player.Mouse.performed += OnMousePerformed;
		_inputReader.InputActions.Player.Mouse.canceled += OnMouseCanceled;

		_absorptionScopeController.Activated += EnterAbsorptionStateClick;
		_stateMachine.GetState<AbsorptionState>().AbsorptionCompleted += EnterMovementState;

		_limbsState.Dead += HandleDeath;
		_limbsState.BodyLosted += HandleBodyLoss;
		_limbsState.LegsLosted += HandleLegsLoss;
		_limbsState.LegsRestored += HandleLegsRestore;
	}

	private void OnDisable()
	{
		_inputReader.InputActions.Player.Mouse.performed -= OnMousePerformed;
		_inputReader.InputActions.Player.Mouse.canceled -= OnMouseCanceled;

		_absorptionScopeController.Activated -= EnterAbsorptionStateClick;
		_stateMachine.GetState<AbsorptionState>().AbsorptionCompleted -= EnterMovementState;

		_limbsState.Dead -= HandleDeath;
		_limbsState.BodyLosted -= HandleBodyLoss;
		_limbsState.LegsLosted -= HandleLegsLoss;
		_limbsState.LegsRestored -= HandleLegsRestore;
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

	public void EnterMovementHeadState()
	{
		_stateMachine.EnterMovementHeadState();
	}

	private void EnterAbsorptionStateClick()
	{
		if (_isHead)
			return;

		EnterAbsorptionState();
	}

	private void OnMousePerformed(InputAction.CallbackContext context)
	{
		if (_isHead)
			return;

		ChooseCurrentState();
		_stateMachine.OnMousePerformed(context);
	}

	private void ChooseCurrentState()
	{
		if (_absorptionScopeController.IsPointInActivationZone() && _absorptionCooldown.IsOnCooldown == false)
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
		Debug.Log("Player died!");
	}

	private void HandleBodyLoss()
	{
		_isHead = true;
		EnterMovementHeadState();
	}

	private void HandleLegsLoss()
	{
		_playerKnightAnimator.FallLegs();
	}

	private void HandleLegsRestore()
	{
		_playerKnightAnimator.GetUpLegs();
	}
}
