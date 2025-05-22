using UnityEngine;
using UnityEngine.InputSystem;

public class AbsorptionState : PlayerState
{
	private readonly AbsorptionScopeController _absorptionScope;

	public AbsorptionState(AbsorptionScopeController absorptionScope)
	{
		_absorptionScope = absorptionScope;
	}

	public override void Enter()
	{
		_absorptionScope.Activate();
	}

	public override void OnMouseCanceled(InputAction.CallbackContext context)
	{
		_absorptionScope.Deactivate();
	}
}