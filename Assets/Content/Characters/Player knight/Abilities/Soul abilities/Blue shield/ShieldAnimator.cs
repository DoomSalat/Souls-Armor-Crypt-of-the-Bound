using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ShieldAnimator : MonoBehaviour
{
	private Animator _animator;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void Activate()
	{
		_animator.SetTrigger(ShieldAnimatorData.Params.Activate);
	}

	public void Deactivate()
	{
		_animator.SetTrigger(ShieldAnimatorData.Params.Deactivate);
	}

	public void Defend()
	{
		_animator.SetTrigger(ShieldAnimatorData.Params.Defend);
	}
}
