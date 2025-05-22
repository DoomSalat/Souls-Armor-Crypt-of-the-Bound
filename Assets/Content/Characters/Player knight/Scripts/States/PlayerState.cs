using UnityEngine.InputSystem;

public abstract class PlayerState
{
	protected Player Player { get; private set; }

	public PlayerState(Player player)
	{
		Player = player;
	}

	public virtual void Update() { }
	public virtual void FixedUpdate() { }
	public virtual void Enter() { }
	public virtual void Exit() { }
	public virtual void OnMousePerformed(InputAction.CallbackContext context) { }
	public virtual void OnMouseCanceled(InputAction.CallbackContext context) { }
}