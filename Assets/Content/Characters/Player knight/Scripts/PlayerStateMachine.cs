using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
	private PlayerState _currentState;
	private Dictionary<System.Type, PlayerState> _states;

	private void Update()
	{
		_currentState?.Update();
	}

	private void FixedUpdate()
	{
		_currentState?.FixedUpdate();
	}

	public void InitializeStates(StepsMove movement, SwordController swordController, AbsorptionScopeController absorptionScope)
	{
		_states = new Dictionary<System.Type, PlayerState>
		{
			{ typeof(MovementState), new MovementState(movement, swordController) },
			{ typeof(AbsorptionState), new AbsorptionState(absorptionScope) }
		};
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
}