using UnityEngine;
using UnityEngine.InputSystem;

public class MovementState : PlayerState
{
	private readonly StepsMove _movement;
	private readonly SwordController _swordController;

	public MovementState(StepsMove movement, SwordController swordController)
	{
		_movement = movement;
		_swordController = swordController;
	}

	public override void FixedUpdate()
	{
		_movement.Move();
	}

	public override void Exit()
	{
		_movement.Stop();
		_swordController.Deactivate();
	}

	public override void OnMousePerformed(InputAction.CallbackContext context)
	{
		_swordController.Activate();
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_swordController.Deactivate();
	}
}