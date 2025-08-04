using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AbsorptionState : PlayerState
{
	private readonly PlayerKnightAnimator _animator;
	private readonly AbsorptionScopeController _absorptionScopeController;
	private readonly AbsorptionScope _absorptionScope;
	private readonly InputReader _inputReader;
	private readonly PlayerLimbs _playerLimbs;
	private readonly Transform _soulAbsorptionTarget;
	private readonly AbsorptionCooldown _absorptionCooldown;
	private readonly SwordController _swordController;

	private ISoul _currentSoul;
	private MonoBehaviour _coroutineRunner;
	private bool _waitingForInventoryCompletion = false;

	private float _absorptionDelay = 0.5f;

	public event System.Action AbsorptionCompleted;
	public event System.Action AbsorptionStarted;
	public event System.Action InventoryClosed;

	public AbsorptionState(PlayerKnightAnimator playerKnightAnimator,
							AbsorptionScopeController absorptionScopeController,
							AbsorptionScope absorptionScope,
							InputReader inputReader,
							PlayerLimbs playerLimbs,
							Transform soulAbsorptionTarget,
							AbsorptionCooldown absorptionCooldown,
							SwordController swordController)
	{
		_animator = playerKnightAnimator;
		_absorptionScopeController = absorptionScopeController;
		_absorptionScope = absorptionScope;
		_inputReader = inputReader;
		_playerLimbs = playerLimbs;
		_coroutineRunner = absorptionScopeController;
		_soulAbsorptionTarget = soulAbsorptionTarget;
		_absorptionCooldown = absorptionCooldown;
		_swordController = swordController;

		var mousePos = InputUtilits.GetMouseClampPosition();
		Camera.main.ScreenToWorldPoint(mousePos);
	}

	public override void Enter()
	{
		_absorptionScopeController.Activate();
		_absorptionScope.SoulFounded += OnSoulFound;
		_absorptionScope.SoulTargeted += OnSoulTargeted;
		_animator.AbdorptionAnimationEnded += OnAbsorptionAnimationEnded;

		_animator.SetCapture(false);
		_animator.AbdorptionActive();
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_absorptionScopeController.StartSoulSearch();
	}

	public override void Exit()
	{
		_absorptionScope.SoulFounded -= OnSoulFound;
		_absorptionScope.SoulTargeted -= OnSoulTargeted;
		_animator.AbdorptionAnimationEnded -= OnAbsorptionAnimationEnded;
		_playerLimbs.InventoryController.InventorySoul.SoulPlaced -= OnInventoryCompleted;

		_animator.SetCapture(false);
	}

	private void OnSoulFound(ISoul soul)
	{
		_currentSoul = soul;

		if (soul == null)
		{
			_animator.AbdorptionDeactive();
			return;
		}

		_animator.SetCapture(true);
		_animator.AbdorptionSoulsCapture();

		AbsorptionStarted?.Invoke();

		_inputReader.Disable();
	}

	private void OnAttractionCompleted()
	{
		_coroutineRunner.StartCoroutine(StartAbsorptionProcess());
	}

	private void OnSoulTargeted()
	{
		_currentSoul.StartAttraction(_soulAbsorptionTarget, OnAttractionCompleted);
	}

	private IEnumerator StartAbsorptionProcess()
	{
		yield return new WaitForSeconds(_absorptionDelay);
		_absorptionScope.Hide();

		TimeController.Instance.StopTime();
		_playerLimbs.ActivateInventory(_currentSoul.GetSoulType());

		_waitingForInventoryCompletion = true;
		_playerLimbs.InventoryController.InventorySoul.SoulPlaced += OnInventoryCompleted;

		yield return new WaitUntil(() => _waitingForInventoryCompletion == false);

		_currentSoul.OnAbsorptionCompleted();

		_animator.AbdorptionDeactive();
	}

	private void OnInventoryCompleted(LimbType limbType, SoulType soulType)
	{
		_playerLimbs.InventoryController.InventorySoul.SoulPlaced -= OnInventoryCompleted;
		_waitingForInventoryCompletion = false;

		TimeController.Instance.ResumeTime();
		InventoryClosed?.Invoke();
	}

	private void OnAbsorptionAnimationEnded()
	{
		if (_currentSoul != null)
		{
			_absorptionCooldown.StartCooldown();
		}

		AbsorptionCompleted?.Invoke();
	}
}