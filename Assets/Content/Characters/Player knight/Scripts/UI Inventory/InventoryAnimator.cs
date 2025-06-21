using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InventoryAnimator : MonoBehaviour
{
	private Animator _animator;
	private InventoryAnimatorEvents _events;

	public event System.Action DeactivateAnimationEnded;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_events = GetComponent<InventoryAnimatorEvents>();
	}

	public void Activate()
	{
		_animator.SetTrigger(InventoryAnimatorData.Params.Activate);
		_events.DeactivateSoul();
	}

	public void Deactivate()
	{
		_animator.SetTrigger(InventoryAnimatorData.Params.Deactivate);
	}

	public void DeactivateAnimationEnd()
	{
		DeactivateAnimationEnded?.Invoke();
	}
}
