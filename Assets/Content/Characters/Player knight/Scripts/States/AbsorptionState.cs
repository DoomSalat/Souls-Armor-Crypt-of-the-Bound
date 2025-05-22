using UnityEngine;
using UnityEngine.InputSystem;

public class AbsorptionState : PlayerState
{
	private readonly SwordController _swordController;
	private readonly AbsorptionScopeController _absorptionScope;

	public AbsorptionState(Player player, SwordController swordController, AbsorptionScopeController absorptionScope) : base(player)
	{
		_swordController = swordController;
		_absorptionScope = absorptionScope;
	}

	public override void Enter()
	{
		_swordController.Deactivate();
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_absorptionScope.OnMouseClickCanceled(context);
	}
}