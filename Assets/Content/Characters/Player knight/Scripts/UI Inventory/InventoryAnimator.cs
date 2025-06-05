using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InventoryAnimator : MonoBehaviour
{
	private Animator _animator;

	public event System.Action DeactivateAnimationEnded;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void Activate()
	{
		_animator.SetTrigger(InventoryAnimatorData.Params.Activate);
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
