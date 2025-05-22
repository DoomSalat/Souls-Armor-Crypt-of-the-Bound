using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateMachine))]
public class Player : MonoBehaviour, IDamagable
{
	[SerializeField, Required] private InputMove _inputMove;
	[SerializeField, Required] private StepsMove _stepsMove;
	[SerializeField, Required] private AbsorptionScopeController _absorptionScope;
	[SerializeField, Required] private SwordController _swordController;

	private InputReader _inputReader;
	private PlayerStateMachine _stateMachine;

	private void Awake()
	{
		_inputReader = _inputMove.InputReader;
		_stateMachine = GetComponent<PlayerStateMachine>();
		_stateMachine.InitializeStates(_stepsMove, _swordController, _absorptionScope);
	}

	private void Start()
	{
		EnterMovementState();
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
		ChooseCurrentState();
		_stateMachine.OnMousePerformed(context);
	}

	private void ChooseCurrentState()
	{
		if (_absorptionScope.IsPointInActivationZone())
			EnterAbsorptionState();
		else
			EnterMovementState();
	}

	private void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_stateMachine.OnMouseCanceled(context);
	}

	public void TakeDamage(DamageData damageData)
	{
		Debug.Log($"Take damage: {gameObject.name}");
	}

	public void EnterAbsorptionState()
	{
		_stateMachine.EnterAbsorptionState();
	}

	public void EnterMovementState()
	{
		_stateMachine.EnterMovementState();
	}
}
