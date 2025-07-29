using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RedExploseAnimator : MonoBehaviour
{
	[SerializeField] private Animator _animator;

	public void PlayAnimation()
	{
		_animator.SetTrigger(SimpleExploseAnimationData.Params.Play);
	}
}
