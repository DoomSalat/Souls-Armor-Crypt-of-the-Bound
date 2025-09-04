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
	[SerializeField, Required] private TimeController _timeController;
	[Space]
	[SerializeField, Required] private PlayerInputHandler _inputHandler;
	[SerializeField, Required] private PlayerDamage _damageHandler;
	[SerializeField, Required] private AbilityInitializer _abilityInitializer;
	[Space]
	[SerializeField, Required] private Transform _soulAbsorptionTarget;
	[SerializeField, Required] private Transform _cutsceneSwordTarget;

	[Header("Colliders")]
	[SerializeField, Required] private Collider2D _bodyCollider;
	[SerializeField, Required] private HurtBox _hurtBoxCollider;

	[Header("Debug")]
	[SerializeField] private bool _skipCutscene = false;

	private PlayerStateMachine _stateMachine;

	private void Awake()
	{
		_stateMachine = GetComponent<PlayerStateMachine>();

		_limbsState.Initialize();
		_abilityInitializer.Initialize(_limbsState);
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
										_playerHandsTarget,
									_abilityInitializer,
									_timeController,
									_damageHandler,
									_cutsceneSwordTarget);
	}

	private void Start()
	{
		if (_skipCutscene)
		{
			EnterMovementState();
		}
		else
		{
			EnterCutsceneState();
		}
	}

	private void OnEnable()
	{
		_inputHandler.MousePerformed += OnMousePerformed;
		_inputHandler.MouseCanceled += OnMouseCanceled;
		_inputHandler.AbsorptionActivated += OnAbsorptionActivated;

		_damageHandler.Died += HandleDead;
		_damageHandler.BodyLost += HandleBodyLost;
		_damageHandler.DamageTaken += OnDamageTaken;

		_stateMachine.GetState<AbsorptionState>().AbsorptionCompleted += EnterMovementState;
		_stateMachine.GetState<CutsceneState>().CutsceneCompleted += EnterMovementState;
	}

	private void OnDisable()
	{
		_inputHandler.MousePerformed -= OnMousePerformed;
		_inputHandler.MouseCanceled -= OnMouseCanceled;
		_inputHandler.AbsorptionActivated -= OnAbsorptionActivated;

		_damageHandler.Died -= HandleDead;
		_damageHandler.BodyLost -= HandleBodyLost;
		_damageHandler.DamageTaken -= OnDamageTaken;

		_stateMachine.GetState<AbsorptionState>().AbsorptionCompleted -= EnterMovementState;
		_stateMachine.GetState<CutsceneState>().CutsceneCompleted -= EnterMovementState;
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

	public void EnterCutsceneState()
	{
		_stateMachine.EnterCutsceneState();
	}

	private void OnMousePerformed(InputAction.CallbackContext context)
	{
		if (_stateMachine.IsCurrentState<CutsceneState>() == false)
		{
			ChooseCurrentState();
		}

		_stateMachine.OnMousePerformed(context);
	}

	private void OnDamageTaken(DamageData damageData)
	{
		_stateMachine.OnDamageTaken(damageData);
	}

	private void ChooseCurrentState()
	{
		if (_absorptionScopeController.IsPointInActivationZone())
		{
			if (_absorptionCooldown.IsOnCooldown == false)
			{
				EnterAbsorptionState();
			}
			else
			{
				EnterCutsceneState();
			}
		}
		else
		{
			EnterMovementState();
		}
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
		if (_stateMachine.IsCurrentState<CutsceneState>())
			return;

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
