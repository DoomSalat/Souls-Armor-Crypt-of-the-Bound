using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
	private PlayerState _currentState;
	private Dictionary<System.Type, PlayerState> _states;

	public event System.Action StatesInitialized;

	private void Update()
	{
		_currentState?.Update();
	}

	private void FixedUpdate()
	{
		_currentState?.FixedUpdate();
	}

	public void InitializeStates(InputMove inputMove,
								SwordController swordController,
								AbsorptionScopeController absorptionScopeController,
								AbsorptionScope absorptionScope,
								InputReader inputReader,
								PlayerKnightAnimator playerKnightAnimator,
								PlayerLimbs playerLimbs,
								Transform soulAbsorptionTarget)
	{
		_states = new Dictionary<System.Type, PlayerState>
		{
			{ typeof(MovementState), new MovementState(playerKnightAnimator, inputMove, swordController, inputReader) },
			{ typeof(AbsorptionState), new AbsorptionState(playerKnightAnimator, absorptionScopeController, absorptionScope, inputReader, playerLimbs, soulAbsorptionTarget) }
		};

		StatesInitialized?.Invoke();
	}

	public void SetState<T>() where T : PlayerState
	{
		if (_currentState is T)
			return;

		var stateType = typeof(T);

		if (_states.TryGetValue(stateType, out var newState))
		{
			_currentState?.Exit();
			_currentState = newState;
			_currentState?.Enter();

			//Debug.Log($"State changed to {stateType}");
		}
		else
		{
			Debug.LogError($"State of type {stateType} not found in states dictionary");
		}
	}

	public PlayerState GetState() => _currentState;

	public T GetState<T>() where T : PlayerState
	{
		return _states[typeof(T)] as T;
	}

	public void OnMousePerformed(InputAction.CallbackContext context)
	{
		_currentState?.OnMousePerformed(context);
	}

	public void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_currentState?.OnMouseCanceled(context);
	}

	public void EnterMovementState()
	{
		SetState<MovementState>();
	}

	public void EnterAbsorptionState()
	{
		SetState<AbsorptionState>();
	}

	private void SetState(PlayerState newState)
	{
		_currentState?.Exit();
		_currentState = newState;
		_currentState.Enter();
	}
}