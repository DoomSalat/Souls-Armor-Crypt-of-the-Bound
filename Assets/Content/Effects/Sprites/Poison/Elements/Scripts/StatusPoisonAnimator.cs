using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StatusPoisonAnimator : MonoBehaviour
{
	[SerializeField] private Animator _animator;

	public void PlayPoisonAnimation()
	{
		_animator.SetTrigger(StatusPoisonAnimatorData.Params.Play);
	}
}
