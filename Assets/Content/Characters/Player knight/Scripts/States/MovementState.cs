using UnityEngine;
using UnityEngine.InputSystem;

public class MovementState : PlayerState
{
	private readonly StepsMove _movement;
	private readonly SwordController _swordController;
	private readonly AbsorptionScopeController _absorptionScope;

	public MovementState(Player player, StepsMove movement, SwordController swordController, AbsorptionScopeController absorptionScope) : base(player)
	{
		_movement = movement;
		_swordController = swordController;
		_absorptionScope = absorptionScope;
	}

	public override void Enter()
	{
		_swordController.Activate();
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
		_absorptionScope.OnMouseClickPerformed(context);
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_absorptionScope.OnMouseClickCanceled(context);
	}
}