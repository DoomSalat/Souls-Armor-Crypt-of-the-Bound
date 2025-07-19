using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateMachine))]
public class Player : MonoBehaviour
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
	[SerializeField, Required] private PlayerInputHandler _inputHandler;
	[SerializeField, Required] private PlayerDamage _damageHandler;
	[SerializeField, Required] private AbilityInitializer _abilityInitializer;
	[Space]
	[SerializeField, Required] private Transform _soulAbsorptionTarget;

	[Header("Colliders")]
	[SerializeField, Required] private Collider2D _bodyCollider;
	[SerializeField, Required] private HurtBox _hurtBoxCollider;

	private PlayerStateMachine _stateMachine;

	private void Awake()
	{
		_stateMachine = GetComponent<PlayerStateMachine>();

		_damageHandler.Initialize(_limbsState, _playerKnightAnimator, _abilityInitializer);
		_inputHandler.Initialize(_stateMachine, _inputMove, _absorptionScopeController);

		_stateMachine.InitializeStates(_inputMove,
										_swordController,
										_absorptionScopeController,
										_absorptionScope,
										_inputMove.InputReader,
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
		_inputHandler.MousePerformed += OnMousePerformed;
		_inputHandler.MouseCanceled += OnMouseCanceled;
		_inputHandler.AbsorptionActivated += OnAbsorptionActivated;

		_damageHandler.Dead += HandleDead;
		_damageHandler.BodyLost += HandleBodyLost;

		_stateMachine.GetState<AbsorptionState>().AbsorptionCompleted += EnterMovementState;
	}

	private void OnDisable()
	{
		_inputHandler.MousePerformed -= OnMousePerformed;
		_inputHandler.MouseCanceled -= OnMouseCanceled;
		_inputHandler.AbsorptionActivated -= OnAbsorptionActivated;

		_damageHandler.Dead -= HandleDead;
		_damageHandler.BodyLost -= HandleBodyLost;

		_stateMachine.GetState<AbsorptionState>().AbsorptionCompleted -= EnterMovementState;
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

	public void EnterEmptyState()
	{
		_stateMachine.EnterEmptyState();
	}

	private void OnMousePerformed(InputAction.CallbackContext context)
	{
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

	private void DisableColliders()
	{
		_bodyCollider.enabled = false;
		_hurtBoxCollider.gameObject.SetActive(false);
	}

	private void EnableColliders()
	{
		_bodyCollider.enabled = true;
		_hurtBoxCollider.gameObject.SetActive(true);
	}

	private void OnAbsorptionActivated()
	{
		EnterAbsorptionState();
	}

	private void HandleDead()
	{
		DisableColliders();
		EnterEmptyState();
	}

	private void HandleBodyLost()
	{
		EnterMovementHeadState();
	}
}
