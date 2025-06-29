using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Animator))]
public class AbsorptionCooldownAnimator : MonoBehaviour
{
	private Animator _animator;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
	}

	public void PlayAppear()
	{
		_animator.SetTrigger(AbsorptionCooldownAnimatorData.Params.Appear);
	}

	public void PlayDisappear()
	{
		_animator.SetTrigger(AbsorptionCooldownAnimatorData.Params.Disappear);
	}
}
