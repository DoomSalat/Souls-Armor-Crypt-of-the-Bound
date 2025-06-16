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
	private ISoul _currentSoul;
	private MonoBehaviour _coroutineRunner;
	private bool _waitingForSoulTargeted = false;
	private bool _waitingForInventoryCompletion = false;

	public event System.Action AbsorptionCompleted;

	public AbsorptionState(PlayerKnightAnimator playerKnightAnimator, AbsorptionScopeController absorptionScopeController, AbsorptionScope absorptionScope, InputReader inputReader, PlayerLimbs playerLimbs)
	{
		_animator = playerKnightAnimator;
		_absorptionScopeController = absorptionScopeController;
		_absorptionScope = absorptionScope;
		_inputReader = inputReader;
		_playerLimbs = playerLimbs;
		_coroutineRunner = absorptionScopeController;
	}

	public override void Enter()
	{
		_absorptionScopeController.Activate();
		_absorptionScope.SoulFounded += OnSoulFounded;
		_absorptionScope.SoulTargeted += OnSoulTargeted;
		_animator.AbdorptionAnimationEnded += OnAbsorptionAnimationEnded;

		_waitingForSoulTargeted = false;

		_animator.SetCapture(false);
		_animator.AbdorptionActive();
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_absorptionScopeController.StartSoulSearch();
	}

	public override void Exit()
	{
		_absorptionScope.SoulFounded -= OnSoulFounded;
		_absorptionScope.SoulTargeted -= OnSoulTargeted;
		_animator.AbdorptionAnimationEnded -= OnAbsorptionAnimationEnded;
		_playerLimbs.InventoryController.InventorySoul.SoulPlaced -= OnInventoryCompleted;

		_waitingForSoulTargeted = false;
		_animator.SetCapture(false);
	}

	private void OnSoulFounded(ISoul soul)
	{
		_currentSoul = soul;

		//Debug.Log($"Soul founded: {soul}");

		if (soul == null)
		{
			_animator.AbdorptionDeactive();
			return;
		}

		_animator.SetCapture(true);

		_waitingForSoulTargeted = true;
		_inputReader.Disable();
	}

	private void OnSoulTargeted()
	{
		if (!_waitingForSoulTargeted || _currentSoul == null)
			return;

		_waitingForSoulTargeted = false;
		_coroutineRunner.StartCoroutine(StartAbsorptionProcess());
	}

	private IEnumerator StartAbsorptionProcess()
	{
		yield return new WaitForSeconds(2f);
		_absorptionScope.Hide();

		_playerLimbs.ActivateInventory();

		_waitingForInventoryCompletion = true;
		_playerLimbs.InventoryController.InventorySoul.SoulPlaced += OnInventoryCompleted;

		yield return new WaitUntil(() => _waitingForInventoryCompletion == false);

		_animator.AbdorptionDeactive();
	}

	private void OnInventoryCompleted(LimbType limbType, SoulType soulType)
	{
		_playerLimbs.InventoryController.InventorySoul.SoulPlaced -= OnInventoryCompleted;
		_waitingForInventoryCompletion = false;
	}

	private void OnAbsorptionAnimationEnded()
	{
		AbsorptionCompleted?.Invoke();
	}
}