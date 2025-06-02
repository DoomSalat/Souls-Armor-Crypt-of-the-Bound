using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class AbsorptionState : PlayerState
{
	private readonly AbsorptionScopeController _absorptionScopeController;
	private readonly AbsorptionScope _absorptionScope;
	private readonly InputReader _inputReader;
	private ISoul _currentSoul;
	private MonoBehaviour _coroutineRunner;

	public event System.Action AbsorptionCompleted;

	public AbsorptionState(AbsorptionScopeController absorptionScopeController, AbsorptionScope absorptionScope, InputReader inputReader)
	{
		_absorptionScopeController = absorptionScopeController;
		_absorptionScope = absorptionScope;
		_inputReader = inputReader;
		_coroutineRunner = absorptionScopeController;
	}

	public override void Enter()
	{
		_absorptionScopeController.Activate();
		_absorptionScope.SoulFounded += OnSoulFounded;
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_absorptionScopeController.FindSoul();
	}

	public override void Exit()
	{
		_absorptionScope.SoulFounded -= OnSoulFounded;
	}

	private void OnSoulFounded(ISoul soul)
	{
		_currentSoul = soul;

		//Debug.Log($"Soul founded: {soul}");

		if (soul == null)
		{
			AbsorptionCompleted?.Invoke();
			return;
		}

		_inputReader.Disable();

		_coroutineRunner.StartCoroutine(StartAbsorptionProcess());
	}

	private IEnumerator StartAbsorptionProcess()
	{
		yield return new WaitForSeconds(2f);

		AbsorptionCompleted?.Invoke();
	}
}